using FrotaGo.Domain.Entities;

namespace FrotaGo.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
