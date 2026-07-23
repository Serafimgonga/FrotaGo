using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using MediatR;

namespace FrotaGo.Application.Features.Authentication;

public record UpdateSchoolPlanCommand(Guid SchoolId, string NewPlan) : IRequest<bool>;

public class UpdateSchoolPlanCommandHandler : IRequestHandler<UpdateSchoolPlanCommand, bool>
{
    private readonly ISchoolRepository _schoolRepository;

    public UpdateSchoolPlanCommandHandler(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<bool> Handle(UpdateSchoolPlanCommand request, CancellationToken cancellationToken)
    {
        var school = await _schoolRepository.GetByIdAsync(request.SchoolId);
        if (school == null)
        {
            throw new Exception("Escola não encontrada.");
        }

        school.Plan = request.NewPlan;
        if (request.NewPlan == "Gratuito")
        {
            school.TrialExpiresAt = DateTime.UtcNow.AddDays(30);
            school.Status = Domain.Enums.SchoolStatus.Approved; // Plano Gratuito desbloqueia imediatamente o Dashboard por 30 dias
        }
        else
        {
            school.TrialExpiresAt = null; // Planos pagos não têm contagem de trial
        }

        await _schoolRepository.UpdateAsync(school);
        return true;
    }
}
