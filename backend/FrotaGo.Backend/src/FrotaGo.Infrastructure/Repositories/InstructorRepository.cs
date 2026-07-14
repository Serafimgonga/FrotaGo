using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly ApplicationDbContext _context;

    public InstructorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Instructor?> GetByIdAsync(Guid id)
    {
        return await _context.Instructors.FindAsync(id);
    }

    public async Task<IEnumerable<Instructor>> GetAllAsync()
    {
        return await _context.Instructors.ToListAsync();
    }

    public async Task AddAsync(Instructor instructor)
    {
        await _context.Instructors.AddAsync(instructor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Instructor instructor)
    {
        _context.Instructors.Update(instructor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Instructor instructor)
    {
        _context.Instructors.Remove(instructor);
        await _context.SaveChangesAsync();
    }
}
