using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetBySubCategoryAsync(
        int subCategoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchByNameAsync(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByStatusAsync(
        ProductStatus status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByBrandAsync(
        int brandId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);
}