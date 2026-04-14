using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class Theme : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ThemeType Type { get; set; } = ThemeType.System;
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double WeightStructural { get; set; } = 0.33;
    public double WeightTags { get; set; } = 0.34;
    public double WeightAi { get; set; } = 0.33;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ThemeTag> ThemeTags { get; set; } = new List<ThemeTag>();
    public ICollection<ProductSimilarity> Similarities { get; set; } = new List<ProductSimilarity>();
    public ICollection<User> FavoritedByUsers { get; set; } = new List<User>();
}

public class ThemeTag : BaseEntity
{
    public int ThemeId { get; set; }
    public int TagId { get; set; }

    // Navigation
    public Theme Theme { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
