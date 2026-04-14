using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Infrastructure.Data.Seed;

public static class TagSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().HasData(
            // Dietary
            new Tag { Id = 1, Name = "bio", Type = TagType.Dietary },
            new Tag { Id = 2, Name = "halal", Type = TagType.Dietary },
            new Tag { Id = 3, Name = "vegan", Type = TagType.Dietary },
            new Tag { Id = 4, Name = "sans_gluten", Type = TagType.Dietary },
            new Tag { Id = 5, Name = "sans_lactose", Type = TagType.Dietary },
            new Tag { Id = 6, Name = "sans_sucre", Type = TagType.Dietary },
            new Tag { Id = 7, Name = "vegetarien", Type = TagType.Dietary },

            // Attribute
            new Tag { Id = 8, Name = "naturel", Type = TagType.Attribute },
            new Tag { Id = 9, Name = "sans_conservateur", Type = TagType.Attribute },
            new Tag { Id = 10, Name = "sans_colorant", Type = TagType.Attribute },
            new Tag { Id = 11, Name = "artisanal", Type = TagType.Attribute },
            new Tag { Id = 12, Name = "economique", Type = TagType.Attribute },
            new Tag { Id = 13, Name = "premium", Type = TagType.Attribute },
            new Tag { Id = 14, Name = "ramadan", Type = TagType.Attribute },
            new Tag { Id = 15, Name = "saisonnier", Type = TagType.Attribute },

            // Origin
            new Tag { Id = 16, Name = "tunisien", Type = TagType.Origin },
            new Tag { Id = 17, Name = "local_sfax", Type = TagType.Origin },
            new Tag { Id = 18, Name = "local_tunis", Type = TagType.Origin },
            new Tag { Id = 19, Name = "maghrebin", Type = TagType.Origin },
            new Tag { Id = 20, Name = "europeen", Type = TagType.Origin }
        );
    }
}