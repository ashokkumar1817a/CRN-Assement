using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;
using ProductApi.Application.Mapping;
using ProductApi.Application.Services;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;
using Xunit;

namespace Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly IMapper _mapper;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Items).Returns(_itemRepositoryMock.Object);

        _sut = new ProductService(_unitOfWorkMock.Object, _mapper, Mock.Of<ILogger<ProductService>>());
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsMappedDto()
    {
        // Arrange
        var product = new Product { Id = 1, ProductName = "Widget", CreatedBy = "tester", CreatedOn = DateTime.UtcNow };
        _productRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.ProductName.Should().Be("Widget");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _sut.GetProductByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateProductAsync_PersistsProductAndReturnsDto()
    {
        // Arrange
        var dto = new CreateProductDto { ProductName = "New Widget" };
        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateProductAsync(dto, "tester");

        // Assert
        result.ProductName.Should().Be("New Widget");
        result.CreatedBy.Should().Be("tester");
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _sut.DeleteProductAsync(5);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsPagedResultWithCorrectMetadata()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, ProductName = "A", CreatedBy = "tester", CreatedOn = DateTime.UtcNow },
            new() { Id = 2, ProductName = "B", CreatedBy = "tester", CreatedOn = DateTime.UtcNow }
        };
        _productRepositoryMock
            .Setup(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _sut.GetProductsAsync(new PaginationQuery { PageNumber = 1, PageSize = 10 });

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }
}
