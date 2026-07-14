using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<IEnumerable<Student>> GetAllAsync();
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
}
