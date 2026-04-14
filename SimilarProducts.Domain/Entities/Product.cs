using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Entities;

public class Product : BaseEntity
{
    public int? BrandId { get; set; }
    public int SubCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? BarcodeCountry { get; set; }
    public string? Description { get; set; }
    public string? Composition { get; set; }
    public OriginLevel OriginLevel { get; set; } = OriginLevel.Imported;
    public DataSource DataSource { get; set; } = DataSource.Manual;
    public ProductStatus Status { get; set; } = ProductStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Brand? Brand { get; set; }
    public SubCategory SubCategory { get; set; } = null!;
    public ProductAiAnalysis? AiAnalysis { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public ICollection<StoreProduct> StoreProducts { get; set; } = new List<StoreProduct>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
    public ICollection<ScrapedProduct> MatchedScrapedProducts { get; set; } = new List<ScrapedProduct>();
    public ICollection<ProductSimilarity> SimilaritiesAsSource { get; set; } = new List<ProductSimilarity>();
    public ICollection<ProductSimilarity> SimilaritiesAsTarget { get; set; } = new List<ProductSimilarity>();

    // Helper: extract country from barcode prefix
    public static string? ExtractBarcodeCountry(string? barcode)
    {
        if (string.IsNullOrEmpty(barcode) || barcode.Length < 3) return null;
        var prefix = barcode[..3];
        return prefix switch
        {
            "619" => "TN",
            "611" => "MA",
            "613" => "DZ",
            "300" or "301" or "302" or "303" or "304" or "305" or "306" or "307" or "308" or "309"
                or "310" or "311" or "312" or "313" or "314" or "315" or "316" or "317" or "318" or "319"
                or "320" or "321" or "322" or "323" or "324" or "325" or "326" or "327" or "328" or "329"
                or "330" or "331" or "332" or "333" or "334" or "335" or "336" or "337" or "338" or "339"
                or "340" or "341" or "342" or "343" or "344" or "345" or "346" or "347" or "348" or "349"
                or "350" or "351" or "352" or "353" or "354" or "355" or "356" or "357" or "358" or "359"
                or "360" or "361" or "362" or "363" or "364" or "365" or "366" or "367" or "368" or "369"
                or "370" or "371" or "372" or "373" or "374" or "375" or "376" or "377" or "378" or "379" => "FR",
            "400" or "401" or "402" or "403" or "404" or "405" or "406" or "407" or "408" or "409"
                or "410" or "411" or "412" or "413" or "414" or "415" or "416" or "417" or "418" or "419"
                or "420" or "421" or "422" or "423" or "424" or "425" or "426" or "427" or "428" or "429"
                or "430" or "431" or "432" or "433" or "434" or "435" or "436" or "437" or "438" or "439"
                or "440" => "DE",
            "800" or "801" or "802" or "803" or "804" or "805" or "806" or "807" or "808" or "809"
                or "810" or "811" or "812" or "813" or "814" or "815" or "816" or "817" or "818" or "819"
                or "820" or "821" or "822" or "823" or "824" or "825" or "826" or "827" or "828" or "829"
                or "830" or "831" or "832" or "833" or "834" or "835" or "836" or "837" or "838" or "839" => "IT",
            "840" or "841" or "842" or "843" or "844" or "845" or "846" or "847" or "848" or "849" => "ES",
            "869" => "TR",
            "871" => "NL",
            _ => null
        };
    }
}
