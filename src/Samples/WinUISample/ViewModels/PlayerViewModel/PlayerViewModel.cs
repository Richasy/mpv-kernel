// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Enums;
using Richasy.WinUIKernel.Share.Toolkits;
using Richasy.WinUIKernel.Share.ViewModels;
using System.ComponentModel;
using WinUISample.Models;

namespace WinUISample.ViewModels;

/// <summary>
/// 播放器视图模型.
/// </summary>
public sealed partial class PlayerViewModel(
        ILogger<PlayerViewModel> logger,
        ISettingsToolkit settingsToolkit) : ViewModelBase, IAsyncDisposable
{
    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_client?.IsDisposed == false)
        {
            await _client.DisposeAsync();
        }

        if (Player is not null)
        {
            Player.PropertyChanged -= OnPlayerPropertyChanged;
            await Player.DisposeAsync();
        }

        if (_playerWindow?.IsDisposed == false)
        {
            _playerWindow.GetWindow().Destroying -= OnWindowDestroying;
            await _playerWindow.DisposeAsync();
        }

        this.Get<AppViewModel>().Players.Remove(this);
    }

    /// <summary>
    /// 初始化.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    public async Task InitializeAsync(string localFilePath)
    {
        await InitializeInternalAsync();
        var sourceResolver = new LocalMediaSourceResolver(localFilePath);
        sourceResolver.WindowHandle = _playerWindow.Handle;
        _playerWindow.GetWindow().Title = Path.GetFileNameWithoutExtension(localFilePath);
        if (Player is not null)
        {
            Player.PropertyChanged -= OnPlayerPropertyChanged;
            await Player.DisposeAsync();
        }

        Player = new Richasy.MpvKernel.Player.MpvPlayer(_client, sourceResolver, logger: logger);
        Player.PropertyChanged += OnPlayerPropertyChanged;
        await Player.InitializeAsync();
        _playerWindow.Show();
    }

    /// <summary>
    /// 初始化播放器.
    /// </summary>
    /// <returns><see cref="Task"/>.</returns>
    private async Task InitializeInternalAsync()
    {
        if (_client is not null)
        {
            return;
        }

        var mpvDllPath = settingsToolkit.ReadLocalSetting("LibMpvPath", string.Empty);
        _client = await MpvClient.CreateAsync(mpvDllPath, logger: logger);

        if (_playerWindow?.IsDisposed != false)
        {
            _playerWindow = new Forms.MpvPlayerWindow(this);
            _playerWindow.GetWindow().Destroying += OnWindowDestroying;
        }
    }

    /// <summary>
    /// 改变当前播放位置.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public async Task ChangePositionAsync(double position)
    {
        if (Player is null || position < 0 || position > Player.Duration || Math.Abs(Player.Position - position) < 2)
        {
            return;
        }

        await _client.SetCurrentPositionAsync(position);
    }

    [RelayCommand]
    private async Task PlayPauseAsync()
    {
        if (Player is null || Player.PlaybackState == MpvPlayerState.Idle)
        {
            return;
        }

        if (Player.PlaybackState == MpvPlayerState.Playing)
        {
            await _client.PauseAsync();
        }
        else if (Player.IsStopped)
        {
            await Player.ReplayAsync();
        }
        else
        {
            await _client.ResumeAsync();
        }
    }

    private async void OnWindowDestroying(AppWindow sender, object args)
        => await DisposeAsync();

    private void OnPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Player.Position))
        {
            PositionText = TimeSpan.FromSeconds(Player.Position).ToString(@"hh\:mm\:ss");
        }
        else if (e.PropertyName == nameof(Player.Duration))
        {
            DurationText = TimeSpan.FromSeconds(Player.Duration).ToString(@"hh\:mm\:ss");
        }
    }
}
