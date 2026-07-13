using FluentValidation;

namespace FrotaGo.Application.Features.Authentication;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email inválido.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("A palavra-passe deve ter pelo menos 6 caracteres.");
        RuleFor(x => x.Role).IsInEnum().WithMessage("Perfil inválido.");
    }
}
