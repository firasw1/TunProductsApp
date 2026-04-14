using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IProductRepository Products { get; }
    public IUserRepository Users { get; }
    public IBrandRepository Brands { get; }
    public IStoreRepository Stores { get; }
    public IReviewRepository Reviews { get; }
    public IThemeRepository Themes { get; }
    public IScrapedProductRepository ScrapedProducts { get; }
    public IPendingRequestRepository PendingRequests { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;

        Products = new ProductRepository(_context);
        Users = new UserRepository(_context);
        Brands = new BrandRepository(_context);
        Stores = new StoreRepository(_context);
        Reviews = new ReviewRepository(_context);
        Themes = new ThemeRepository(_context);
        ScrapedProducts = new ScrapedProductRepository(_context);
        PendingRequests = new PendingRequestRepository(_context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}