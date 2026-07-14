using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly ApplicationDbContext _context;

    public MaintenanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Maintenance?> GetByIdAsync(Guid id)
    {
        return await _context.Maintenances
            .Include(m => m.Vehicle)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Maintenance>> GetAllAsync()
    {
        return await _context.Maintenances
            .Include(m => m.Vehicle)
            .ToListAsync();
    }

    public async Task AddAsync(Maintenance maintenance)
    {
        await _context.Maintenances.AddAsync(maintenance);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Maintenance maintenance)
    {
        _context.Maintenances.Update(maintenance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Maintenance maintenance)
    {
        _context.Maintenances.Remove(maintenance);
        await _context.SaveChangesAsync();
    }
}
