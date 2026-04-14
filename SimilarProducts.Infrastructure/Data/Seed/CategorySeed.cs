using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Infrastructure.Data.Seed;

public static class CategorySeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Alimentaire", DisplayOrder = 1 },
            new Category { Id = 2, Name = "Hygiène et soins", DisplayOrder = 2 },
            new Category { Id = 3, Name = "Ménager", DisplayOrder = 3 },
            new Category { Id = 4, Name = "Bébé et enfant", DisplayOrder = 4 }
        );

        modelBuilder.Entity<SubCategory>().HasData(
            // Alimentaire
            new SubCategory { Id = 1, CategoryId = 1, Name = "Boissons" },
            new SubCategory { Id = 2, CategoryId = 1, Name = "Produits laitiers" },
            new SubCategory { Id = 3, CategoryId = 1, Name = "Épicerie" },
            new SubCategory { Id = 4, CategoryId = 1, Name = "Surgelés" },
            new SubCategory { Id = 5, CategoryId = 1, Name = "Pâtes" },
            new SubCategory { Id = 6, CategoryId = 1, Name = "Conserves" },
            new SubCategory { Id = 7, CategoryId = 1, Name = "Sauces" },
            new SubCategory { Id = 8, CategoryId = 1, Name = "Huiles" },
            new SubCategory { Id = 9, CategoryId = 1, Name = "Céréales" },
            new SubCategory { Id = 10, CategoryId = 1, Name = "Biscuits et gâteaux" },
            new SubCategory { Id = 11, CategoryId = 1, Name = "Sucre et miel" },
            new SubCategory { Id = 12, CategoryId = 1, Name = "Épices" },
            new SubCategory { Id = 13, CategoryId = 1, Name = "Harissa et condiments" },
            new SubCategory { Id = 14, CategoryId = 1, Name = "Fruits secs" },

            // Hygiène et soins
            new SubCategory { Id = 15, CategoryId = 2, Name = "Shampooing" },
            new SubCategory { Id = 16, CategoryId = 2, Name = "Savon et gel douche" },
            new SubCategory { Id = 17, CategoryId = 2, Name = "Dentifrice" },
            new SubCategory { Id = 18, CategoryId = 2, Name = "Déodorant" },
            new SubCategory { Id = 19, CategoryId = 2, Name = "Soins du visage" },
            new SubCategory { Id = 20, CategoryId = 2, Name = "Soins du corps" },

            // Ménager
            new SubCategory { Id = 21, CategoryId = 3, Name = "Détergent" },
            new SubCategory { Id = 22, CategoryId = 3, Name = "Nettoyant" },
            new SubCategory { Id = 23, CategoryId = 3, Name = "Lessive" },
            new SubCategory { Id = 24, CategoryId = 3, Name = "Vaisselle" },
            new SubCategory { Id = 25, CategoryId = 3, Name = "Entretien maison" },

            // Bébé et enfant
            new SubCategory { Id = 26, CategoryId = 4, Name = "Couches" },
            new SubCategory { Id = 27, CategoryId = 4, Name = "Lingettes" },
            new SubCategory { Id = 28, CategoryId = 4, Name = "Lait infantile" },
            new SubCategory { Id = 29, CategoryId = 4, Name = "Petits pots et repas bébé" },
            new SubCategory { Id = 30, CategoryId = 4, Name = "Hygiène bébé" }
        );
    }
}