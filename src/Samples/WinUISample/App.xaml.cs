// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.Storage;
using WinUISample.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISample;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string Id = "Richasy.MpvWinUISample";

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    /// <summary>
    /// 日志文件夹.
    /// </summary>
    public static string LoggerFolder { get; private set; }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var instance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey(Id);
        if (instance.IsCurrent)
        {
            var rootFolder = ApplicationData.Current.LocalFolder;
            var fullPath = $"{rootFolder.Path}\\Logger";
            LoggerFolder = fullPath;
            if (!Directory.Exists(fullPath))
            {
                _ = Directory.CreateDirectory(fullPath);
            }

            instance.Activated += OnInstanceActivated;
            GlobalDependencies.Initialize();
            var mainWindow = new MainWindow();
            mainWindow.Activate();
        }
        else
        {
            var activatedArgs = instance.GetActivatedEventArgs();
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
#pragma warning disable VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
            _ = instance.RedirectActivationToAsync(activatedArgs).AsTask().ContinueWith(_ => Current.Exit());
#pragma warning restore VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
        }
    }

    private void OnInstanceActivated(object? sender, AppActivationArguments e)
    {
        this.Get<DispatcherQueue>().TryEnqueue(() => this.Get<AppViewModel>().MainWindow.Activate());
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        GlobalDependencies.Kernel.GetRequiredService<ILogger<App>>().LogError(e.Exception, "Unhandled exception occurred.");
        e.Handled = true;
    }
}
