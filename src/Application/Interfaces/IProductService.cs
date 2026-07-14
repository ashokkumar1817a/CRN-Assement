using ProductApi.Application.DTOs;

namespace ProductApi.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, string createdBy, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, string modifiedBy, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItemDto>> GetItemsForProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<ItemDto> AddItemToProductAsync(int productId, CreateItemDto dto, CancellationToken cancellationToken = default);
}
