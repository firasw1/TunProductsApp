using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarProducts.Domain.Common;

using SimilarProducts.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IUserRepository Users { get; }
    IBrandRepository Brands { get; }
    IStoreRepository Stores { get; }
    IReviewRepository Reviews { get; }
    IThemeRepository Themes { get; }
    IScrapedProductRepository ScrapedProducts { get; }
    IPendingRequestRepository PendingRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}