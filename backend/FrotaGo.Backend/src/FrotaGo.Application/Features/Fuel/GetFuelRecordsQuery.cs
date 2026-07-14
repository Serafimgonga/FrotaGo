using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Fuel;

public record GetFuelRecordsQuery : IRequest<IEnumerable<FuelRecord>>;

public class GetFuelRecordsQueryHandler : IRequestHandler<GetFuelRecordsQuery, IEnumerable<FuelRecord>>
{
    private readonly IFuelRecordRepository _fuelRecordRepository;

    public GetFuelRecordsQueryHandler(IFuelRecordRepository fuelRecordRepository)
    {
        _fuelRecordRepository = fuelRecordRepository;
    }

    public async Task<IEnumerable<FuelRecord>> Handle(GetFuelRecordsQuery request, CancellationToken cancellationToken)
    {
        return await _fuelRecordRepository.GetAllAsync();
    }
}
