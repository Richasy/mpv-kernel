// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using MpvKernel.WinUI;
using Richasy.MpvKernel;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Core.Models;
using System.Diagnostics;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WinUISample;

/// <summary>
/// Main window.
/// </summary>
public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
{
    /// <summary>
    /// Initializes a new instance of the MainWindow class. It sets up the user interface components.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        MpvNative.Initialize(@"C:\Users\zrich\Desktop\libmpv-2.dll");
    }

    private async void OnOpenButtonClick(object sender, RoutedEventArgs e)
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var filePicker = new FileOpenPicker();
        InitializeWithWindow.Initialize(filePicker, hwnd);
        filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        filePicker.FileTypeFilter.Add(".mp4");
        try
        {
            var file = await filePicker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            var client = await MpvClient.CreateAsync();
            await client.SetLogLevelAsync(MpvLogLevel.Info);
            await client.UseIdleAsync(true);
            var playerWindow = new MpvPlayerWindow(client, DispatcherQueue);
            var overlay = new PlayerOverlay(client,
                isFullScreen =>
                {
                    if (isFullScreen)
                    {
                        playerWindow.GetWindow().SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
                    }
                    else
                    {
                        playerWindow.GetWindow().SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);
                    }
                },
                isCompactOverlay =>
                {
                    if (isCompactOverlay)
                    {
                        playerWindow.GetWindow().SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.CompactOverlay);
                    }
                    else
                    {
                        playerWindow.GetWindow().SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);
                    }
                });
            playerWindow.SetUIElement(overlay);
            playerWindow.GetWindow().Title = file.Name;
            playerWindow.GetWindow().TitleBar.ExtendsContentIntoTitleBar = true;
            playerWindow.GetWindow().TitleBar.ButtonBackgroundColor = Colors.Transparent;
            playerWindow.GetWindow().TitleBar.PreferredTheme = Microsoft.UI.Windowing.TitleBarTheme.UseDefaultAppMode;
            playerWindow.Show();
            var options = new MpvPlayOptions
            {
                WindowHandle = playerWindow.Handle,
                StartPosition = 10,
                InitialSpeed = 2d,
            };
            await client.PlayAsync(file.Path, options);
            await client.SetVideoOutput(Richasy.MpvKernel.Core.Enums.VideoOutputType.GpuNext);
            await client.SetGpuApiAsync(Richasy.MpvKernel.Core.Enums.GpuApiType.D3D11);
            await client.SetGpuContextAsync(Richasy.MpvKernel.Core.Enums.GpuContextType.D3D11);

        }
        catch (Exception)
        {
            // Handle the exception if needed
            Debug.WriteLine("Error picking file.");
        }
    }
}
