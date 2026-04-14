using FluentValidation;
using SimilarProducts.Application.DTOs.Auth;

namespace SimilarProducts.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Le nom complet est obligatoire.")
            .MaximumLength(150).WithMessage("Le nom complet ne doit pas dépasser 150 caractères.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est obligatoire.")
            .EmailAddress().WithMessage("Le format de l'email est invalide.")
            .MaximumLength(256).WithMessage("L'email ne doit pas dépasser 256 caractères.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Le numéro de téléphone ne doit pas dépasser 20 caractères.")
            .Matches(@"^\+?[0-9\s\-()]*$")
            .WithMessage("Le format du numéro de téléphone est invalide.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

public class RegisterBusinessOwnerValidator : AbstractValidator<RegisterBusinessOwnerDto>
{
    public RegisterBusinessOwnerValidator()
    {
        Include(new RegisterValidator());

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Le nom de l'entreprise est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom de l'entreprise ne doit pas dépasser 200 caractères.");

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("L'identifiant fiscal ne doit pas dépasser 50 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));
    }
}