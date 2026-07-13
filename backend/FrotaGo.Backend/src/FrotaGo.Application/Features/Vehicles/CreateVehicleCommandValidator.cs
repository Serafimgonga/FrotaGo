using FluentValidation;

namespace FrotaGo.Application.Features.Vehicles;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().WithMessage("A matrícula é obrigatória.");
        RuleFor(x => x.Brand).NotEmpty().WithMessage("A marca é obrigatória.");
        RuleFor(x => x.Model).NotEmpty().WithMessage("O modelo é obrigatório.");
        RuleFor(x => x.Chassis).NotEmpty().WithMessage("O chassis é obrigatório.");
        RuleFor(x => x.Year).GreaterThan(1900).WithMessage("Ano inválido.");
        RuleFor(x => x.Odometer).GreaterThanOrEqualTo(0).WithMessage("A quilometragem não pode ser negativa.");
        RuleFor(x => x.Fuel).IsInEnum().WithMessage("Tipo de combustível inválido.");
        RuleFor(x => x.Transmission).IsInEnum().WithMessage("Tipo de caixa de velocidades inválido.");
    }
}
