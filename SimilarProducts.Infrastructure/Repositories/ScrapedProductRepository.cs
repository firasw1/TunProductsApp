using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class ScrapedProductRepository : GenericRepository<ScrapedProduct>, IScrapedProductRepository
{
    public ScrapedProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ScrapedProduct?> GetByUrlSourceAsync(
        string urlSource,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(urlSource))
            return null;

        urlSource = urlSource.Trim();

        return await _dbSet
            .AsNoTracking()
            .Include(sp => sp.MatchedProduct)
                .ThenInclude(p => p!.Brand)
            .FirstOrDefaultAsync(sp => sp.UrlSource == urlSource, cancellationToken);
    }

    public async Task<IReadOnlyList<ScrapedProduct>> GetUnmatchedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(sp => sp.MatchStatus == MatchStatus.Unmatched)
            .OrderByDescending(sp => sp.ScrapedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScrapedProduct>> GetByMatchStatusAsync(
        MatchStatus matchStatus,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(sp => sp.MatchStatus == matchStatus)
            .Include(sp => sp.MatchedProduct)
            .OrderByDescending(sp => sp.ScrapedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScrapedProduct>> GetByMatchedProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(sp => sp.MatchedProductId == productId)
            .OrderByDescending(sp => sp.ScrapedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScrapedProduct>> GetRecentAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = limit < 1 ? 50 : limit;

        return await _dbSet
            .AsNoTracking()
            .Include(sp => sp.MatchedProduct)
            .OrderByDescending(sp => sp.ScrapedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUrlSourceAsync(
        string urlSource,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(urlSource))
            return false;

        urlSource = urlSource.Trim();

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(sp => sp.UrlSource == urlSource, cancellationToken);
    }
}