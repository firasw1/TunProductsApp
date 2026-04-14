using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Services;

public interface IScanService
{
    Task<Product?> FindByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);

    Task<ScanFoundIn> ResolveSourceAsync(
        string barcode,
        CancellationToken cancellationToken = default);

    Task RecordScanAsync(
        string barcode,
        int? userId,
        ScanFoundIn foundIn,
        int? productId = null,
        CancellationToken cancellationToken = default);
}