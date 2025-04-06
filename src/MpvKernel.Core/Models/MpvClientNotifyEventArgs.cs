// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Enums;

namespace Richasy.MpvKernel.Core.Models;

/// <summary>
/// MPV 客户端通知事件参数.
/// </summary>
public sealed class MpvClientNotifyEventArgs(MpvClientEventId id, object data) : EventArgs
{
    /// <summary>
    /// 事件 ID.
    /// </summary>
    public MpvClientEventId Id { get; set; } = id;

    /// <summary>
    /// 数据.
    /// </summary>
    public object Data { get; set; } = data;
}
