// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Richasy.MpvKernel;
using Richasy.MpvKernel.Core;
using Richasy.MpvKernel.Win32;
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
            var wnd = new MpvPlayerWindow(client);
            var filePath = file.Path;
            await wnd.InitializeAsync(filePath);
            wnd.SetSize(1280, 720);
        }
        catch (Exception)
        {
            // Handle the exception if needed
            Debug.WriteLine("Error picking file.");
        }
    }
}
