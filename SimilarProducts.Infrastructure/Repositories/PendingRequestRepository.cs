using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class PendingRequestRepository : GenericRepository<PendingRequest>, IPendingRequestRepository
{
    public PendingRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PendingRequest?> GetByIdWithUsersAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(pr => pr.Submitter)
            .Include(pr => pr.Reviewer)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PendingRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pr => pr.Status == ApprovalStatus.Pending)
            .Include(pr => pr.Submitter)
            .Include(pr => pr.Reviewer)
            .OrderBy(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingRequest>> GetByStatusAsync(
        ApprovalStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pr => pr.Status == status)
            .Include(pr => pr.Submitter)
            .Include(pr => pr.Reviewer)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingRequest>> GetByTypeAsync(
        RequestType requestType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pr => pr.RequestType == requestType)
            .Include(pr => pr.Submitter)
            .Include(pr => pr.Reviewer)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingRequest>> GetBySubmitterIdAsync(
        int submitterId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pr => pr.SubmittedBy == submitterId)
            .Include(pr => pr.Submitter)
            .Include(pr => pr.Reviewer)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingRequest>> GetByReviewerIdAsync(
        int reviewerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pr => pr.ReviewedBy == reviewerId)
            .Include(pr => pr.Submitter)
            .Include(pr => pr.Reviewer)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(pr => pr.Status == ApprovalStatus.Pending, cancellationToken);
    }
}