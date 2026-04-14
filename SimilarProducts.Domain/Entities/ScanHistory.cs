using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class ScanHistory : BaseEntity
{
    public int UserId { get; set; }
    public int? ProductId { get; set; }   // null si produit non trouvé
    public string BarcodeScanned { get; set; } = string.Empty;
    public ScanFoundIn FoundIn { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Product? Product { get; set; }
}
