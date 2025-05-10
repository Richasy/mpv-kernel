// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.Player;
using Richasy.MpvKernel.Player.Models;

namespace WinUISample.Models;

internal sealed class LocalMediaSourceResolver(string filePath) : IMpvMediaSourceResolver
{
    public IntPtr WindowHandle { get; set; }

    public Task<MpvMediaSource> GetSourceAsync()
    {
        var options = new MpvPlayOptions
        {
            WindowHandle = WindowHandle,
        };
        var title = Path.GetFileNameWithoutExtension(filePath);
        var id = Path.GetFileName(filePath);
        var source = new MpvMediaSource(filePath, id, title, options: options);
        return Task.FromResult(source);
    }
}
