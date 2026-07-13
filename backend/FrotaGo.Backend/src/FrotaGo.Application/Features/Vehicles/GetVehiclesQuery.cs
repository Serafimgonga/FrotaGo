using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Vehicles;

public record GetVehiclesQuery : IRequest<IEnumerable<Vehicle>>;

public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, IEnumerable<Vehicle>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<Vehicle>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        return await _vehicleRepository.GetAllAsync();
    }
}
