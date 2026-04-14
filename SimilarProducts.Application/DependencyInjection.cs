using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SimilarProducts.Application.Services;
using SimilarProducts.Application.Validators;

namespace SimilarProducts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services applicatifs
        services.AddScoped<AuthService>();

        // Validators FluentValidation
        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

        // Plus tard, quand tu auras MappingProfile.cs :
        // services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }
}