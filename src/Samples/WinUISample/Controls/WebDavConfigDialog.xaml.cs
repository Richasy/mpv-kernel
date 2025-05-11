// Copyright (c) Richasy. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Richasy.WebDavKernel;
using Richasy.WinUIKernel.Share.Base;
using WinUISample.ViewModels;

namespace WinUISample.Controls;

/// <summary>
/// WebDav ≈‰÷√∂‘ª∞øÚ.
/// </summary>
public sealed partial class WebDavConfigDialog : AppDialog
{
    private readonly WebDavConfig? _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDavConfigDialog"/> class.
    /// </summary>
    public WebDavConfigDialog()
    {
        InitializeComponent();
        Title = "¥¥Ω® WebDav ≈‰÷√";
        PortBox.Value = 80;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDavConfigDialog"/> class.
    /// </summary>
    public WebDavConfigDialog(WebDavConfig config)
    {
        InitializeComponent();
        _source = config;
        Title = "–ﬁ∏ƒ WebDav ≈‰÷√";
        HostBox.Text = config.Host;
        PortBox.Value = config.Port ?? 80;
        UserNameBox.Text = config.UserName;
        PasswordBox.Password = config.Password;
        PathBox.Text = config.Path;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var host = HostBox.Text;
        var port = PortBox.Value;
        var userName = UserNameBox.Text;
        var password = PasswordBox.Password;
        var path = PathBox.Text;

        if (string.IsNullOrEmpty(host)
            || (!string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password)))
        {
            return;
        }

        if (!host.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            host = $"http://{host}";
            if (port == 0)
            {
                port = 80;
            }
        }

        var config = new WebDavConfig
        {
            Id = _source?.Id ?? Guid.NewGuid().ToString(),
            Name = "Test",
            Host = host,
            Path = path,
            Port = Convert.ToInt32(port),
            UserName = userName,
            Password = password,
        };

        if (_source is null)
        {
            this.Get<WebDavVideoPageViewModel>().AddConfigCommand.Execute(config);
        }
        else
        {
            this.Get<WebDavVideoPageViewModel>().UpdateConfigCommand.Execute(config);
        }
    }

    private void OnHostBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (HostBox.Text.StartsWith("https", StringComparison.OrdinalIgnoreCase) && (PortBox.Value == 0 || PortBox.Value == 80))
        {
            PortBox.Value = 443;
        }
        else if (!HostBox.Text.StartsWith("https", StringComparison.OrdinalIgnoreCase) && HostBox.Text.StartsWith("http", StringComparison.OrdinalIgnoreCase) && (PortBox.Value == 0 || PortBox.Value == 443))
        {
            PortBox.Value = 80;
        }
    }
}
