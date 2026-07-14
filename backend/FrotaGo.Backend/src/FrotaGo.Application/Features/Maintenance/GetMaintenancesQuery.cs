using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using MediatR;

namespace FrotaGo.Application.Features.Maintenance;

public record GetMaintenancesQuery : IRequest<IEnumerable<FrotaGo.Domain.Entities.Maintenance>>;

public class GetMaintenancesQueryHandler : IRequestHandler<GetMaintenancesQuery, IEnumerable<FrotaGo.Domain.Entities.Maintenance>>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public GetMaintenancesQueryHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<IEnumerable<FrotaGo.Domain.Entities.Maintenance>> Handle(GetMaintenancesQuery request, CancellationToken cancellationToken)
    {
        return await _maintenanceRepository.GetAllAsync();
    }
}
