using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IInstructorRepository
{
    Task<Instructor?> GetByIdAsync(Guid id);
    Task<IEnumerable<Instructor>> GetAllAsync();
    Task AddAsync(Instructor instructor);
    Task UpdateAsync(Instructor instructor);
    Task DeleteAsync(Instructor instructor);
}
