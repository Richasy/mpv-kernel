// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Richasy.MpvKernel.Core.Models;
using Richasy.MpvKernel.Player.Models;
using Richasy.WinUIKernel.Share.Toolkits;
using System.Text;

namespace WinUISample.Models;

internal sealed class WebDavMediaSourceResolver(string filePath) : MediaSourceResolverBase
{
    public override async Task<MpvMediaSource> GetSourceAsync()
    {
        var config = await this.Get<IFileToolkit>().ReadLocalDataAsync("WebDavConfig.json", JsonGenContext.Default.WebDavConfig);
        var headers = new Dictionary<string, string>();
        var auth = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.UserName}:{config.Password}"))}";
        headers.Add("Authorization", auth);
        var options = new MpvPlayOptions
        {
            WindowHandle = WindowHandle,
            HttpHeaders = headers,
            EnableYtdl = false,
            EnableCookies = false
        };
        var title = Path.GetFileNameWithoutExtension(filePath);
        var id = Path.GetFileName(filePath);
        var source = new MpvMediaSource(filePath, id, title, options: options);
        return source;
    }
}
