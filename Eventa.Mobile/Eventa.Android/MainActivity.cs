using Android.App;
using Android.Content;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Eventa.Services;
using System.Collections.Generic;

namespace Eventa.Android;

[Activity(
    Label = "Eventa",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if (intent?.Data != null)
        {
            var uri = intent.Data;
            if (uri.Scheme == "com.eventa.app" && uri.Host == "oauth2callback")
            {
                var parameters = new Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var queryParams = uri.Query.TrimStart('?').Split('&');
                    foreach (var param in queryParams)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            parameters[System.Uri.UnescapeDataString(parts[0])] = 
                                System.Uri.UnescapeDataString(parts[1]);
                        }
                    }
                }

                if (parameters.ContainsKey("error"))
                {
                    PlatformOAuthService.Instance.HandleOAuthCallbackError(
                        parameters.ContainsKey("error_description") 
                            ? parameters["error_description"] 
                            : parameters["error"]
                    );
                }
                else
                {
                    PlatformOAuthService.Instance.HandleOAuthCallback(parameters);
                }
            }
        }
    }
}