using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        barcode = barcode.Trim();

        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(p => p.Barcode == barcode, cancellationToken);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetBySubCategoryAsync(
        int subCategoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        return await _dbSet
            .AsNoTracking()
            .Where(p => p.SubCategoryId == subCategoryId && p.Status == ProductStatus.Approved)
            .Include(p => p.Brand)
            .Include(p => p.Images.Where(i => i.IsPrimary).OrderBy(i => i.DisplayOrder))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<Product>();

        query = query.Trim();
        limit = limit < 1 ? 20 : limit;

        return await _dbSet
            .AsNoTracking()
            .Where(p =>
                p.Status == ProductStatus.Approved &&
                EF.Functions.Like(p.Name, $"%{query}%"))
            .Include(p => p.Brand)
            .Include(p => p.Images.Where(i => i.IsPrimary).OrderBy(i => i.DisplayOrder))
            .OrderBy(p => p.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStatusAsync(
        ProductStatus status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Status == status)
            .Include(p => p.Brand)
            .Include(p => p.Images.Where(i => i.IsPrimary).OrderBy(i => i.DisplayOrder))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByBrandAsync(
        int brandId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.BrandId == brandId)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary).OrderBy(i => i.DisplayOrder))
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        barcode = barcode.Trim();

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(p => p.Barcode == barcode, cancellationToken);
    }

    private IQueryable<Product> BuildDetailsQuery()
    {
        return _dbSet
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.AiAnalysis);
    }
}