using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 權限
/// </summary>
public partial class AdminPermission
{
    public string Uid { get; set; } = null!;

    public long Id { get; set; }

    /// <summary>
    /// 英文，權限
    /// </summary>
    public string Code { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 中文，權限的描述
    /// </summary>
    public string? Description { get; set; }

    public long Sort { get; set; }

    public string CreatorUid { get; set; } = null!;

    public string? ModifierUid { get; set; }
}
