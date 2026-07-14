using Microsoft.EntityFrameworkCore;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;

namespace ProductApi.Infrastructure.Data.Repositories;

public class ItemRepository : GenericRepository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Item>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);
}
