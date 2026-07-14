using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductApi.Application.DTOs;
using Xunit;

namespace API.Tests;

public class ProductsControllerTests : IClassFixture<ProductApiFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(ProductApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var payload = new CreateProductDto { ProductName = "Test Product" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
