using SimilarProducts.Domain.Common;

namespace SimilarProducts.Domain.Entities;

public class StoreLocation : BaseEntity
{
    public int StoreId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Phone { get; set; }
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Store Store { get; set; } = null!;
    public ICollection<StoreProduct> StoreProducts { get; set; } = new List<StoreProduct>();
}
