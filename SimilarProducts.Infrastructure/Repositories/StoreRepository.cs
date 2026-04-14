using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class StoreRepository : GenericRepository<Store>, IStoreRepository
{
    public StoreRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Store?> GetByIdWithLocationsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(s => s.Owner)
                .ThenInclude(o => o.User)
            .Include(s => s.Locations.OrderBy(l => l.City).ThenBy(l => l.Address))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Store>> GetByOwnerIdAsync(
        int ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.OwnerId == ownerId)
            .Include(s => s.Locations)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Store>> GetByTypeAsync(
        StoreType storeType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.StoreType == storeType)
            .Include(s => s.Locations.Where(l => l.IsActive))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Store>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.Locations.Any(l => l.IsActive))
            .Include(s => s.Locations.Where(l => l.IsActive))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameForOwnerAsync(
        int ownerId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Trim();

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                s => s.OwnerId == ownerId && s.Name == name,
                cancellationToken);
    }
}