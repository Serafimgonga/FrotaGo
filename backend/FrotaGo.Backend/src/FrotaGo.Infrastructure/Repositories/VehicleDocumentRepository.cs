using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class VehicleDocumentRepository : IVehicleDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleDocument?> GetByIdAsync(Guid id)
    {
        return await _context.VehicleDocuments
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<VehicleDocument>> GetAllAsync()
    {
        return await _context.VehicleDocuments
            .Include(d => d.Vehicle)
            .ToListAsync();
    }

    public async Task AddAsync(VehicleDocument document)
    {
        await _context.VehicleDocuments.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VehicleDocument document)
    {
        _context.VehicleDocuments.Update(document);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(VehicleDocument document)
    {
        _context.VehicleDocuments.Remove(document);
        await _context.SaveChangesAsync();
    }
}
