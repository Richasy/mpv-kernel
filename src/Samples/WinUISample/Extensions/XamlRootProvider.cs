// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Richasy.WinUIKernel.Share;
using WinUISample.ViewModels;

namespace WinUISample.Extensions;

internal sealed class XamlRootProvider : IXamlRootProvider
{
    public XamlRoot? XamlRoot => this.Get<AppViewModel>().ActivateXamlRoot;
}
