using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly ApplicationDbContext _context;

    public LessonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _context.Lessons
            .Include(l => l.Student)
            .Include(l => l.Instructor)
            .Include(l => l.Vehicle)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Lesson>> GetAllAsync()
    {
        return await _context.Lessons
            .Include(l => l.Student)
            .Include(l => l.Instructor)
            .Include(l => l.Vehicle)
            .ToListAsync();
    }

    public async Task AddAsync(Lesson lesson)
    {
        await _context.Lessons.AddAsync(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Lesson lesson)
    {
        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
    }
}
