using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class AccidentRepository : IAccidentRepository
{
    private readonly ApplicationDbContext _context;

    public AccidentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Accident?> GetByIdAsync(Guid id)
    {
        return await _context.Accidents
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Accident>> GetAllAsync()
    {
        return await _context.Accidents
            .Include(a => a.Vehicle)
            .ToListAsync();
    }

    public async Task AddAsync(Accident accident)
    {
        await _context.Accidents.AddAsync(accident);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Accident accident)
    {
        _context.Accidents.Update(accident);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Accident accident)
    {
        _context.Accidents.Remove(accident);
        await _context.SaveChangesAsync();
    }
}
