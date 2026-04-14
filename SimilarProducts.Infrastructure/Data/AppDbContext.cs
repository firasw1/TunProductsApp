using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ──────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<BusinessOwner> BusinessOwners => Set<BusinessOwner>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreLocation> StoreLocations => Set<StoreLocation>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductAiAnalysis> ProductAiAnalyses => Set<ProductAiAnalysis>();
    public DbSet<Theme> Themes => Set<Theme>();
    public DbSet<ThemeTag> ThemeTags => Set<ThemeTag>();
    public DbSet<ProductSimilarity> ProductSimilarities => Set<ProductSimilarity>();
    public DbSet<StoreProduct> StoreProducts => Set<StoreProduct>();
    public DbSet<ScrapedProduct> ScrapedProducts => Set<ScrapedProduct>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ScanHistory> ScanHistories => Set<ScanHistory>();
    public DbSet<PendingRequest> PendingRequests => Set<PendingRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Enum conversions (stored as string in DB) ───────
        modelBuilder.Entity<User>()
            .Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<BusinessOwner>()
            .Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Store>()
            .Property(e => e.StoreType).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.OriginLevel).HasConversion<string>().HasMaxLength(30);
            e.Property(p => p.DataSource).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        });
        modelBuilder.Entity<Tag>()
            .Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Theme>()
            .Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<ProductSimilarity>(e =>
        {
            e.Property(p => p.Source).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        });
        modelBuilder.Entity<ScrapedProduct>()
            .Property(e => e.MatchStatus).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<ScanHistory>()
            .Property(e => e.FoundIn).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<PendingRequest>(e =>
        {
            e.Property(p => p.RequestType).HasConversion<string>().HasMaxLength(30);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        });

        // ═══════════════════════════════════════════════════════
        // USER
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(e => e.FavoriteTheme)
                  .WithMany(t => t.FavoritedByUsers)
                  .HasForeignKey(e => e.FavoriteThemeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ═══════════════════════════════════════════════════════
        // BUSINESS_OWNER (1:1 with User)
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<BusinessOwner>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.DocumentsUrl).HasMaxLength(500);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.BusinessOwner)
                  .HasForeignKey<BusinessOwner>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // BRAND
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CountryOrigin).HasMaxLength(50);

            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.Brands)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // STORE
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Store>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LogoUrl).HasMaxLength(500);

            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.Stores)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // STORE_LOCATION
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<StoreLocation>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(300).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.WorkingHours).HasMaxLength(500);

            entity.HasOne(e => e.Store)
                  .WithMany(s => s.Locations)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // CATEGORY / SUB_CATEGORY
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IconUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // PRODUCT (central table)
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
            entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.BarcodeCountry).HasMaxLength(5);

            entity.HasOne(e => e.Brand)
                  .WithMany(b => b.Products)
                  .HasForeignKey(e => e.BrandId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SubCategory)
                  .WithMany(sc => sc.Products)
                  .HasForeignKey(e => e.SubCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ═══════════════════════════════════════════════════════
        // PRODUCT_IMAGE
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // TAG / PRODUCT_TAG
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.TagId }).IsUnique();

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductTags)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.ProductTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // PRODUCT_AI_ANALYSIS (1:1 with Product)
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<ProductAiAnalysis>(entity =>
        {
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.Property(e => e.ModelVersion).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Product)
                  .WithOne(p => p.AiAnalysis)
                  .HasForeignKey<ProductAiAnalysis>(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // THEME / THEME_TAG
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IconUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<ThemeTag>(entity =>
        {
            entity.HasIndex(e => new { e.ThemeId, e.TagId }).IsUnique();

            entity.HasOne(e => e.Theme)
                  .WithMany(t => t.ThemeTags)
                  .HasForeignKey(e => e.ThemeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.ThemeTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // PRODUCT_SIMILARITY
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<ProductSimilarity>(entity =>
        {
            entity.HasOne(e => e.ProductA)
                  .WithMany(p => p.SimilaritiesAsSource)
                  .HasForeignKey(e => e.ProductAId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductB)
                  .WithMany(p => p.SimilaritiesAsTarget)
                  .HasForeignKey(e => e.ProductBId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Theme)
                  .WithMany(t => t.Similarities)
                  .HasForeignKey(e => e.ThemeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ═══════════════════════════════════════════════════════
        // STORE_PRODUCT
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<StoreProduct>(entity =>
        {
            entity.HasIndex(e => new { e.StoreLocationId, e.ProductId }).IsUnique();
            entity.Property(e => e.Price).HasColumnType("decimal(8,3)");

            entity.HasOne(e => e.StoreLocation)
                  .WithMany(sl => sl.StoreProducts)
                  .HasForeignKey(e => e.StoreLocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.StoreProducts)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // SCRAPED_PRODUCT
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<ScrapedProduct>(entity =>
        {
            entity.Property(e => e.StoreSource).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(8,3)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CategoryOnSite).HasMaxLength(200);
            entity.Property(e => e.UrlSource).HasMaxLength(500).IsRequired();

            entity.HasIndex(e => e.MatchedProductId);

            entity.HasOne(e => e.MatchedProduct)
                  .WithMany(p => p.MatchedScrapedProducts)
                  .HasForeignKey(e => e.MatchedProductId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ═══════════════════════════════════════════════════════
        // REVIEW
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════
        // SCAN_HISTORY
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<ScanHistory>(entity =>
        {
            entity.Property(e => e.BarcodeScanned).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ScannedAt);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.ScanHistories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ScanHistories)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ═══════════════════════════════════════════════════════
        // PENDING_REQUEST
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<PendingRequest>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestType);

            entity.HasOne(e => e.Submitter)
                  .WithMany(u => u.SubmittedRequests)
                  .HasForeignKey(e => e.SubmittedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reviewer)
                  .WithMany(u => u.ReviewedRequests)
                  .HasForeignKey(e => e.ReviewedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
