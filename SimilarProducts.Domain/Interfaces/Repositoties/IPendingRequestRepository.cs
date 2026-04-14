using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IPendingRequestRepository : IGenericRepository<PendingRequest>
{
    Task<PendingRequest?> GetByIdWithUsersAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingRequest>> GetByStatusAsync(
        ApprovalStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingRequest>> GetByTypeAsync(
        RequestType requestType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingRequest>> GetBySubmitterIdAsync(
        int submitterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingRequest>> GetByReviewerIdAsync(
        int reviewerId,
        CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(
        CancellationToken cancellationToken = default);
}