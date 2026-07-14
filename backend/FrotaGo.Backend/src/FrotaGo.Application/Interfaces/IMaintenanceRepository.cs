using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IMaintenanceRepository
{
    Task<Maintenance?> GetByIdAsync(Guid id);
    Task<IEnumerable<Maintenance>> GetAllAsync();
    Task AddAsync(Maintenance maintenance);
    Task UpdateAsync(Maintenance maintenance);
    Task DeleteAsync(Maintenance maintenance);
}
