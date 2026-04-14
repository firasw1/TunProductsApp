using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Review?> GetByUserAndProductAsync(
        int userId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(
                r => r.UserId == userId && r.ProductId == productId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetRecentByProductIdAsync(
        int productId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        limit = limit < 1 ? 10 : limit;

        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetAverageRatingByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var average = await _dbSet
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .Select(r => (double?)r.Rating)
            .AverageAsync(cancellationToken);

        return average ?? 0d;
    }

    public async Task<int> GetCountByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId, cancellationToken);
    }

    public async Task<bool> ExistsByUserAndProductAsync(
        int userId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                r => r.UserId == userId && r.ProductId == productId,
                cancellationToken);
    }
}