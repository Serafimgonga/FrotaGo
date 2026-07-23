using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByInvitationTokenAsync(string token);
    Task<IEnumerable<User>> GetBySchoolIdAsync(Guid schoolId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
