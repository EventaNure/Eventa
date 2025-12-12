using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient;

namespace Eventa.Services;

public class GoogleAuthService
{
    private const string GoogleDiscoveryUrl = "https://accounts.google.com/";
    
    private const string DesktopClientId = "171553413720-hiohepqf2enmrmqvhac66sjtihljb88o.apps.googleusercontent.com";
    private const string DesktopClientSecret = "valuehere";
    private const string DesktopRedirectUrl = "http://127.0.0.1:7890/";
    
    private const string AndroidClientId = "171553413720-85876fh5n4f2hbqlojhcbo4jov878o7g.apps.googleusercontent.com";
    private const string AndroidClientSecret = "valuehere";
    private const string AndroidRedirectUrl = "com.eventa.app://oauth2callback";
    
    private const string BrowserClientId = "171553413720-9serj0ak8bcie7mjt69sq2jsjgs98204.apps.googleusercontent.com";
    private const string BrowserClientSecret = "valuehere";
    private const string BrowserRedirectUrl = "https://localhost:5001/oauth2callback";

    public async Task<string?> AuthenticateAsync()
    {
        try
        {
            if (OperatingSystem.IsAndroid())
            {
                return await AuthenticateAndroidAsync();
            }
            else if (OperatingSystem.IsBrowser())
            {
                return await AuthenticateBrowserAsync();
            }
            else
            {
                return await AuthenticateDesktopAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Google Authentication Exception: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> AuthenticateDesktopAsync()
    {
        var options = new OidcClientOptions
        {
            Authority = GoogleDiscoveryUrl,
            ClientId = DesktopClientId,
            RedirectUri = DesktopRedirectUrl,
            ClientSecret = DesktopClientSecret,
            Scope = "openid profile email",
            LoadProfile = true,
        };

        options.Policy.Discovery.ValidateEndpoints = false;

        var httpListener = new HttpListener();
        httpListener.Prefixes.Add(DesktopRedirectUrl);
        httpListener.Start();

        try
        {
            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            OpenBrowser(state.StartUrl);

            var context = await httpListener.GetContextAsync();
            await RespondToCallback(context);

            var result = await client.ProcessResponseAsync(context.Request.RawUrl ?? string.Empty, state);

            if (result.IsError)
            {
                Debug.WriteLine($"Google Auth Error: {result.Error}");
                return null;
            }

            return result.IdentityToken;
        }
        finally
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }

    private async Task<string?> AuthenticateAndroidAsync()
    {
        try
        {
            var options = new OidcClientOptions
            {
                Authority = GoogleDiscoveryUrl,
                ClientId = AndroidClientId,
                RedirectUri = AndroidRedirectUrl,
                ClientSecret = AndroidClientSecret,
                Scope = "openid profile email",
                LoadProfile = true,
            };

            options.Policy.Discovery.ValidateEndpoints = false;

            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            // Open browser with OAuth URL
            // On Android, this opens the system browser or Chrome Custom Tabs
            OpenBrowserAndroid(state.StartUrl);

            Debug.WriteLine("Android OAuth initiated. Waiting for callback from MainActivity...");
            
            // Get task that completes when OAuth callback is received
            var oauthTask = PlatformOAuthService.Instance.GetOAuthCallbackTask();
            
            // Wait for callback with 5-minute timeout
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5));
            var completedTask = await Task.WhenAny(oauthTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.WriteLine("Android OAuth timeout - user did not complete authentication");
                return null;
            }

            var idToken = await oauthTask;
            PlatformOAuthService.Instance.Clear();
            
            return idToken;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Android Google Authentication Exception: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> AuthenticateBrowserAsync()
    {
        try
        {
            var options = new OidcClientOptions
            {
                Authority = GoogleDiscoveryUrl,
                ClientId = BrowserClientId,
                RedirectUri = BrowserRedirectUrl,
                ClientSecret = BrowserClientSecret,
                Scope = "openid profile email",
                LoadProfile = true,
            };

            options.Policy.Discovery.ValidateEndpoints = false;

            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            var oauthTask = PlatformOAuthService.Instance.GetOAuthCallbackTask();
            
            OpenBrowserWeb(state.StartUrl);

            Debug.WriteLine("Browser OAuth initiated. Waiting for server callback...");
            
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(10));
            var completedTask = await Task.WhenAny(oauthTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.WriteLine("Browser OAuth timeout - user did not complete authentication");
                return null;
            }

            var idToken = await oauthTask;
            PlatformOAuthService.Instance.Clear();
            
            return idToken;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Browser Google Authentication Exception: {ex.Message}");
            return null;
        }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open browser: {ex.Message}");
        }
    }

    private static void OpenBrowserAndroid(string url)
    {
        try
        {
            Debug.WriteLine($"Opening Android browser with OAuth URL");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open Android browser: {ex.Message}");
        }
    }

    private static void OpenBrowserWeb(string url)
    {
        try
        {
            Debug.WriteLine($"Opening browser OAuth URL");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open web browser: {ex.Message}");
        }
    }

    private static async Task RespondToCallback(HttpListenerContext context)
    {
        try
        {
            var response = context.Response;
            var html = """
                <html>
                <head>
                    <title>Authentication Successful</title>
                    <style>
                        body {
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            margin: 0;
                        }
                        .container {
                            text-align: center;
                            background: white;
                            padding: 40px;
                            border-radius: 10px;
                            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
                        }
                        h1 { color: #333; margin: 0 0 10px 0; }
                        p { color: #666; }
                    </style>
                </head>
                <body>
                    <div class="container">
                        <h1>Authentication Successful</h1>
                        <p>You can now return to the application.</p>
                    </div>
                </body>
                </html>
                """;

            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";

            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error responding to callback: {ex.Message}");
        }
    }
}