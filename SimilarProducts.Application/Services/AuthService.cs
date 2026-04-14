using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SimilarProducts.Application.DTOs.Auth;
using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JwtClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
namespace SimilarProducts.Application.Services;

/// <summary>
/// Service d'authentification.
///
/// Responsabilités :
///   - Inscription consumer
///   - Inscription business owner
///   - Connexion
///   - Refresh token
///   - Gestion du profil utilisateur
///   - Vérification du statut business owner
///
/// Remarque :
///   - On garde ici la logique métier de la version d'origine.
///   - On corrige seulement ce qui est nécessaire pour l'aligner
///     avec les vraies interfaces du projet.
/// </summary>
public class AuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<BusinessOwner> _businessOwnerRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUnitOfWork unitOfWork,
        IGenericRepository<BusinessOwner> businessOwnerRepository,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _businessOwnerRepository = businessOwnerRepository;
        _configuration = configuration;
    }
    // ═══════════════════════════════════════════════════════
    // INSCRIPTION CONSUMER
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Créer un compte consommateur.
    /// Le compte est actif immédiatement — pas besoin d'approbation admin.
    /// Retourne directement un token JWT pour login automatique.
    /// </summary>
    public async Task<TokenResponseDto> RegisterConsumerAsync(
        string email,
        string password,
        string fullName,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        ValidateEmail(normalizedEmail);
        ValidatePassword(password);
        ValidateFullName(fullName);

        await EnsureEmailAvailableAsync(normalizedEmail, cancellationToken);

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = HashPassword(password),
            FullName = fullName.Trim(),
            Phone = NormalizePhone(phone),
            Role = UserRole.Consumer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GenerateTokenResponse(user);
    }

    /// <summary>
    /// Surcharge pratique basée sur RegisterDto.
    /// </summary>
    public Task<TokenResponseDto> RegisterConsumerAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        return RegisterConsumerAsync(
            dto.Email,
            dto.Password,
            dto.FullName,
            dto.Phone,
            cancellationToken);
    }

    /// <summary>
    /// Créer un compte business owner.
    /// Le user est créé et peut se connecter, mais le BusinessOwner
    /// reste en attente d'approbation admin.
    ///
    /// Flux :
    ///   1. Créer User
    ///   2. Créer BusinessOwner (Pending)
    ///   3. Créer PendingRequest avec payload { businessOwnerId }
    ///
    /// Remarque :
    ///   On fait ici deux SaveChangesAsync parce que le PendingRequest
    ///   doit contenir l'ID réel du BusinessOwner pour rester compatible
    ///   avec le AdminService actuel.
    /// </summary>
    public async Task<TokenResponseDto> RegisterBusinessOwnerAsync(
        string email,
        string password,
        string fullName,
        string? phone,
        string companyName,
        string? taxId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        ValidateEmail(normalizedEmail);
        ValidatePassword(password);
        ValidateFullName(fullName);
        ValidateCompanyName(companyName);

        await EnsureEmailAvailableAsync(normalizedEmail, cancellationToken);

        // 1. Créer le compte utilisateur
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = HashPassword(password),
            FullName = fullName.Trim(),
            Phone = NormalizePhone(phone),
            Role = UserRole.BusinessOwner,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. Créer le profil business owner
        var businessOwner = new BusinessOwner
        {
            UserId = user.Id,
            CompanyName = companyName.Trim(),
            TaxId = NormalizeOptional(taxId),
            Status = BusinessOwnerStatus.Pending
        };

        await _businessOwnerRepository.AddAsync(businessOwner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Créer la demande admin compatible avec AdminService actuel
        var payload = new BusinessRegistrationPayload
        {
            BusinessOwnerId = businessOwner.Id
        };

        var pendingRequest = new PendingRequest
        {
            SubmittedBy = user.Id,
            RequestType = RequestType.BusinessRegistration,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PendingRequests.AddAsync(pendingRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GenerateTokenResponse(user);
    }

    /// <summary>
    /// Surcharge pratique basée sur RegisterBusinessOwnerDto.
    /// </summary>
    public Task<TokenResponseDto> RegisterBusinessOwnerAsync(
        RegisterBusinessOwnerDto dto,
        CancellationToken cancellationToken = default)
    {
        return RegisterBusinessOwnerAsync(
            dto.Email,
            dto.Password,
            dto.FullName,
            dto.Phone,
            dto.CompanyName,
            dto.TaxId,
            cancellationToken);
    }

    private sealed class BusinessRegistrationPayload
    {
        public int BusinessOwnerId { get; set; }
    }
    // ═══════════════════════════════════════════════════════
    // CONNEXION
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Vérifier les identifiants et retourner un token JWT.
    ///
    /// Cas d'erreur :
    ///   - Email inconnu → "Identifiants invalides"
    ///   - Mot de passe incorrect → "Identifiants invalides"
    ///   - Compte désactivé → "Votre compte a été désactivé"
    /// </summary>
    public async Task<TokenResponseDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        ValidateEmail(normalizedEmail);

        var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Même message pour éviter l'énumération d'emails
        if (user == null || !VerifyPassword(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Identifiants invalides");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Votre compte a été désactivé");

        return GenerateTokenResponse(user);
    }

    /// <summary>
    /// Surcharge pratique basée sur LoginDto.
    /// </summary>
    public Task<TokenResponseDto> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        return LoginAsync(dto.Email, dto.Password, cancellationToken);
    }

    // ═══════════════════════════════════════════════════════
    // REFRESH TOKEN
    //
    // Ici, on utilise un refresh token JWT signé.
    // Pas de persistance ni de rotation serveur pour le moment.
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Renouveler le JWT via un refresh token valide.
    /// </summary>
    public async Task<TokenResponseDto> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Refresh token manquant");

        var userId = ValidateRefreshToken(refreshToken);

        if (!userId.HasValue)
            throw new UnauthorizedAccessException("Refresh token invalide ou expiré");

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Utilisateur non trouvé");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Votre compte a été désactivé");

        return GenerateTokenResponse(user);
    }

    // ═══════════════════════════════════════════════════════
    // PROFIL
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Récupérer le profil complet de l'utilisateur connecté.
    /// Inclut les infos du BusinessOwner si le user en est un,
    /// et le thème favori s'il en a un.
    /// </summary>
    public async Task<UserProfile> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithBusinessOwnerAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Utilisateur non trouvé");

        string? favoriteThemeName = null;

        if (user.FavoriteThemeId.HasValue)
        {
            var theme = await _unitOfWork.Themes.GetByIdAsync(user.FavoriteThemeId.Value, cancellationToken);
            favoriteThemeName = theme?.Name;
        }

        var reviewCount = await _unitOfWork.Reviews.CountAsync(
            r => r.UserId == userId,
            cancellationToken);

        // Temporaire : ton IUnitOfWork actuel n'expose pas encore ScanHistories
        var scanCount = 0;

        return new UserProfile
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            FavoriteThemeId = user.FavoriteThemeId,
            FavoriteThemeName = favoriteThemeName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            BusinessOwnerStatus = user.BusinessOwner?.Status.ToString(),
            CompanyName = user.BusinessOwner?.CompanyName,
            ReviewCount = reviewCount,
            ScanCount = scanCount
        };
    }

    /// <summary>
    /// Mettre à jour le profil.
    /// Le user peut modifier : nom, téléphone, localisation, thème favori.
    /// Il ne peut PAS modifier : email, rôle, status.
    /// </summary>
    public async Task<UserProfile> UpdateProfileAsync(
        int userId,
        string? fullName,
        string? phone,
        double? latitude,
        double? longitude,
        int? favoriteThemeId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Utilisateur non trouvé");

        if (fullName != null)
        {
            ValidateFullName(fullName);
            user.FullName = fullName.Trim();
        }

        if (phone != null)
        {
            user.Phone = NormalizePhone(phone);
        }

        // Si l'appel veut modifier la localisation, il faut fournir latitude + longitude ensemble
        if (latitude.HasValue || longitude.HasValue)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                throw new ArgumentException("Latitude et longitude doivent être fournies ensemble.");

            ValidateCoordinates(latitude.Value, longitude.Value);

            user.Latitude = latitude.Value;
            user.Longitude = longitude.Value;
        }

        if (favoriteThemeId.HasValue)
        {
            if (favoriteThemeId.Value == 0)
            {
                user.FavoriteThemeId = null;
            }
            else
            {
                var themeExists = await _unitOfWork.Themes.AnyAsync(
                    t => t.Id == favoriteThemeId.Value && t.IsActive,
                    cancellationToken);

                if (!themeExists)
                    throw new ArgumentException("Thème invalide ou inactif.");

                user.FavoriteThemeId = favoriteThemeId.Value;
            }
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProfileAsync(userId, cancellationToken);
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentException("Latitude invalide. Elle doit être comprise entre -90 et 90.");

        if (longitude is < -180 or > 180)
            throw new ArgumentException("Longitude invalide. Elle doit être comprise entre -180 et 180.");
    }

    // ═══════════════════════════════════════════════════════
    // GESTION DE COMPTE / ADMIN
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Changer le mot de passe d'un utilisateur.
    /// Vérifie :
    ///   - que l'utilisateur existe
    ///   - que l'ancien mot de passe est correct
    ///   - que le nouveau mot de passe est différent de l'ancien
    ///   - que le nouveau mot de passe respecte les règles de validation
    /// </summary>
    public async Task ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Utilisateur non trouvé");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Ancien mot de passe incorrect");

        if (VerifyPassword(newPassword, user.PasswordHash))
            throw new ArgumentException("Le nouveau mot de passe doit être différent de l'ancien");

        ValidatePassword(newPassword);

        user.PasswordHash = HashPassword(newPassword);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Activer ou désactiver un compte utilisateur.
    /// Seul un admin peut le faire.
    /// Un admin ne peut pas se désactiver lui-même.
    /// </summary>
    public async Task SetAccountActiveAsync(
        int actorUserId,
        int targetUserId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var actor = await _unitOfWork.Users.GetByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Utilisateur admin introuvable");

        if (actor.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Seul un administrateur peut modifier l'état d'un compte");

        var targetUser = await _unitOfWork.Users.GetByIdAsync(targetUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Utilisateur cible introuvable");

        if (actorUserId == targetUserId && !isActive)
            throw new InvalidOperationException("Un administrateur ne peut pas se désactiver lui-même");

        targetUser.IsActive = isActive;

        _unitOfWork.Users.Update(targetUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Lister les utilisateurs pour le dashboard admin.
    ///
    /// Version compatible avec les interfaces actuelles :
    ///   - utilise FindAsync au lieu de Query()/Include()
    ///   - enrichit la page courante avec BusinessOwner si nécessaire
    ///
    /// Remarque :
    ///   Ce n'est pas la version la plus optimale, mais elle reste
    ///   compatible avec ton architecture actuelle sans fuite EF Core.
    /// </summary>
    public async Task<PagedUsersResult> GetUsersAsync(
        UserRole? role = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 20;

        if (pageSize > 100)
            pageSize = 100;

        IEnumerable<User> users;

        if (role.HasValue && isActive.HasValue)
        {
            users = await _unitOfWork.Users.FindAsync(
                u => u.Role == role.Value && u.IsActive == isActive.Value,
                cancellationToken);
        }
        else if (role.HasValue)
        {
            users = await _unitOfWork.Users.FindAsync(
                u => u.Role == role.Value,
                cancellationToken);
        }
        else if (isActive.HasValue)
        {
            users = await _unitOfWork.Users.FindAsync(
                u => u.IsActive == isActive.Value,
                cancellationToken);
        }
        else
        {
            users = await _unitOfWork.Users.FindAsync(_ => true, cancellationToken);
        }

        var orderedUsers = users
            .OrderByDescending(u => u.CreatedAt)
            .ToList();

        var totalCount = orderedUsers.Count;

        var pagedUsers = orderedUsers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<UserListItem>();

        foreach (var user in pagedUsers)
        {
            string? businessOwnerStatus = null;
            string? companyName = null;

            if (user.Role == UserRole.BusinessOwner)
            {
                var enrichedUser = await _unitOfWork.Users.GetByIdWithBusinessOwnerAsync(user.Id, cancellationToken);
                businessOwnerStatus = enrichedUser?.BusinessOwner?.Status.ToString();
                companyName = enrichedUser?.BusinessOwner?.CompanyName;
            }

            items.Add(new UserListItem
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                BusinessOwnerStatus = businessOwnerStatus,
                CompanyName = companyName
            });
        }

        return new PagedUsersResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // ═══════════════════════════════════════════════════════
    // HELPER : vérifier si le business owner est approuvé
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Vérifie que l'utilisateur est un business owner approuvé.
    /// Lève une exception si :
    ///   - l'user n'a pas de BusinessOwner associé
    ///   - le BusinessOwner n'est pas encore approuvé
    /// </summary>
    public async Task<BusinessOwner> RequireApprovedBusinessOwnerAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var businessOwner = await _businessOwnerRepository.FirstOrDefaultAsync(
            b => b.UserId == userId,
            cancellationToken);

        if (businessOwner == null)
            throw new UnauthorizedAccessException(
                "Vous n'êtes pas inscrit en tant que business owner");

        if (businessOwner.Status != BusinessOwnerStatus.Approved)
            throw new UnauthorizedAccessException(
                $"Votre compte business est en attente d'approbation (status: {businessOwner.Status})");

        return businessOwner;
    }

    // ═══════════════════════════════════════════════════════
    // HELPER : vérifier si le business owner est approuvé
    //
    // Appelé par d'autres services (BrandService, StoreService)
    // avant de permettre une action business.
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Vérifie que l'utilisateur est un business owner approuvé.
    /// Lève une exception si :
    ///   - l'user n'a pas de BusinessOwner associé
    ///   - le BusinessOwner n'est pas encore approuvé
    ///
    /// Retourne le BusinessOwner si tout est OK.
    /// </summary>

    // ═══════════════════════════════════════════════════════
    // MÉTHODES PRIVÉES : cryptographie et JWT
    // ═══════════════════════════════════════════════════════

    // ═══════════════════════════════════════════════════════
    // JWT / REFRESH TOKEN
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Génère la réponse complète d'authentification :
    /// - access token JWT
    /// - refresh token
    /// - expiration
    /// - rôle et nom affichable
    /// </summary>
    private TokenResponseDto GenerateTokenResponse(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");

        var expirationMinutes = int.TryParse(jwtSettings["ExpirationInMinutes"], out var value)
            ? value
            : 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return new TokenResponseDto
        {
            Token = GenerateJwtToken(user, expiresAt),
            RefreshToken = GenerateRefreshToken(user.Id),
            ExpiresAt = expiresAt,
            Role = user.Role.ToString(),
            FullName = user.FullName
        };
    }

    /// <summary>
    /// Génère le JWT principal utilisé pour authentifier l'utilisateur.
    /// </summary>
    private string GenerateJwtToken(User user, DateTime expiresAt)
    {
        var jwtSettings = _configuration.GetSection("Jwt");

        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer non configuré");

        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("JWT Audience non configuré");

        var credentials = new SigningCredentials(
            GetSigningKey(),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Génère un refresh token JWT signé.
    /// Pour le PFE, cette approche JWT-based est suffisante.
    /// </summary>
    private string GenerateRefreshToken(int userId)
    {
        var jwtSettings = _configuration.GetSection("Jwt");

        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer non configuré");

        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("JWT Audience non configuré");

        var refreshDays = int.TryParse(jwtSettings["RefreshTokenExpirationInDays"], out var value)
            ? value
            : 7;

        var credentials = new SigningCredentials(
            GetSigningKey(),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtClaimNames.Sub, userId.ToString()),
            new Claim("token_type", "refresh"),
            new Claim(JwtClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(refreshDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Valide un refresh token et retourne le userId s'il est valide.
    /// Retourne null si le token est invalide, expiré ou n'est pas un refresh token.
    /// </summary>
    private int? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer non configuré");

            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("JWT Audience non configuré");

            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = GetSigningKey(),
                ClockSkew = TimeSpan.Zero
            }, out _);

            var tokenType = principal.FindFirst("token_type")?.Value;
            if (tokenType != "refresh")
                return null;

            var userIdClaim = principal.FindFirst(JwtClaimNames.Sub)?.Value;

            return int.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Retourne la clé de signature JWT.
    /// Vérifie que le secret existe et fait au moins 32 caractères.
    /// </summary>
    private SymmetricSecurityKey GetSigningKey()
    {
        var secret = _configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT Secret non configuré");

        if (secret.Length < 32)
            throw new InvalidOperationException(
                "Le secret JWT doit contenir au moins 32 caractères.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
    // ═══════════════════════════════════════════════════════
    // VALIDATION / NORMALISATION / SÉCURITÉ
    // ═══════════════════════════════════════════════════════

    private async Task EnsureEmailAvailableAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.Users.ExistsByEmailAsync(normalizedEmail, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Cet email est déjà utilisé");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("L'email est obligatoire");

        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        if (!Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
            throw new ArgumentException("Format d'email invalide");
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Le mot de passe est obligatoire");

        if (password.Length < 8)
            throw new ArgumentException("Le mot de passe doit contenir au moins 8 caractères");

        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Le mot de passe doit contenir au moins une majuscule");

        if (!password.Any(char.IsLower))
            throw new ArgumentException("Le mot de passe doit contenir au moins une minuscule");

        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Le mot de passe doit contenir au moins un chiffre");
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Le nom complet est obligatoire");

        var trimmed = fullName.Trim();

        if (trimmed.Length < 2)
            throw new ArgumentException("Le nom complet est trop court");

        if (trimmed.Length > 150)
            throw new ArgumentException("Le nom complet ne doit pas dépasser 150 caractères");
    }

    private static void ValidateCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Le nom de l'entreprise est obligatoire");

        var trimmed = companyName.Trim();

        if (trimmed.Length < 2)
            throw new ArgumentException("Le nom de l'entreprise est trop court");

        if (trimmed.Length > 200)
            throw new ArgumentException("Le nom de l'entreprise ne doit pas dépasser 200 caractères");
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? string.Empty
            : email.Trim().ToLowerInvariant();
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var cleaned = phone.Trim();

        cleaned = cleaned
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty);

        // Garde le + en tête si présent
        if (cleaned.StartsWith("+"))
        {
            var prefix = "+";
            var digits = new string(cleaned.Skip(1).Where(char.IsDigit).ToArray());
            return prefix + digits;
        }

        return new string(cleaned.Where(char.IsDigit).ToArray());
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// DTO temporaire de profil.
/// Tu pourras plus tard le déplacer dans DTOs/Auth si tu veux rester 100% strict.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? FavoriteThemeId { get; set; }
    public string? FavoriteThemeName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? BusinessOwnerStatus { get; set; }
    public string? CompanyName { get; set; }
    public int ReviewCount { get; set; }
    public int ScanCount { get; set; }
}
public class UserListItem
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? BusinessOwnerStatus { get; set; }
    public string? CompanyName { get; set; }
}

public class PagedUsersResult
{
    public IReadOnlyList<UserListItem> Items { get; set; } = Array.Empty<UserListItem>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}