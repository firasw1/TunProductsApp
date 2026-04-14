using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class Store : BaseEntity
{
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public StoreType StoreType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public BusinessOwner Owner { get; set; } = null!;
    public ICollection<StoreLocation> Locations { get; set; } = new List<StoreLocation>();
}
