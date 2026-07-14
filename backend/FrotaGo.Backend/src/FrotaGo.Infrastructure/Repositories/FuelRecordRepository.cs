using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class FuelRecordRepository : IFuelRecordRepository
{
    private readonly ApplicationDbContext _context;

    public FuelRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FuelRecord?> GetByIdAsync(Guid id)
    {
        return await _context.FuelRecords
            .Include(f => f.Vehicle)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FuelRecord>> GetAllAsync()
    {
        return await _context.FuelRecords
            .Include(f => f.Vehicle)
            .ToListAsync();
    }

    public async Task AddAsync(FuelRecord record)
    {
        await _context.FuelRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FuelRecord record)
    {
        _context.FuelRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(FuelRecord record)
    {
        _context.FuelRecords.Remove(record);
        await _context.SaveChangesAsync();
    }
}
