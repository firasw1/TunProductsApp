using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class BrandRepository : GenericRepository<Brand>, IBrandRepository
{
    public BrandRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Brand?> GetByIdWithProductsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(b => b.Owner)
                .ThenInclude(o => o.User)
            .Include(b => b.Products)
                .ThenInclude(p => p.Images)
            .Include(b => b.Products)
                .ThenInclude(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetByOwnerIdAsync(
        int ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.OwnerId == ownerId)
            .Include(b => b.Owner)
                .ThenInclude(o => o.User)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.IsVerified)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Trim();

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(b => b.Name == name, cancellationToken);
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
            .AnyAsync(b => b.OwnerId == ownerId && b.Name == name, cancellationToken);
    }
}