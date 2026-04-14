using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IBrandRepository : IGenericRepository<Brand>
{
    Task<Brand?> GetByIdWithProductsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Brand>> GetByOwnerIdAsync(
        int ownerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Brand>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameForOwnerAsync(
        int ownerId,
        string name,
        CancellationToken cancellationToken = default);
}