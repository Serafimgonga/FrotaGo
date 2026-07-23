using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.School)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.School)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByInvitationTokenAsync(string token)
    {
        return await _context.Users
            .Include(u => u.School)
            .FirstOrDefaultAsync(u => u.InvitationToken == token);
    }

    public async Task<IEnumerable<User>> GetBySchoolIdAsync(Guid schoolId)
    {
        return await _context.Users
            .Include(u => u.School)
            .Where(u => u.SchoolId == schoolId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
