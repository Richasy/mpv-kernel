// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;

namespace WinUISample.ViewModels;

public sealed partial class PlayerViewModel
{
    [RelayCommand]
    private async Task IncreaseVolumeAsync()
    {
        var newVolume = Math.Min(100, Volume + 5);
        if (Math.Abs(newVolume - Volume) > 1)
        {
            Volume = newVolume;
            await Client.SetVolumeAsync(newVolume);
        }
    }

    [RelayCommand]
    private async Task DecreaseVolumeAsync()
    {
        var newVolume = Math.Max(0, Volume - 5);
        if (Math.Abs(newVolume - Volume) > 1)
        {
            Volume = newVolume;
            await Client.SetVolumeAsync(newVolume);
        }
    }

    [RelayCommand]
    private async Task IncreaseSpeedAsync()
    {
        var newSpeed = Math.Min(2, Speed + 0.25);
        if (Math.Abs(newSpeed - Speed) > 0.01)
        {
            Speed = newSpeed;
            await Client.SetSpeedAsync(newSpeed);
        }
    }

    [RelayCommand]
    private async Task DecreaseSpeedAsync()
    {
        var newSpeed = Math.Max(0.1, Speed - 0.25);
        if (Math.Abs(newSpeed - Speed) > 0.01)
        {
            Speed = newSpeed;
            await Client.SetSpeedAsync(newSpeed);
        }
    }

    [RelayCommand]
    private async Task BackToDefaultModeAsync()
    {
        if (IsFullScreen)
        {
            await Client.SetFullScreenStateAsync(false);
        }
        else if (IsCompactOverlay)
        {
            await Client.SetCompactOverlayStateAsync(false);
        }
    }

    [RelayCommand]
    private async Task ForwardSkipAsync()
    {
        var seconds = 15d;
        if (seconds <= 0 || CurrentPosition <= 0)
        {
            return;
        }

        var pos = Math.Min(Duration, CurrentPosition + seconds);
        if (Math.Abs(pos - CurrentPosition) > 1)
        {
            await Client.SetCurrentPositionAsync(pos);
        }
    }

    [RelayCommand]
    private async Task BackwardSkipAsync()
    {
        var seconds = 15d;
        if (seconds <= 0 || CurrentPosition <= 0)
        {
            return;
        }

        var pos = Math.Max(0, CurrentPosition - seconds);
        await Client.SetCurrentPositionAsync(pos);
    }

    [RelayCommand]
    private async Task ToggleCompactOverlayAsync()
        => await Client.SetCompactOverlayStateAsync(!IsCompactOverlay);

    [RelayCommand]
    private async Task ToggleFullScreenAsync()
        => await Client.SetFullScreenStateAsync(!IsFullScreen);

    [RelayCommand]
    private void CloseWindow()
    {
        this.Get<AppViewModel>().PlayerWindows.Remove(this);
        _tipTimer.Stop();
        _tipTimer.Tick -= OnTipTimerTick;
        _tipTimer = default;
        Window.GetWindow().Closing -= OnWindowClosing;
        SaveCurrentWindowStats();
    }
}
