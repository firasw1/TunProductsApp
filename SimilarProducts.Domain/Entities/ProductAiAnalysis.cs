using SimilarProducts.Domain.Common;

namespace SimilarProducts.Domain.Entities;

public class ProductAiAnalysis : BaseEntity
{
    public int ProductId { get; set; }
    public int BioScore { get; set; }        // 0-100
    public int NaturalScore { get; set; }    // 0-100
    public int HealthScore { get; set; }     // 0-100
    public string? AdditivesJson { get; set; }   // JSON: [{code:"E621", name:"...", risk:"high"}]
    public string? AnalysisSummary { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public string ModelVersion { get; set; } = string.Empty;

    // Navigation
    public Product Product { get; set; } = null!;
}
