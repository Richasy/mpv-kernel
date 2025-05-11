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
using WinUISample.Models.Constants;

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
    public async Task InitializeAsync(string filePath, SectionType type)
    {
        await InitializeInternalAsync();
        MediaSourceResolverBase sourceResolver = type is SectionType.Local
            ? new LocalMediaSourceResolver(filePath)
            : new BiliMediaSourceResolver(filePath);
        sourceResolver.WindowHandle = _playerWindow.Handle;
        if (Player is not null)
        {
            Player.PropertyChanged -= OnPlayerPropertyChanged;
            await Player.DisposeAsync();
        }

        Player = new Richasy.MpvKernel.Player.MpvPlayer(_client, sourceResolver, logger: logger);
        Player.PropertyChanged += OnPlayerPropertyChanged;
        await Player.InitializeAsync();
        _playerWindow.Show();
        CheckFullScreen();
        CheckCompactOverlay();
    }

    /// <summary>
    /// 关闭播放器.
    /// </summary>
    public void Close()
        => _playerWindow?.Close();

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

    /// <summary>
    /// 改变音量.
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public async Task ChangeVolumeAsync(double volume)
    {
        if (Player is null || volume < 0 || volume > 100 || Math.Abs(Player.Volume - volume) < 1)
        {
            return;
        }

        await _client.SetVolumeAsync(volume);
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

    [RelayCommand]
    private async Task ToggleFullScreenAsync()
    {
        if (Player is null)
        {
            return;
        }

        await _client.SetFullScreenStateAsync(!Player.IsFullScreen);
    }

    [RelayCommand]
    private async Task ToggleCompactOverlayAsync()
    {
        if (Player is null)
        {
            return;
        }

        await _client.SetCompactOverlayStateAsync(!Player.IsCompactOverlay);
    }

    [RelayCommand]
    private Task SkipBackward10Async()
        => ChangePositionAsync(Math.Max(Player.Position - 10, 0));

    [RelayCommand]
    private Task SkipForward30Async()
        => ChangePositionAsync(Math.Min(Player.Position + 30, Player.Duration - 1));

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
        else if (e.PropertyName == nameof(Player.IsFullScreen))
        {
            CheckFullScreen();
        }
        else if (e.PropertyName == nameof(Player.IsCompactOverlay))
        {
            CheckCompactOverlay();
        }
        else if (e.PropertyName == nameof(Player.Title))
        {
            _playerWindow.GetWindow().Title = Player.Title;
        }
    }

    private void CheckFullScreen()
    {
        if (Player.IsFullScreen && _playerWindow.GetWindow().Presenter.Kind != AppWindowPresenterKind.FullScreen)
        {
            _playerWindow.GetWindow().SetPresenter(AppWindowPresenterKind.FullScreen);
        }
        else if (!Player.IsFullScreen && _playerWindow.GetWindow().Presenter.Kind == AppWindowPresenterKind.FullScreen)
        {
            _playerWindow.GetWindow().SetPresenter(AppWindowPresenterKind.Default);
        }
    }

    private void CheckCompactOverlay()
    {
        if (Player.IsCompactOverlay && _playerWindow.GetWindow().Presenter.Kind != AppWindowPresenterKind.CompactOverlay)
        {
            _playerWindow.GetWindow().SetPresenter(AppWindowPresenterKind.CompactOverlay);
        }
        else if (!Player.IsCompactOverlay && _playerWindow.GetWindow().Presenter.Kind == AppWindowPresenterKind.CompactOverlay)
        {
            _playerWindow.GetWindow().SetPresenter(AppWindowPresenterKind.Default);
        }
    }
}
