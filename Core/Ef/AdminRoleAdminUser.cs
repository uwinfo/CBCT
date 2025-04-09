using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 角色管理員對應
/// </summary>
public partial class AdminRoleAdminUser
{
    public long Id { get; set; }

    public string AdminUserUid { get; set; } = null!;

    public string AdminRoleUid { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string ModifierUid { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}
