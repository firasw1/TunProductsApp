using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimilarProducts.Application.DTOs.Auth;
using SimilarProducts.Application.DTOs.Common;
using SimilarProducts.Application.Services;

namespace SimilarProducts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inscription d'un consommateur.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterConsumerAsync(dto, cancellationToken);

        return Ok(ApiResponse<TokenResponseDto>.Ok(
            result,
            "Inscription réussie."));
    }

    /// <summary>
    /// Inscription d'un business owner.
    /// </summary>
    [HttpPost("register/business")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RegisterBusiness(
        [FromBody] RegisterBusinessOwnerDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterBusinessOwnerAsync(dto, cancellationToken);

        return Ok(ApiResponse<TokenResponseDto>.Ok(
            result,
            "Compte business créé. En attente d'approbation admin."));
    }

    /// <summary>
    /// Connexion utilisateur.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);

        return Ok(ApiResponse<TokenResponseDto>.Ok(
            result,
            "Connexion réussie."));
    }

    /// <summary>
    /// Renouvelle le token JWT via le refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, cancellationToken);

        return Ok(ApiResponse<TokenResponseDto>.Ok(
            result,
            "Token renouvelé."));
    }

    /// <summary>
    /// Récupère le profil de l'utilisateur connecté.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfile>>> GetProfile(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var profile = await _authService.GetProfileAsync(userId, cancellationToken);

        return Ok(ApiResponse<UserProfile>.Ok(
            profile,
            "Profil récupéré avec succès."));
    }

    /// <summary>
    /// Met à jour le profil de l'utilisateur connecté.
    /// </summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserProfile>>> UpdateProfile(
        [FromBody] UpdateProfileRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var updatedProfile = await _authService.UpdateProfileAsync(
            userId,
            dto.FullName,
            dto.Phone,
            dto.Latitude,
            dto.Longitude,
            dto.FavoriteThemeId,
            cancellationToken);

        return Ok(ApiResponse<UserProfile>.Ok(
            updatedProfile,
            "Profil mis à jour avec succès."));
    }

    /// <summary>
    /// Change le mot de passe de l'utilisateur connecté.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await _authService.ChangePasswordAsync(
            userId,
            dto.CurrentPassword,
            dto.NewPassword,
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(
            null,
            "Mot de passe modifié avec succès."));
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
}

/// <summary>
/// DTO temporaire pour le refresh token.
/// À déplacer plus tard dans DTOs/Auth.
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO temporaire pour la mise à jour du profil.
/// À déplacer plus tard dans DTOs/Auth.
/// </summary>
public class UpdateProfileRequestDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? FavoriteThemeId { get; set; }
}

/// <summary>
/// DTO temporaire pour le changement de mot de passe.
/// À déplacer plus tard dans DTOs/Auth.
/// </summary>
public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}