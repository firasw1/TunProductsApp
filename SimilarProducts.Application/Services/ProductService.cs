using SimilarProducts.Application.DTOs.Common;
using SimilarProducts.Application.DTOs.Products;
using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Domain.Interfaces.Services;
using System.Text.Json;
namespace SimilarProducts.Application.Services;

/// <summary>
/// Service métier des produits.
///
/// Objectifs :
///   - lecture paginée et recherche
///   - détail produit enrichi
///   - création / mise à jour / suppression
///   - synchronisation des tags
///   - gestion des images
///   - logique business owner / admin
///   - déclenchement d'analyse IA si nécessaire
///
/// Remarque :
///   On garde ici l'ambition fonctionnelle du fichier d'origine.
///   On corrige seulement ce qui est nécessaire pour l'aligner
///   avec les interfaces réellement disponibles dans le projet.
/// </summary>
public class ProductService
{
    private readonly IGenericRepository<SubCategory> _subCategoryRepository;

    private readonly IUnitOfWork _unitOfWork;

    // Repositories génériques nécessaires car IUnitOfWork n'expose
    // pas forcément toutes ces entités directement
    private readonly IGenericRepository<Tag> _tagRepository;
    private readonly IGenericRepository<ProductTag> _productTagRepository;
    private readonly IGenericRepository<ProductImage> _productImageRepository;
    private readonly IGenericRepository<Brand> _brandRepository;
    private readonly IGenericRepository<BusinessOwner> _businessOwnerRepository;

    // Service IA : on le garde pour ne pas perdre la logique métier avancée
    private readonly IAiAnalysisService _aiAnalysisService;

    public ProductService(
        IUnitOfWork unitOfWork,
        IGenericRepository<Tag> tagRepository,
        IGenericRepository<ProductTag> productTagRepository,
        IGenericRepository<ProductImage> productImageRepository,
        IGenericRepository<Brand> brandRepository,
        IGenericRepository<BusinessOwner> businessOwnerRepository,
        IGenericRepository<SubCategory> subCategoryRepository,
        IAiAnalysisService aiAnalysisService)
    {
        _unitOfWork = unitOfWork;
        _tagRepository = tagRepository;
        _productTagRepository = productTagRepository;
        _productImageRepository = productImageRepository;
        _brandRepository = brandRepository;
        _businessOwnerRepository = businessOwnerRepository;
        _aiAnalysisService = aiAnalysisService;
        _subCategoryRepository = subCategoryRepository;
    }

