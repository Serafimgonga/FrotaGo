using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Vehicles;

public record GetVehicleByIdQuery(Guid Id) : IRequest<Vehicle?>;

public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, Vehicle?>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetVehicleByIdQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Vehicle?> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _vehicleRepository.GetByIdAsync(request.Id);
    }
}
