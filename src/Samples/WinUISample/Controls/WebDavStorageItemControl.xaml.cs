// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.WinUIKernel.Share.Base;
using WinUISample.ViewModels;

namespace WinUISample.Controls;

/// <summary>
/// WebDAV 存储项控件.
/// </summary>
public sealed partial class WebDavStorageItemControl : WebDavStorageItemControlBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDavStorageItemControl"/> class.
    /// </summary>
    public WebDavStorageItemControl() => InitializeComponent();
}

/// <summary>
/// WebDAV 存储项控件基类.
/// </summary>
public abstract class WebDavStorageItemControlBase : LayoutUserControlBase<WebDavStorageItemViewModel>;