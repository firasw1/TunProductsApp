using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Infrastructure.Data.Seed;

public static class ThemeSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Theme>().HasData(
            new Theme
            {
                Id = 1,
                Name = "100% Tunisien",
                Type = ThemeType.System,
                IsActive = true,
                Description = "Trouver les alternatives 100% fabriquées en Tunisie",
                WeightStructural = 0.60,
                WeightTags = 0.10,
                WeightAi = 0.30,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Theme
            {
                Id = 2,
                Name = "Bio et naturel",
                Type = ThemeType.System,
                IsActive = true,
                Description = "Produits biologiques et naturels avec moins d'additifs artificiels",
                WeightStructural = 0.20,
                WeightTags = 0.20,
                WeightAi = 0.60,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Theme
            {
                Id = 3,
                Name = "Économique",
                Type = ThemeType.System,
                IsActive = true,
                Description = "Trouver le meilleur rapport qualité-prix",
                WeightStructural = 0.60,
                WeightTags = 0.10,
                WeightAi = 0.30,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Theme
            {
                Id = 4,
                Name = "Sans gluten",
                Type = ThemeType.System,
                IsActive = true,
                Description = "Alternatives adaptées aux régimes sans gluten",
                WeightStructural = 0.20,
                WeightTags = 0.60,
                WeightAi = 0.20,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Theme
            {
                Id = 5,
                Name = "Santé",
                Type = ThemeType.System,
                IsActive = true,
                Description = "Produits avec un meilleur profil santé et moins d'additifs à risque",
                WeightStructural = 0.10,
                WeightTags = 0.20,
                WeightAi = 0.70,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<ThemeTag>().HasData(
            // 100% Tunisien
            new ThemeTag { Id = 1, ThemeId = 1, TagId = 16 }, // tunisien
            new ThemeTag { Id = 2, ThemeId = 1, TagId = 17 }, // local_sfax
            new ThemeTag { Id = 3, ThemeId = 1, TagId = 18 }, // local_tunis

            // Bio et naturel
            new ThemeTag { Id = 4, ThemeId = 2, TagId = 1 },  // bio
            new ThemeTag { Id = 5, ThemeId = 2, TagId = 8 },  // naturel
            new ThemeTag { Id = 6, ThemeId = 2, TagId = 9 },  // sans_conservateur
            new ThemeTag { Id = 7, ThemeId = 2, TagId = 10 }, // sans_colorant

            // Économique
            new ThemeTag { Id = 8, ThemeId = 3, TagId = 12 }, // economique

            // Sans gluten
            new ThemeTag { Id = 9, ThemeId = 4, TagId = 4 },  // sans_gluten

            // Santé
            new ThemeTag { Id = 10, ThemeId = 5, TagId = 8 },  // naturel
            new ThemeTag { Id = 11, ThemeId = 5, TagId = 9 },  // sans_conservateur
            new ThemeTag { Id = 12, ThemeId = 5, TagId = 10 }, // sans_colorant
            new ThemeTag { Id = 13, ThemeId = 5, TagId = 6 }   // sans_sucre
        );
    }
}