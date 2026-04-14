using SimilarProducts.Domain.Common;

namespace SimilarProducts.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
}

public class SubCategory : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