    // ═══════════════════════════════════════════════════════
    // LECTURE : LISTE / DÉTAIL / RECHERCHE
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Retourne une liste paginée de produits avec filtres.
    /// Lecture enrichie : marque, catégorie, sous-catégorie, tags, image principale.
    /// </summary>
    public async Task<PaginatedResultDto<ProductDto>> GetAllAsync(
        string? searchTerm = null,
        int? brandId = null,
        int? subCategoryId = null,
        string? status = null,
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

        // IMPORTANT :
        // On garde la lecture enrichie, mais on la délègue au repository
        // pour éviter Query()/Include() dans Application.
        var products = await _unitOfWork.Products.GetAllWithDetailsAsync(cancellationToken);

        IEnumerable<Product> query = products;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim().ToLowerInvariant();

            query = query.Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                (!string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode.ToLower().Contains(normalizedSearch)) ||
                (p.Brand != null && p.Brand.Name.ToLower().Contains(normalizedSearch)));
        }

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (subCategoryId.HasValue)
            query = query.Where(p => p.SubCategoryId == subCategoryId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status.ToString().Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = query.Count();

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToProductDto)
            .ToList();

        return new PaginatedResultDto<ProductDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Retourne le détail complet d'un produit.
    /// Inclut images, tags, analyse IA et statistiques d'avis.
    /// </summary>
    public async Task<ProductDetailDto> GetDetailAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Produit introuvable");

        return MapToProductDetailDto(product);
    }

    /// <summary>
    /// Recherche rapide de produits.
    /// Version légère pour autocomplete / sélecteur / recherche simple.
    /// </summary>
    public async Task<List<ProductDto>> SearchAsync(
        string searchTerm,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<ProductDto>();

        if (limit <= 0)
            limit = 10;

        if (limit > 50)
            limit = 50;

        var normalizedSearch = searchTerm.Trim().ToLowerInvariant();

        var products = await _unitOfWork.Products.GetAllWithDetailsAsync(cancellationToken);

        return products
            .Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                (!string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode.ToLower().Contains(normalizedSearch)) ||
                (p.Brand != null && p.Brand.Name.ToLower().Contains(normalizedSearch)))
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(MapToProductDto)
            .ToList();
    }

    // ═══════════════════════════════════════════════════════
    // HELPERS DE MAPPING
    // ═══════════════════════════════════════════════════════

    private static ProductDto MapToProductDto(Product product)
    {
        var primaryImage = product.Images?
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.DisplayOrder)
            .FirstOrDefault();

        return new ProductDto
        {
            Id = product.Id,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            SubCategoryId = product.SubCategoryId,
            CategoryName = product.SubCategory?.Category?.Name ?? string.Empty,
            SubCategoryName = product.SubCategory?.Name ?? string.Empty,
            Name = product.Name,
            Barcode = product.Barcode,
            OriginLevel = product.OriginLevel.ToString(),
            DataSource = product.DataSource.ToString(),
            Status = product.Status.ToString(),
            PrimaryImageUrl = primaryImage?.ImageUrl,
            Tags = product.ProductTags?
                .Where(pt => pt.Tag != null)
                .Select(pt => pt.Tag!.Name)
                .Distinct()
                .ToList() ?? new List<string>()
        };
    }

    private static ProductDetailDto MapToProductDetailDto(Product product)
    {
        var dto = new ProductDetailDto
        {
            Id = product.Id,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            SubCategoryId = product.SubCategoryId,
            CategoryName = product.SubCategory?.Category?.Name ?? string.Empty,
            SubCategoryName = product.SubCategory?.Name ?? string.Empty,
            Name = product.Name,
            Barcode = product.Barcode,
            OriginLevel = product.OriginLevel.ToString(),
            DataSource = product.DataSource.ToString(),
            Status = product.Status.ToString(),
            PrimaryImageUrl = product.Images?
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault()?.ImageUrl,
            Tags = product.ProductTags?
                .Where(pt => pt.Tag != null)
                .Select(pt => pt.Tag!.Name)
                .Distinct()
                .ToList() ?? new List<string>(),

            BarcodeCountry = product.BarcodeCountry,
            Description = product.Description,
            Composition = product.Composition,

            ImageUrls = product.Images?
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList() ?? new List<string>(),

            BioScore = product.AiAnalysis?.BioScore,
            NaturalScore = product.AiAnalysis?.NaturalScore,
            HealthScore = product.AiAnalysis?.HealthScore,
            AiSummary = product.AiAnalysis?.Summary,

            AverageRating = product.Reviews != null && product.Reviews.Any()
                ? Math.Round(product.Reviews.Average(r => r.Rating), 2)
                : 0,

            ReviewCount = product.Reviews?.Count ?? 0
        };

        return dto;
    }
    // ═══════════════════════════════════════════════════════
    // CRÉATION — 3 SOURCES DIFFÉRENTES
    //
    // La logique est la même dans les 3 cas :
    //   1. Vérifier que le barcode n'existe pas déjà
    //   2. Créer le produit avec le status approprié
    //   3. Associer les tags
    //   4. Soumettre une PendingRequest si nécessaire
    //   5. Déclencher l'analyse IA en background
    //
    // La différence est dans le status initial et les permissions.
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// SOURCE 1 : Un consumer propose un produit (après scan introuvable).
    ///
    /// Status initial : Pending
    /// Permissions : tout user authentifié
    /// Flux : crée le produit + soumet une PendingRequest
    ///         → l'admin doit approuver avant que le produit soit visible
    ///
    /// C'est le cas quand le ScanService retourne not_found
    /// et que le user remplit le formulaire de proposition.
    /// </summary>
      // ═══════════════════════════════════════════════════════
    // CRÉATION PRODUIT
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Création d'un produit par un business owner approuvé.
    /// Le produit est créé avec un statut Pending par défaut.
    /// </summary>
    public async Task<ProductDetailDto> CreateAsync(
        CreateProductDto dto,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        return await CreateInternalAsync(
            dto,
            actorUserId: actorUserId,
            isAdmin: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Création directe par un administrateur.
    /// Le produit est approuvé immédiatement.
    /// </summary>
    public async Task<ProductDetailDto> CreateAsAdminAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        return await CreateInternalAsync(
            dto,
            actorUserId: null,
            isAdmin: true,
            cancellationToken: cancellationToken);
    }

    private async Task<ProductDetailDto> CreateInternalAsync(
        CreateProductDto dto,
        int? actorUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Le nom du produit est obligatoire.");

        var productName = dto.Name.Trim();

        var subCategoryExists = await _subCategoryRepository.AnyAsync(
            sc => sc.Id == dto.SubCategoryId,
            cancellationToken);

        if (!subCategoryExists)
            throw new ArgumentException("Sous-catégorie introuvable.");

        if (dto.BrandId.HasValue)
        {
            var brandExists = await _brandRepository.AnyAsync(
                b => b.Id == dto.BrandId.Value,
                cancellationToken);

            if (!brandExists)
                throw new ArgumentException("Marque introuvable.");
        }

        if (!isAdmin)
        {
            if (!actorUserId.HasValue)
                throw new UnauthorizedAccessException("Utilisateur invalide.");

            var businessOwner = await RequireApprovedBusinessOwnerAsync(actorUserId.Value, cancellationToken);

            if (dto.BrandId.HasValue)
            {
                await EnsureBrandBelongsToBusinessOwnerAsync(
                    dto.BrandId.Value,
                    businessOwner.Id,
                    cancellationToken);
            }
        }

        var product = new Product
        {
            BrandId = dto.BrandId,
            SubCategoryId = dto.SubCategoryId,
            Name = productName,
            Barcode = NormalizeOptional(dto.Barcode),
            Description = NormalizeOptional(dto.Description),
            Composition = NormalizeOptional(dto.Composition),
            OriginLevel = ParseOriginLevel(dto.OriginLevel),
            DataSource = isAdmin ? DataSource.Manual : DataSource.BrandDeclared,
            Status = isAdmin ? ProductStatus.Approved : ProductStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Tags : on garde la fonctionnalité riche, elle sera implémentée au bloc tags
        if (dto.TagIds is { Count: > 0 })
        {
            await SyncTagsAsync(product.Id, dto.TagIds, cancellationToken);
        }

        // Analyse IA : on garde la logique métier avancée,
        // l'implémentation détaillée viendra dans le bloc IA/helpers
        if (!string.IsNullOrWhiteSpace(product.Composition))
        {
            await TryTriggerAiAnalysisAsync(product.Id, cancellationToken);
        }

        return await GetDetailAsync(product.Id, cancellationToken);
    }
    // ═══════════════════════════════════════════════════════
    // MISE À JOUR / SUPPRESSION
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Mise à jour d'un produit par un business owner approuvé.
    /// Le produit doit appartenir à une marque liée à ce business owner.
    /// </summary>
    public async Task<ProductDetailDto> UpdateAsync(
        int productId,
        UpdateProductDto dto,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        return await UpdateInternalAsync(
            productId,
            dto,
            actorUserId: actorUserId,
            isAdmin: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Mise à jour d'un produit par un administrateur.
    /// </summary>
    public async Task<ProductDetailDto> UpdateAsAdminAsync(
        int productId,
        UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        return await UpdateInternalAsync(
            productId,
            dto,
            actorUserId: null,
            isAdmin: true,
            cancellationToken: cancellationToken);
    }

    private async Task<ProductDetailDto> UpdateInternalAsync(
        int productId,
        UpdateProductDto dto,
        int? actorUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Produit introuvable.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Le nom du produit est obligatoire.");

        var subCategoryExists = await _subCategoryRepository.AnyAsync(
            sc => sc.Id == dto.SubCategoryId,
            cancellationToken);

        if (!subCategoryExists)
            throw new ArgumentException("Sous-catégorie introuvable.");

        if (dto.BrandId.HasValue)
        {
            var brandExists = await _brandRepository.AnyAsync(
                b => b.Id == dto.BrandId.Value,
                cancellationToken);

            if (!brandExists)
                throw new ArgumentException("Marque introuvable.");
        }

        if (!isAdmin)
        {
            if (!actorUserId.HasValue)
                throw new UnauthorizedAccessException("Utilisateur invalide.");

            var businessOwner = await RequireApprovedBusinessOwnerAsync(actorUserId.Value, cancellationToken);

            // Le produit doit être rattaché à une marque appartenant à ce business owner
            if (!product.BrandId.HasValue)
                throw new UnauthorizedAccessException("Ce produit n'est pas modifiable par un business owner.");

            await EnsureBrandBelongsToBusinessOwnerAsync(
                product.BrandId.Value,
                businessOwner.Id,
                cancellationToken);

            // Si on change de marque, la nouvelle marque doit aussi lui appartenir
            if (dto.BrandId.HasValue && dto.BrandId.Value != product.BrandId.Value)
            {
                await EnsureBrandBelongsToBusinessOwnerAsync(
                    dto.BrandId.Value,
                    businessOwner.Id,
                    cancellationToken);
            }
        }

        var previousComposition = product.Composition;

        product.BrandId = dto.BrandId;
        product.SubCategoryId = dto.SubCategoryId;
        product.Name = dto.Name.Trim();
        product.Barcode = NormalizeOptional(dto.Barcode);
        product.Description = NormalizeOptional(dto.Description);
        product.Composition = NormalizeOptional(dto.Composition);
        product.OriginLevel = ParseOriginLevel(dto.OriginLevel);
        product.UpdatedAt = DateTime.UtcNow;

        // Règle métier conservée :
        // si un business owner modifie un produit déjà approuvé,
        // on le repasse en Pending pour revalidation admin.
        if (!isAdmin && product.Status == ProductStatus.Approved)
        {
            product.Status = ProductStatus.Pending;
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Synchronisation des tags si fournis
        if (dto.TagIds != null)
        {
            await SyncTagsAsync(product.Id, dto.TagIds, cancellationToken);
        }

        // Si la composition a changé, on relance l'analyse IA
        if (!string.Equals(
                NormalizeOptional(previousComposition),
                NormalizeOptional(product.Composition),
                StringComparison.Ordinal))
        {
            await TryTriggerAiAnalysisAsync(product.Id, cancellationToken);
        }

        return await GetDetailAsync(product.Id, cancellationToken);
    }

    /// <summary>
    /// Suppression d'un produit par un business owner approuvé.
    /// Le produit doit appartenir à une marque liée à ce business owner.
    /// </summary>
    public async Task DeleteAsync(
        int productId,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        await DeleteInternalAsync(
            productId,
            actorUserId: actorUserId,
            isAdmin: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Suppression d'un produit par un administrateur.
    /// </summary>
    public async Task DeleteAsAdminAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        await DeleteInternalAsync(
            productId,
            actorUserId: null,
            isAdmin: true,
            cancellationToken: cancellationToken);
    }

    private async Task DeleteInternalAsync(
        int productId,
        int? actorUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Produit introuvable.");

        if (!isAdmin)
        {
            if (!actorUserId.HasValue)
                throw new UnauthorizedAccessException("Utilisateur invalide.");

            var businessOwner = await RequireApprovedBusinessOwnerAsync(actorUserId.Value, cancellationToken);

            if (!product.BrandId.HasValue)
                throw new UnauthorizedAccessException("Ce produit n'est pas supprimable par un business owner.");

            await EnsureBrandBelongsToBusinessOwnerAsync(
                product.BrandId.Value,
                businessOwner.Id,
                cancellationToken);
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    // ═══════════════════════════════════════════════════════
    // TAGS / OWNERSHIP / PARSING HELPERS
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Synchronise les tags d'un produit.
    /// Garde la logique riche : ajout des nouveaux, suppression des anciens,
    /// sans reset brutal de toute la collection.
    /// </summary>
    private async Task SyncTagsAsync(
        int productId,
        List<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        tagIds ??= new List<int>();

        var normalizedTagIds = tagIds
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();

        var existingLinks = (await _productTagRepository.FindAsync(
                pt => pt.ProductId == productId,
                cancellationToken))
            .ToList();

        var existingTagIds = existingLinks
            .Select(pt => pt.TagId)
            .ToHashSet();

        // Vérifier que tous les tags demandés existent
        if (normalizedTagIds.Count > 0)
        {
            var existingTags = await _tagRepository.FindAsync(
                t => normalizedTagIds.Contains(t.Id),
                cancellationToken);

            var existingRealTagIds = existingTags
                .Select(t => t.Id)
                .ToHashSet();

            var missingTagIds = normalizedTagIds.Except(existingRealTagIds).ToList();

            if (missingTagIds.Count > 0)
                throw new ArgumentException(
                    $"Les tags suivants sont introuvables : {string.Join(", ", missingTagIds)}");
        }

        var linksToAdd = normalizedTagIds.Except(existingTagIds).ToList();
        var linksToRemove = existingLinks
            .Where(link => !normalizedTagIds.Contains(link.TagId))
            .ToList();

        foreach (var tagId in linksToAdd)
        {
            await _productTagRepository.AddAsync(new ProductTag
            {
                ProductId = productId,
                TagId = tagId
            }, cancellationToken);
        }

        foreach (var link in linksToRemove)
        {
            _productTagRepository.Delete(link);
        }

        if (linksToAdd.Count > 0 || linksToRemove.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Vérifie qu'une marque appartient bien au business owner donné.
    /// </summary>
    private async Task EnsureBrandBelongsToBusinessOwnerAsync(
        int brandId,
        int businessOwnerId,
        CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.FirstOrDefaultAsync(
            b => b.Id == brandId,
            cancellationToken);

        if (brand == null)
            throw new ArgumentException("Marque introuvable.");

        // IMPORTANT :
        // Ici j'utilise OwnerId car c'est le nom le plus probable selon ton modèle.
        // Si dans ton Brand.cs la FK s'appelle BusinessOwnerId, remplace simplement OwnerId par BusinessOwnerId.
        if (brand.OwnerId != businessOwnerId)
            throw new UnauthorizedAccessException(
                "Cette marque n'appartient pas à votre compte business.");
    }

    /// <summary>
    /// Convertit la valeur string venant du DTO vers l'enum Domain OriginLevel.
    /// </summary>
    private static OriginLevel ParseOriginLevel(string originLevel)
    {
        if (string.IsNullOrWhiteSpace(originLevel))
            throw new ArgumentException("L'origine du produit est obligatoire.");

        if (Enum.TryParse<OriginLevel>(originLevel.Trim(), true, out var parsed))
            return parsed;

        throw new ArgumentException($"Origine du produit invalide : {originLevel}");
    }

    /// <summary>
    /// Normalise une chaîne optionnelle : null si vide, sinon trim.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // ═══════════════════════════════════════════════════════
    // IMAGES / IA
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Ajoute une image à un produit pour un business owner approuvé.
    /// Le produit doit appartenir à une marque liée à ce business owner.
    /// </summary>
    public async Task<ProductDetailDto> AddImageAsync(
        int productId,
        string imageUrl,
        bool isPrimary,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        return await AddImageInternalAsync(
            productId,
            imageUrl,
            isPrimary,
            actorUserId: actorUserId,
            isAdmin: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Ajoute une image à un produit pour un administrateur.
    /// </summary>
    public async Task<ProductDetailDto> AddImageAsAdminAsync(
        int productId,
        string imageUrl,
        bool isPrimary,
        CancellationToken cancellationToken = default)
    {
        return await AddImageInternalAsync(
            productId,
            imageUrl,
            isPrimary,
            actorUserId: null,
            isAdmin: true,
            cancellationToken: cancellationToken);
    }

    private async Task<ProductDetailDto> AddImageInternalAsync(
        int productId,
        string imageUrl,
        bool isPrimary,
        int? actorUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Produit introuvable.");

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("L'URL de l'image est obligatoire.");

        if (!isAdmin)
        {
            if (!actorUserId.HasValue)
                throw new UnauthorizedAccessException("Utilisateur invalide.");

            var businessOwner = await RequireApprovedBusinessOwnerAsync(actorUserId.Value, cancellationToken);

            if (!product.BrandId.HasValue)
                throw new UnauthorizedAccessException("Ce produit n'est pas modifiable par un business owner.");

            await EnsureBrandBelongsToBusinessOwnerAsync(
                product.BrandId.Value,
                businessOwner.Id,
                cancellationToken);
        }

        var normalizedUrl = imageUrl.Trim();

        var existingImages = product.Images?.ToList() ?? new List<ProductImage>();

        var nextDisplayOrder = existingImages.Count == 0
            ? 1
            : existingImages.Max(i => i.DisplayOrder) + 1;

        // Si l'image ajoutée devient principale, on retire ce statut aux autres
        if (isPrimary)
        {
            foreach (var existingImage in existingImages.Where(i => i.IsPrimary))
            {
                existingImage.IsPrimary = false;
                _productImageRepository.Update(existingImage);
            }
        }

        // Si aucune image n'existe encore, la première devient automatiquement principale
        var shouldBePrimary = isPrimary || existingImages.Count == 0;

        var newImage = new ProductImage
        {
            ProductId = productId,
            ImageUrl = normalizedUrl,
            IsPrimary = shouldBePrimary,
            DisplayOrder = nextDisplayOrder
        };

        await _productImageRepository.AddAsync(newImage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetDetailAsync(productId, cancellationToken);
    }

    /// <summary>
    /// Supprime une image d'un produit pour un business owner approuvé.
    /// Le produit doit appartenir à une marque liée à ce business owner.
    /// </summary>
    public async Task<ProductDetailDto> DeleteImageAsync(
        int productId,
        int imageId,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteImageInternalAsync(
            productId,
            imageId,
            actorUserId: actorUserId,
            isAdmin: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Supprime une image d'un produit pour un administrateur.
    /// </summary>
    public async Task<ProductDetailDto> DeleteImageAsAdminAsync(
        int productId,
        int imageId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteImageInternalAsync(
            productId,
            imageId,
            actorUserId: null,
            isAdmin: true,
            cancellationToken: cancellationToken);
    }

    private async Task<ProductDetailDto> DeleteImageInternalAsync(
        int productId,
        int imageId,
        int? actorUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Produit introuvable.");

        if (!isAdmin)
        {
            if (!actorUserId.HasValue)
                throw new UnauthorizedAccessException("Utilisateur invalide.");

            var businessOwner = await RequireApprovedBusinessOwnerAsync(actorUserId.Value, cancellationToken);

            if (!product.BrandId.HasValue)
                throw new UnauthorizedAccessException("Ce produit n'est pas modifiable par un business owner.");

            await EnsureBrandBelongsToBusinessOwnerAsync(
                product.BrandId.Value,
                businessOwner.Id,
                cancellationToken);
        }

        var image = product.Images?.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new KeyNotFoundException("Image introuvable.");

        var wasPrimary = image.IsPrimary;

        _productImageRepository.Delete(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Si on a supprimé l'image principale, on promeut une autre image si elle existe
        if (wasPrimary)
        {
            var refreshedProduct = await _unitOfWork.Products.GetByIdWithDetailsAsync(productId, cancellationToken);
            var remainingImages = refreshedProduct?.Images?
                .OrderBy(i => i.DisplayOrder)
                .ToList() ?? new List<ProductImage>();

            var newPrimary = remainingImages.FirstOrDefault();
            if (newPrimary != null)
            {
                newPrimary.IsPrimary = true;
                _productImageRepository.Update(newPrimary);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return await GetDetailAsync(productId, cancellationToken);
    }

    /// <summary>
    /// Déclenche l'analyse IA si le produit possède une composition exploitable.
    /// Garde la logique métier avancée, mais isole l'appel réel à l'interface IA.
    /// </summary>
    private async Task TryTriggerAiAnalysisAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);

        if (product == null)
            return;

        if (string.IsNullOrWhiteSpace(product.Composition))
            return;

        // IMPORTANT :
        // Adapte uniquement la ligne ci-dessous si le nom exact de la méthode
        // dans IAiAnalysisService est différent dans ton projet.
        await _aiAnalysisService.AnalyzeProductAsync(productId, cancellationToken);
    }

    // ═══════════════════════════════════════════════════════
    // PROPOSITION PRODUIT PAR CONSUMER
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Permet à un consommateur de proposer un produit qui sera traité par l'admin.
    /// Le produit n'est pas créé immédiatement dans la table Product.
    /// Une PendingRequest de type ProductAdd est créée à la place.
    /// </summary>
    public async Task<int> ProposeProductAsync(
        CreateProductDto dto,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await _unitOfWork.Users.GetByIdAsync(actorUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Utilisateur introuvable.");

        if (!actor.IsActive)
            throw new UnauthorizedAccessException("Votre compte est désactivé.");

        if (actor.Role != UserRole.Consumer)
            throw new UnauthorizedAccessException(
                "Seul un consommateur peut proposer un produit via ce flux.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Le nom du produit est obligatoire.");

        var productName = dto.Name.Trim();

        var subCategoryExists = await _subCategoryRepository.AnyAsync(
            sc => sc.Id == dto.SubCategoryId,
            cancellationToken);

        if (!subCategoryExists)
            throw new ArgumentException("Sous-catégorie introuvable.");

        if (dto.BrandId.HasValue)
        {
            var brandExists = await _brandRepository.AnyAsync(
                b => b.Id == dto.BrandId.Value,
                cancellationToken);

            if (!brandExists)
                throw new ArgumentException("Marque introuvable.");
        }

        await EnsureTagIdsExistAsync(dto.TagIds, cancellationToken);

        var payload = new ProductAddRequestPayload
        {
            BrandId = dto.BrandId,
            SubCategoryId = dto.SubCategoryId,
            Name = productName,
            Barcode = NormalizeOptional(dto.Barcode),
            Description = NormalizeOptional(dto.Description),
            Composition = NormalizeOptional(dto.Composition),
            OriginLevel = dto.OriginLevel?.Trim() ?? string.Empty,
            TagIds = dto.TagIds?.Distinct().ToList() ?? new List<int>()
        };

        var pendingRequest = new PendingRequest
        {
            SubmittedBy = actor.Id,
            RequestType = RequestType.ProductAdd,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PendingRequests.AddAsync(pendingRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pendingRequest.Id;
    }

    /// <summary>
    /// Vérifie que tous les TagIds demandés existent réellement.
    /// </summary>
    private async Task EnsureTagIdsExistAsync(
        IEnumerable<int>? tagIds,
        CancellationToken cancellationToken = default)
    {
        if (tagIds == null)
            return;

        var normalizedTagIds = tagIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (normalizedTagIds.Count == 0)
            return;

        var existingTags = await _tagRepository.FindAsync(
            t => normalizedTagIds.Contains(t.Id),
            cancellationToken);

        var existingIds = existingTags
            .Select(t => t.Id)
            .ToHashSet();

        var missingIds = normalizedTagIds
            .Where(id => !existingIds.Contains(id))
            .ToList();

        if (missingIds.Count > 0)
            throw new ArgumentException(
                $"Les tags suivants sont introuvables : {string.Join(", ", missingIds)}");
    }

    /// <summary>
    /// Payload utilisé pour les demandes admin de type ProductAdd.
    /// </summary>
    private sealed class ProductAddRequestPayload
    {
        public int? BrandId { get; set; }
        public int SubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? Composition { get; set; }
        public string OriginLevel { get; set; } = string.Empty;
        public List<int> TagIds { get; set; } = new();
    }
}