using Duende.IdentityModel.OidcClient;
using Eventa.Config;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Eventa.Services;

public class GoogleAuthService
{
    private const string GoogleDiscoveryUrl = "https://accounts.google.com/";

    private const string DesktopClientId = "171553413720-hiohepqf2enmrmqvhac66sjtihljb88o.apps.googleusercontent.com";
    private const string DesktopClientSecret = "valuehere";
    private const string DesktopRedirectUrl = "http://127.0.0.1:7890/";

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
        // TODO: Implement Android authentication
        await Task.CompletedTask;
        return null;
    }

    private async Task<string?> AuthenticateBrowserAsync()
    {
        // For browser, we navigate to the google-auth.html page
        // The page handles authentication and stores JWT in cookies
        // Then redirects back to index.html
        try
        {
            // Use JS interop to navigate to the Google auth page
            GoogleAuthHelper.NavigateToGoogleAuth();

            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Browser auth error: {ex.Message}");
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