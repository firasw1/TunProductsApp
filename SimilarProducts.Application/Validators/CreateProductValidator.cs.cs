using FluentValidation;
using SimilarProducts.Application.DTOs.Products;

namespace SimilarProducts.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    private static readonly string[] AllowedOriginLevels =
    {
        "Imported",
        "TunisianMade",
        "TunisianAssembled"
    };

    public CreateProductValidator()
    {
        RuleFor(x => x.SubCategoryId)
            .GreaterThan(0)
            .WithMessage("La sous-catégorie est obligatoire.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du produit est obligatoire.")
            .MaximumLength(200)
            .WithMessage("Le nom du produit ne doit pas dépasser 200 caractères.");

        RuleFor(x => x.Barcode)
            .MaximumLength(50)
            .WithMessage("Le code-barres ne doit pas dépasser 50 caractères.")
            .Matches(@"^[0-9A-Za-z\-]+$")
            .WithMessage("Le code-barres contient des caractères invalides.")
            .When(x => !string.IsNullOrWhiteSpace(x.Barcode));

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("La description ne doit pas dépasser 2000 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Composition)
            .MaximumLength(4000)
            .WithMessage("La composition ne doit pas dépasser 4000 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.Composition));

        RuleFor(x => x.OriginLevel)
            .NotEmpty()
            .WithMessage("L'origine du produit est obligatoire.")
            .Must(BeAValidOriginLevel)
            .WithMessage("L'origine du produit est invalide.");

        RuleFor(x => x.BrandId)
            .GreaterThan(0)
            .WithMessage("La marque est invalide.")
            .When(x => x.BrandId.HasValue);

        RuleForEach(x => x.TagIds)
            .GreaterThan(0)
            .WithMessage("Un tag sélectionné est invalide.");

        RuleFor(x => x.TagIds)
            .Must(tags => tags == null || tags.Distinct().Count() == tags.Count)
            .WithMessage("La liste des tags contient des doublons.");
    }

    private static bool BeAValidOriginLevel(string originLevel)
    {
        return AllowedOriginLevels.Any(x =>
            x.Equals(originLevel?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}