using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Eventa.Services;

/// <summary>
/// Platform-specific OAuth callback handler.
/// This service manages OAuth callbacks for different platforms (Desktop, Android, Browser).
/// </summary>
public class PlatformOAuthService
{
    private static PlatformOAuthService? _instance;
    private TaskCompletionSource<string?>? _oauthCompletionSource;
    private Dictionary<string, string> _callbackParams = new();

    public static PlatformOAuthService Instance => _instance ??= new PlatformOAuthService();

    /// <summary>
    /// Gets a task that completes when the OAuth callback is received.
    /// Used for Android and Browser platforms.
    /// </summary>
    public Task<string?> GetOAuthCallbackTask()
    {
        _oauthCompletionSource = new TaskCompletionSource<string?>();
        return _oauthCompletionSource.Task;
    }

    /// <summary>
    /// Handles the OAuth callback from the platform (Android intent or Browser redirect).
    /// This should be called from the Android MainActivity or Browser redirect handler.
    /// </summary>
    public void HandleOAuthCallback(Dictionary<string, string> parameters)
    {
        _callbackParams = parameters;
        _oauthCompletionSource?.TrySetResult(parameters.ContainsKey("id_token") ? parameters["id_token"] : null);
    }

    /// <summary>
    /// Handles OAuth callback error.
    /// </summary>
    public void HandleOAuthCallbackError(string errorMessage)
    {
        _oauthCompletionSource?.TrySetException(new Exception($"OAuth callback error: {errorMessage}"));
    }

    /// <summary>
    /// Clears the current OAuth state.
    /// </summary>
    public void Clear()
    {
        _oauthCompletionSource = null;
        _callbackParams.Clear();
    }

    /// <summary>
    /// Gets callback parameters for inspection.
    /// </summary>
    public Dictionary<string, string> GetCallbackParams() => new Dictionary<string, string>(_callbackParams);
}
