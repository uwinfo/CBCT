using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 後台管理角色
/// </summary>
public partial class AdminRole
{
    public string Uid { get; set; } = null!;

    public long Id { get; set; }

    /// <summary>
    /// 群組名稱
    /// </summary>
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string ModifierUid { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }

    public long Sort { get; set; }
}
