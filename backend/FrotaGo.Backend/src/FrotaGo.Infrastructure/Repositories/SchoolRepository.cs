using System;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class SchoolRepository : ISchoolRepository
{
    private readonly ApplicationDbContext _context;

    public SchoolRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<School?> GetByIdAsync(Guid id)
    {
        return await _context.Schools.FindAsync(id);
    }

    public async Task<School?> GetByNifAsync(string nif)
    {
        return await _context.Schools.FirstOrDefaultAsync(s => s.NIF == nif);
    }

    public async Task AddAsync(School school)
    {
        await _context.Schools.AddAsync(school);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(School school)
    {
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
    }
}
