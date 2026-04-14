using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class BusinessOwner : BaseEntity
{
    public int UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public BusinessOwnerStatus Status { get; set; } = BusinessOwnerStatus.Pending;
    public string? DocumentsUrl { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Brand> Brands { get; set; } = new List<Brand>();
    public ICollection<Store> Stores { get; set; } = new List<Store>();
}
