// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace MpvKernel.WinUI;

/// <summary>
/// MPV UI 通知事件参数.
/// </summary>
public sealed class MpvUINotifyEventArgs(MpvUIEventId id, object data) : EventArgs
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public MpvUIEventId Id { get; set; } = id;

    /// <summary>
    /// 数据.
    /// </summary>
    public object Data { get; set; } = data;
}
