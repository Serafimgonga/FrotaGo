using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IFuelRecordRepository
{
    Task<FuelRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<FuelRecord>> GetAllAsync();
    Task AddAsync(FuelRecord record);
    Task UpdateAsync(FuelRecord record);
    Task DeleteAsync(FuelRecord record);
}
