//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using SimilarProducts.Domain.Common;
//using SimilarProducts.Domain.Interfaces.Repositories;
//using SimilarProducts.Domain.Interfaces.Services;
//using SimilarProducts.Infrastructure.Data;
//using SimilarProducts.Infrastructure.ExternalServices;
//using SimilarProducts.Infrastructure.Repositories;
//using System;

//namespace SimilarProducts.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructure(
//        this IServiceCollection services, IConfiguration configuration)
//    {
//        // ── Database ────────────────────────────────────────
//        services.AddDbContext<AppDbContext>(options =>
//            options.UseSqlServer(
//                configuration.GetConnectionString("DefaultConnection"),
//                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

//        // ── Repositories ────────────────────────────────────
//        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
//        services.AddScoped<IProductRepository, ProductRepository>();
//        services.AddScoped<IReviewRepository, ReviewRepository>();
//        services.AddScoped<IUnitOfWork, UnitOfWork>();

//        // ── External services ───────────────────────────────
//        services.AddHttpClient<IOpenFoodFactsService, OpenFoodFactsService>(client =>
//        {
//            client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
//            client.DefaultRequestHeaders.Add("User-Agent", "SimilarProducts-PFE/1.0");
//        });

//        return services;
//    }
//}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimilarProducts.Domain.Common;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;
using SimilarProducts.Infrastructure.Repositories;

namespace SimilarProducts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories génériques et spécifiques
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IThemeRepository, ThemeRepository>();
        services.AddScoped<IScrapedProductRepository, ScrapedProductRepository>();
        services.AddScoped<IPendingRequestRepository, PendingRequestRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}