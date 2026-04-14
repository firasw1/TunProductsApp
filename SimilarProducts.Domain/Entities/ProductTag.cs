using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Product Product { get; set; } = null!;
}

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TagType Type { get; set; }

    // Navigation
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public ICollection<ThemeTag> ThemeTags { get; set; } = new List<ThemeTag>();
}

public class ProductTag : BaseEntity
{
    public int ProductId { get; set; }
    public int TagId { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
