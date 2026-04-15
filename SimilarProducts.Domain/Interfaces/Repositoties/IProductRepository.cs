using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithDetailsAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetBySubCategoryAsync(
        int subCategoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchByNameAsync(
        string searchTerm,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByStatusAsync(
        ProductStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByBrandAsync(
        int brandId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);
}