using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Data;
using ProductApi.Infrastructure.Data.Repositories;
using Xunit;

namespace Infrastructure.Tests;

public class ProductRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPageAndTotalCount()
    {
        // Arrange
        await using var context = CreateContext();
        for (var i = 1; i <= 15; i++)
        {
            context.Products.Add(new Product
            {
                ProductName = $"Product {i}",
                CreatedBy = "seed",
                CreatedOn = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var (items, totalCount) = await repository.GetPagedAsync(pageNumber: 2, pageSize: 10, search: null);

        // Assert
        totalCount.Should().Be(15);
        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearchTerm_FiltersByProductName()
    {
        // Arrange
        await using var context = CreateContext();
        context.Products.AddRange(
            new Product { ProductName = "Blue Widget", CreatedBy = "seed", CreatedOn = DateTime.UtcNow },
            new Product { ProductName = "Red Gadget", CreatedBy = "seed", CreatedOn = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var (items, totalCount) = await repository.GetPagedAsync(pageNumber: 1, pageSize: 10, search: "Widget");

        // Assert
        totalCount.Should().Be(1);
        items.Single().ProductName.Should().Be("Blue Widget");
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_ReturnsProductWithRelatedItems()
    {
        // Arrange
        await using var context = CreateContext();
        var product = new Product { ProductName = "Kit", CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
        product.Items.Add(new Item { Quantity = 3 });
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdWithItemsAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(i => i.Quantity == 3);
    }
}
