namespace SimilarProducts.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }

    public int? BrandId { get; set; }
    public string? BrandName { get; set; }

    public int SubCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    public string OriginLevel { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string? PrimaryImageUrl { get; set; }

    public List<string> Tags { get; set; } = new();
}