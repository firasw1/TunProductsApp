namespace SimilarProducts.Application.DTOs.Products;

public class UpdateProductDto
{
    public int? BrandId { get; set; }
    public int SubCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? Composition { get; set; }

    public string OriginLevel { get; set; } = "Imported";

    // null => ne pas toucher
    // []   => vider tous les tags
    public List<int>? TagIds { get; set; }
}