using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 後台目錄
/// </summary>
public partial class AdminMenu
{
    public string Uid { get; set; } = null!;

    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Link { get; set; }

    public long Sort { get; set; }

    /// <summary>
    /// 第一層為#，後面的接對應的id
    /// </summary>
    public string? ParentUid { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string ModifierUid { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}
