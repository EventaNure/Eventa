using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Eventa.Config;

public class JsonSettings<T> where T : class, new()
{
    private static readonly JsonSerializerOptions _fileOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _cookieOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public string FilePath { get; }
    private readonly string _cookieName;
    private readonly bool _isBrowser;

    public JsonSettings(string fileName = "settings.json")
    {
        // Detect if running in browser (WASM)
        _isBrowser = OperatingSystem.IsBrowser();
        _cookieName = fileName.Replace(".json", "").Replace(".", "_");

        if (!_isBrowser)
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Eventa");
            Directory.CreateDirectory(folder);
            FilePath = Path.Combine(folder, fileName);
        }
        else
        {
            FilePath = string.Empty;
        }
    }

    public async Task<T> LoadAsync()
    {
        if (_isBrowser)
        {
            return LoadFromCookie();
        }
        else
        {
            return await LoadFromFileAsync();
        }
    }

    public async Task SaveAsync(T settings)
    {
        if (_isBrowser)
        {
            SaveToCookie(settings);
        }
        else
        {
            await SaveToFileAsync(settings);
        }
    }

    private async Task<T> LoadFromFileAsync()
    {
        if (!File.Exists(FilePath))
            return new T();

        await using var stream = File.OpenRead(FilePath);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, _fileOptions);
        return result ?? new T();
    }

    private async Task SaveToFileAsync(T settings)
    {
        await using var stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, settings, _fileOptions);
    }

    private T LoadFromCookie()
    {
        try
        {
            var cookieValue = CookieHelper.GetCookie(_cookieName);

            if (string.IsNullOrEmpty(cookieValue))
                return new T();

            return JsonSerializer.Deserialize<T>(cookieValue, _cookieOptions) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    private void SaveToCookie(T settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _cookieOptions);
            CookieHelper.SetCookie(_cookieName, json, 365);

            // Verify the cookie was set correctly
            var verification = CookieHelper.GetCookie(_cookieName);
            if (string.IsNullOrEmpty(verification))
            {
                // Retry once if failed
                Thread.Sleep(50);
                CookieHelper.SetCookie(_cookieName, json, 365);
            }
        }
        catch
        {
        }
    }
}
