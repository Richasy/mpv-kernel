// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;

namespace Richasy.MpvKernel.WinUI;

public partial class MpvPlayerWindow
{
    /// <summary>
    /// 该方法旨在定时检查播放器状态或执行其他定时任务.
    /// </summary>
    private async void OnAutoCheckTimerTick(DispatcherQueueTimer sender, object args)
    {
        if (IsDisposed || _client?.IsDisposed != false)
        {
            return;
        }

        var stateResult = await _client.GetPlayerStateAsync();
        if (stateResult.IsFailed)
        {
            return;
        }

        HandleInteractiveNotify(MpvUIEventId.StateChecked, stateResult.Value);
    }

    private void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidSizeChange || args.DidVisibilityChange || args.DidPresenterChange)
        {
            UpdateXamlSourcePosition();
        }
    }

    private async void OnWindowDestroying(AppWindow sender, object args) => await DisposeAsync();
}
