using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.Consumer;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? FavoriteThemeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Theme? FavoriteTheme { get; set; }
    public BusinessOwner? BusinessOwner { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
    public ICollection<PendingRequest> SubmittedRequests { get; set; } = new List<PendingRequest>();
    public ICollection<PendingRequest> ReviewedRequests { get; set; } = new List<PendingRequest>();
}
