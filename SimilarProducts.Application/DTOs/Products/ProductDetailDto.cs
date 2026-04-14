namespace SimilarProducts.Application.DTOs.Products;

public class ProductDetailDto : ProductDto
{
    public string? BarcodeCountry { get; set; }

    public string? Description { get; set; }
    public string? Composition { get; set; }

    public List<string> ImageUrls { get; set; } = new();

    public int? BioScore { get; set; }
    public int? NaturalScore { get; set; }
    public int? HealthScore { get; set; }

    public string? AiSummary { get; set; }

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}