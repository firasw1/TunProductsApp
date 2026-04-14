using SimilarProducts.Domain.Common;

namespace SimilarProducts.Domain.Entities;

public class Review : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }          // 1-5
    public int? QualityScore { get; set; }   // 1-5
    public int? ValueScore { get; set; }     // 1-5 (rapport qualité/prix)
    public bool? IsLike { get; set; }        // true=like, false=dislike, null=not set
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
