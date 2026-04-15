using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailsQuery()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<Product?> GetByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        var normalizedBarcode = barcode.Trim();

        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(p => p.Barcode == normalizedBarcode, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetBySubCategoryAsync(
        int subCategoryId,
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailsQuery()
            .Where(p => p.SubCategoryId == subCategoryId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(
        string searchTerm,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Product>();

        if (limit <= 0)
            limit = 20;

        if (limit > 100)
            limit = 100;

        var normalizedSearch = searchTerm.Trim().ToLower();

        return await BuildDetailsQuery()
            .Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                (p.Brand != null && p.Brand.Name.ToLower().Contains(normalizedSearch)) ||
                (!string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode!.ToLower().Contains(normalizedSearch)))
            .OrderBy(p => p.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStatusAsync(
        ProductStatus status,
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailsQuery()
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByBrandAsync(
        int brandId,
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailsQuery()
            .Where(p => p.BrandId == brandId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        var normalizedBarcode = barcode.Trim();

        return await _context.Products
            .AnyAsync(p => p.Barcode == normalizedBarcode, cancellationToken);
    }

    private IQueryable<Product> BuildDetailsQuery()
    {
        return _context.Products
            .Include(p => p.Brand)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Images)
            .Include(p => p.AiAnalysis)
            .Include(p => p.Reviews)
            .AsSplitQuery();
    }
}