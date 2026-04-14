using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Domain.Interfaces.Services;

public interface ISimilarityService
{
    Task<IReadOnlyList<Product>> FindSimilarAsync(
        int productId,
        int? themeId = null,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<double> CalculateSimilarityScoreAsync(
        Product sourceProduct,
        Product targetProduct,
        int? themeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> AreSimilarAsync(
        int productAId,
        int productBId,
        int? themeId = null,
        double threshold = 0.6,
        CancellationToken cancellationToken = default);
}