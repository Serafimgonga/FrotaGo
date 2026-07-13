using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using MediatR;

namespace FrotaGo.Application.Features.Authentication;

public record LoginQuery(
    string Email,
    string Password
) : IRequest<LoginResult>;

public record LoginResult(
    string Token,
    string Name,
    string Email,
    string Role
);

public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new Exception("Credenciais inválidas.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception("Credenciais inválidas.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResult(
            token,
            user.Name,
            user.Email,
            user.Role.ToString()
        );
    }
}
