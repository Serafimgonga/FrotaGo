using System;

namespace FrotaGo.Domain.Entities;

public class RolePermission
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
