using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IVehicleDocumentRepository
{
    Task<VehicleDocument?> GetByIdAsync(Guid id);
    Task<IEnumerable<VehicleDocument>> GetAllAsync();
    Task AddAsync(VehicleDocument document);
    Task UpdateAsync(VehicleDocument document);
    Task DeleteAsync(VehicleDocument document);
}
