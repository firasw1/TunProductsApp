using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Domain.Interfaces.Services;

public interface IOpenFoodFactsService
{
    Task<Product?> GetProductByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);
}