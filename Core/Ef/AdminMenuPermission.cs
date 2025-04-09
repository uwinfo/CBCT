using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 後台menu的權限設定
/// </summary>
public partial class AdminMenuPermission
{
    public long Id { get; set; }

    public string AdminMenuUid { get; set; } = null!;

    public string PermissionUid { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string ModifierUid { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}
