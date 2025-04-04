// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

namespace Richasy.MpvKernel.Win32;

public partial class MpvPlayerWindow
{
    private void OnClientShutdown(object? sender, EventArgs e) => Close();
}
