using System;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(Guid id);
    Task<School?> GetByNifAsync(string nif);
    Task AddAsync(School school);
    Task UpdateAsync(School school);
}
