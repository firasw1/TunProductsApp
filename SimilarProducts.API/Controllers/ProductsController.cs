using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimilarProducts.Application.DTOs.Common;
using SimilarProducts.Application.DTOs.Products;
using SimilarProducts.Application.Services;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Liste paginée des produits avec filtres optionnels.
    /// Public.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaginatedResultDto<ProductDto>>>> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] int? brandId,
        [FromQuery] int? subCategoryId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetAllAsync(
            searchTerm,
            brandId,
            subCategoryId,
            status,
            page,
            pageSize,
            cancellationToken);

        return Ok(ApiResponse<PaginatedResultDto<ProductDto>>.Ok(
            result,
            "Produits récupérés avec succès."));
    }

    /// <summary>
    /// Détail complet d’un produit.
    /// Public.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetDetailAsync(id, cancellationToken);

        return Ok(ApiResponse<ProductDetailDto>.Ok(
            result,
            "Produit récupéré avec succès."));
    }

    /// <summary>
    /// Recherche rapide de produits.
    /// Public.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> Search(
        [FromQuery] string searchTerm,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.SearchAsync(searchTerm, limit, cancellationToken);

        return Ok(ApiResponse<List<ProductDto>>.Ok(
            result,
            "Recherche effectuée avec succès."));
    }

    /// <summary>
    /// Proposition d’un produit par un consumer.
    /// Crée une PendingRequest, pas un Product direct.
    /// </summary>
    [HttpPost("propose")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<int>>> Propose(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var requestId = await _productService.ProposeProductAsync(dto, userId, cancellationToken);

        return Ok(ApiResponse<int>.Ok(
            requestId,
            "Proposition envoyée avec succès."));
    }

    /// <summary>
    /// Création d’un produit.
    /// - Admin : création directe approuvée
    /// - BusinessOwner : création en Pending
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        ProductDetailDto result;

        if (role == UserRole.Admin)
        {
            result = await _productService.CreateAsAdminAsync(dto, cancellationToken);
        }
        else
        {
            result = await _productService.CreateAsync(dto, userId, cancellationToken);
        }

        return Ok(ApiResponse<ProductDetailDto>.Ok(
            result,
            "Produit créé avec succès."));
    }

    /// <summary>
    /// Mise à jour d’un produit.
    /// - Admin : update direct
    /// - BusinessOwner : update autorisé si la marque lui appartient
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> Update(
        int id,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        ProductDetailDto result;

        if (role == UserRole.Admin)
        {
            result = await _productService.UpdateAsAdminAsync(id, dto, cancellationToken);
        }
        else
        {
            result = await _productService.UpdateAsync(id, dto, userId, cancellationToken);
        }

        return Ok(ApiResponse<ProductDetailDto>.Ok(
            result,
            "Produit mis à jour avec succès."));
    }

    /// <summary>
    /// Suppression d’un produit.
    /// - Admin : suppression directe
    /// - BusinessOwner : suppression autorisée si la marque lui appartient
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        if (role == UserRole.Admin)
        {
            await _productService.DeleteAsAdminAsync(id, cancellationToken);
        }
        else
        {
            await _productService.DeleteAsync(id, userId, cancellationToken);
        }

        return Ok(ApiResponse<object>.Ok(
            null,
            "Produit supprimé avec succès."));
    }

    /// <summary>
    /// Ajout d’image à un produit.
    /// - Admin : direct
    /// - BusinessOwner : autorisé si la marque lui appartient
    /// </summary>
    [HttpPost("{id:int}/images")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> AddImage(
        int id,
        [FromBody] AddProductImageRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        ProductDetailDto result;

        if (role == UserRole.Admin)
        {
            result = await _productService.AddImageAsAdminAsync(
                id,
                dto.ImageUrl,
                dto.IsPrimary,
                cancellationToken);
        }
        else
        {
            result = await _productService.AddImageAsync(
                id,
                dto.ImageUrl,
                dto.IsPrimary,
                userId,
                cancellationToken);
        }

        return Ok(ApiResponse<ProductDetailDto>.Ok(
            result,
            "Image ajoutée avec succès."));
    }

    /// <summary>
    /// Suppression d’image d’un produit.
    /// - Admin : direct
    /// - BusinessOwner : autorisé si la marque lui appartient
    /// </summary>
    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> DeleteImage(
        int id,
        int imageId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        ProductDetailDto result;

        if (role == UserRole.Admin)
        {
            result = await _productService.DeleteImageAsAdminAsync(
                id,
                imageId,
                cancellationToken);
        }
        else
        {
            result = await _productService.DeleteImageAsync(
                id,
                imageId,
                userId,
                cancellationToken);
        }

        return Ok(ApiResponse<ProductDetailDto>.Ok(
            result,
            "Image supprimée avec succès."));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Token invalide : userId introuvable.");

        return userId;
    }

    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(roleClaim))
            throw new UnauthorizedAccessException("Token invalide : rôle introuvable.");

        if (!Enum.TryParse<UserRole>(roleClaim, true, out var role))
            throw new UnauthorizedAccessException("Token invalide : rôle non reconnu.");

        return role;
    }
}

/// <summary>
/// DTO temporaire pour l’ajout d’image produit.
/// À déplacer plus tard dans Application/DTOs/Products.
/// </summary>
public class AddProductImageRequestDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}