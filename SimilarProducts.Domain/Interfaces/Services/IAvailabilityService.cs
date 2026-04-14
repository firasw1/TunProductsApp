using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Domain.Interfaces.Services;

public interface IAvailabilityService
{
    Task<IReadOnlyList<StoreProduct>> GetDeclaredAvailabilityAsync(
        int productId,
        double? userLat = null,
        double? userLng = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScrapedProduct>> GetScrapedAvailabilityAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyAvailabilityAsync(
        int productId,
        CancellationToken cancellationToken = default);
}