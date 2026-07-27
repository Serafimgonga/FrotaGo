using System;
using System.Security.Claims;
using FrotaGo.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FrotaGo.Infrastructure.Authentication;

/// <summary>
/// Lê o SchoolId do claim "schoolId" do token JWT via IHttpContextAccessor.
/// Registado como Scoped no DI para acompanhar o ciclo de vida do pedido HTTP.
/// </summary>
public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? SchoolId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("schoolId");
            if (claim != null && Guid.TryParse(claim.Value, out var schoolId))
            {
                return schoolId;
            }
            return null;
        }
    }
}
