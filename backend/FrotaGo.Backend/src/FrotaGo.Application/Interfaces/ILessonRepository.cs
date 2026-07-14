using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lesson>> GetAllAsync();
    Task AddAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(Lesson lesson);
}
