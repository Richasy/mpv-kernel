// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace Richasy.MpvKernel.WinUI;

public sealed partial class MpvPlayerWindow
{
    /// <summary>
    /// UI 通知.
    /// </summary>
    public event EventHandler<MpvUINotifyEventArgs> UINotify;

    /// <summary>
    /// 对象是否已经被释放.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 窗口句柄.
    /// </summary>
    public IntPtr Handle { get; }

    /// <summary>
    /// XAML 根元素.
    /// </summary>
    public XamlRoot? XamlRoot => _xamlSource?.Content?.XamlRoot;
}
