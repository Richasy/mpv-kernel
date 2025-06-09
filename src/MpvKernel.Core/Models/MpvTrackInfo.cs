// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// 轨道信息.
/// </summary>
public sealed class MpvTrackInfo
{
    /// <summary>
    /// 类型.
    /// </summary>
    public MpvTrackType Type { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 标识.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 语言.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 当前轨道.
    /// </summary>
    public bool Current { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MpvTrackInfo info && Id == info.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
