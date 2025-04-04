// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using NativeWindow.Windowing;
using NativeWindow.Windowing.Events;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Models;

namespace WinUISample;

internal sealed class PlayerWindow(MpvClient client, WindowSettings gws) : Window(gws)
{
    private readonly MpvClient _client = client;

    public async Task InitializeAsync(string filePath)
    {
        var options = new MpvPlayOptions
        {
            WindowHandle = this.WindowHandler.Value
        };

        _client.Shutdown += OnShutdown;
        await _client.PlayAsync(filePath, options);
    }

    private async void OnShutdown(object? sender, EventArgs e)
    {
        _client.Shutdown -= OnShutdown;
        await _client.DisposeAsync();
        //Close();
    }

    protected override async void OnKeyboardKeyUp(KeyboardKeyEventArgs eventArgs)
    {
        if (eventArgs.Key == Keys.Space)
        {
            // 切换播放/暂停.
            var state = await _client?.GetPlayerStateAsync();
            if (state == Richasy.MpvKernel.Core.Enums.MpvPlayerState.Playing)
            {
                await _client.PauseAsync();
            }
            else if (state == Richasy.MpvKernel.Core.Enums.MpvPlayerState.Paused)
            {
                await _client.ResumeAsync();
            }
        }
    }
}
