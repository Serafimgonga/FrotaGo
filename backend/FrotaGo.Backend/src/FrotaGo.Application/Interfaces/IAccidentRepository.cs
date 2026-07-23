using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IAccidentRepository
{
    Task<Accident?> GetByIdAsync(Guid id);
    Task<IEnumerable<Accident>> GetAllAsync();
    Task AddAsync(Accident accident);
    Task UpdateAsync(Accident accident);
    Task DeleteAsync(Accident accident);
}
