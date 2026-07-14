using ProductApi.Domain.Entities;

namespace ProductApi.Application.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
}
