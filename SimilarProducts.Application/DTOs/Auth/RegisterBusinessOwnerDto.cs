namespace SimilarProducts.Application.DTOs.Auth;

public class RegisterBusinessOwnerDto : RegisterDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxId { get; set; }
}