using Microsoft.EntityFrameworkCore;

namespace SimilarProducts.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        CategorySeed.Seed(modelBuilder);
        TagSeed.Seed(modelBuilder);
        ThemeSeed.Seed(modelBuilder);
    }
}