using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class PendingRequest : BaseEntity
{
    public int SubmittedBy { get; set; }
    public RequestType RequestType { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? AdminNote { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public User Submitter { get; set; } = null!;
    public User? Reviewer { get; set; }
}
