using SimilarProducts.Domain.Common;

namespace SimilarProducts.Domain.Entities;

public class Brand : BaseEntity
{
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? CountryOrigin { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public BusinessOwner Owner { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
