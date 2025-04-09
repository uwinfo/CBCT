using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 權限對應角色
/// </summary>
public partial class AdminRoleAdminPermission
{
    public long Id { get; set; }

    public string AdminRoleUid { get; set; } = null!;

    public string PermissionUid { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string ModifierUid { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}
