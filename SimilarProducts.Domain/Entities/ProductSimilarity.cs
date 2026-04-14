using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class ProductSimilarity : BaseEntity
{
    public int ProductAId { get; set; }
    public int ProductBId { get; set; }
    public int? ThemeId { get; set; }
    public SimilaritySource Source { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public double? Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product ProductA { get; set; } = null!;
    public Product ProductB { get; set; } = null!;
    public Theme? Theme { get; set; }
}
