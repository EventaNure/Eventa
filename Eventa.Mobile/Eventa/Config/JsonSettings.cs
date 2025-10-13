using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.Config;

public class JsonSettings<T> where T : class, new()
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string FilePath { get; }

    public JsonSettings(string fileName = "settings.json")
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Eventa");
        Directory.CreateDirectory(folder);
        FilePath = Path.Combine(folder, fileName);
    }

    public async Task<T> LoadAsync()
    {
        if (!File.Exists(FilePath))
            return new T();

        await using var stream = File.OpenRead(FilePath);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, _options);
        return result ?? new T();
    }

    public async Task SaveAsync(T settings)
    {
        await using var stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, settings, _options);
    }
}