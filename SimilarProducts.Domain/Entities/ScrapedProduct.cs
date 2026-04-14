using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class ScrapedProduct : BaseEntity
{
    public string StoreSource { get; set; } = string.Empty;  // mg_tunisie, monoprix, etc.
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? CategoryOnSite { get; set; }
    public string UrlSource { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;
    public int? MatchedProductId { get; set; }
    public MatchStatus MatchStatus { get; set; } = MatchStatus.Unmatched;
    public double? MatchConfidence { get; set; }

    // Navigation
    public Product? MatchedProduct { get; set; }
}
