using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<Review?> GetByUserAndProductAsync(
        int userId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetRecentByProductIdAsync(
        int productId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<double> GetAverageRatingByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<int> GetCountByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserAndProductAsync(
        int userId,
        int productId,
        CancellationToken cancellationToken = default);
}