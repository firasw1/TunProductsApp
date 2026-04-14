using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IScrapedProductRepository : IGenericRepository<ScrapedProduct>
{
    Task<ScrapedProduct?> GetByUrlSourceAsync(
        string urlSource,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScrapedProduct>> GetUnmatchedAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScrapedProduct>> GetByMatchStatusAsync(
        MatchStatus matchStatus,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScrapedProduct>> GetByMatchedProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScrapedProduct>> GetRecentAsync(
        int limit = 50,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUrlSourceAsync(
        string urlSource,
        CancellationToken cancellationToken = default);
}