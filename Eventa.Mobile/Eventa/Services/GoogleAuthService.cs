using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient;

namespace Eventa.Services;

/// <summary>
/// Handles Google OAuth authentication flow using OIDC.
/// Supports Desktop (HttpListener), Android (Intent), and Browser platforms.
/// Each platform uses its own Google OAuth credentials.
/// </summary>
public class GoogleAuthService
{
    private const string GoogleDiscoveryUrl = "https://accounts.google.com/";
    
    // Desktop credentials
    private const string DesktopClientId = "171553413720-hiohepqf2enmrmqvhac66sjtihljb88o.apps.googleusercontent.com";
    private const string DesktopClientSecret = "valuehere";
    private const string DesktopRedirectUrl = "http://127.0.0.1:7890/";
    
    private const string AndroidClientId = "171553413720-85876fh5n4f2hbqlojhcbo4jov878o7g.apps.googleusercontent.com";
    private const string AndroidClientSecret = "valuehere";
    private const string AndroidRedirectUrl = "com.eventa.app://oauth2callback";
    
    private const string BrowserClientId = "171553413720-9serj0ak8bcie7mjt69sq2jsjgs98204.apps.googleusercontent.com";
    private const string BrowserClientSecret = "valuehere";
    private const string BrowserRedirectUrl = "https://localhost:5001/oauth2callback";

    /// <summary>
    /// Opens Google Sign-In flow and returns the ID token.
    /// Automatically detects the platform and uses the appropriate method.
    /// </summary>
    public async Task<string?> AuthenticateAsync()
    {
        try
        {
            // Detect platform and use appropriate method
            if (IsAndroid())
            {
                return await AuthenticateAndroidAsync();
            }
            else if (IsBrowser())
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

    /// <summary>
    /// Desktop authentication using HttpListener.
    /// Works on Windows, macOS, and Linux desktop applications.
    /// </summary>
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

        // Disable endpoint validation for Google
        options.Policy.Discovery.ValidateEndpoints = false;

        var httpListener = new HttpListener();
        httpListener.Prefixes.Add(DesktopRedirectUrl);
        httpListener.Start();

        try
        {
            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            // Open browser with Google login URL
            OpenBrowser(state.StartUrl);

            // Wait for callback
            var context = await httpListener.GetContextAsync();
            await RespondToCallback(context);

            // Process the response
            var result = await client.ProcessResponseAsync(context.Request.RawUrl ?? string.Empty, state);

            if (result.IsError)
            {
                Debug.WriteLine($"Google Auth Error: {result.Error}");
                return null;
            }

            // Return the ID token
            return result.IdentityToken;
        }
        finally
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }

    /// <summary>
    /// Android authentication using Custom Tabs or system browser.
    /// Opens the browser with the OAuth URL and waits for callback via intent.
    /// </summary>
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

    /// <summary>
    /// Browser authentication using WebAssembly and browser APIs.
    /// Navigates to OAuth endpoint and waits for server-side redirect callback.
    /// </summary>
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

            // Get task that completes when OAuth callback is received from server
            var oauthTask = PlatformOAuthService.Instance.GetOAuthCallbackTask();
            
            // Navigate to OAuth endpoint in browser
            // This is done via window.location in JavaScript (requires JS interop)
            OpenBrowserWeb(state.StartUrl);

            Debug.WriteLine("Browser OAuth initiated. Waiting for server callback...");
            
            // Wait for callback with 10-minute timeout (user might take time to authenticate)
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

    /// <summary>
    /// Extracts user information from the ID token (without validation).
    /// Note: In production, always validate the token signature server-side.
    /// </summary>
    public static GoogleUserInfo? ExtractUserInfoFromToken(string idToken)
    {
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
                return null;

            // Decode the payload (second part)
            var payload = parts[1];
            // Add padding if necessary
            var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
            var bytes = Convert.FromBase64String(padded);
            var json = Encoding.UTF8.GetString(bytes);

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new GoogleUserInfo
            {
                Sub = root.TryGetProperty("sub", out var sub) ? sub.GetString() : null,
                Email = root.TryGetProperty("email", out var email) ? email.GetString() : null,
                Name = root.TryGetProperty("name", out var name) ? name.GetString() : null,
                Picture = root.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
                EmailVerified = root.TryGetProperty("email_verified", out var emailVerified) && emailVerified.GetBoolean()
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Token extraction error: {ex.Message}");
            return null;
        }
    }

    private static bool IsAndroid()
    {
        // Check if running on Android platform
        try
        {
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Create("ANDROID"));
        }
        catch
        {
            return false;
        }
    }

    private static bool IsBrowser()
    {
        // Check if running in browser (WASM)
        try
        {
            // WASM runs on Mono runtime
            var frameworkDesc = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            return frameworkDesc.Contains("Mono") || frameworkDesc.Contains("Wasm");
        }
        catch
        {
            return false;
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
            // On Android, this method should open the system browser
            // For now, this is a placeholder - actual implementation would use Android intents
            // The MainActivity will need to implement browser launching via Android APIs
            Debug.WriteLine($"Opening Android browser with OAuth URL");
            
            // TODO: Implement Android-specific browser launching in MainActivity
            // This would typically use:
            // - Intent with ACTION_VIEW
            // - Chrome Custom Tabs for better UX
            // - Or system browser fallback
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
            // In browser (WASM), navigate to OAuth URL
            // This requires JavaScript interop to set window.location.href
            Debug.WriteLine($"Opening browser OAuth URL");
            
            // TODO: Implement JavaScript interop for browser navigation
            // Example implementation:
            // IJSRuntime.InvokeVoidAsync("window.location.href", url);
            
            // For now, the implementation should be handled by Blazor component
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

/// <summary>
/// Represents extracted user information from Google ID token
/// </summary>
public class GoogleUserInfo
{
    public string? Sub { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public bool EmailVerified { get; set; }
}