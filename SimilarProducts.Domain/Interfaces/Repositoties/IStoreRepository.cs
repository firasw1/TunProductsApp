using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IStoreRepository : IGenericRepository<Store>
{
    Task<Store?> GetByIdWithLocationsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Store>> GetByOwnerIdAsync(
        int ownerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Store>> GetByTypeAsync(
        StoreType storeType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Store>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameForOwnerAsync(
        int ownerId,
        string name,
        CancellationToken cancellationToken = default);
}