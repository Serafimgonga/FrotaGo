using System;

namespace FrotaGo.Application.Interfaces;

/// <summary>
/// Fornece o SchoolId do utilizador autenticado actual para isolamento de dados multi-tenant.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// SchoolId extraído do token JWT do utilizador autenticado.
    /// Null para Super Admins (acesso global) ou utilizadores sem escola associada.
    /// </summary>
    Guid? SchoolId { get; }
}
