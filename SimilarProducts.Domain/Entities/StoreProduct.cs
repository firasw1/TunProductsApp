using SimilarProducts.Domain.Common;

namespace SimilarProducts.Domain.Entities;

public class StoreProduct : BaseEntity
{
    public int StoreLocationId { get; set; }
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public StoreLocation StoreLocation { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
