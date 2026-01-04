using System.Text.Json;
using BarrowWeather.Core.Models;

namespace BarrowWeather.Core.Services;

public class WeatherCache
{
    private readonly string _cacheDir;
    private readonly string _cacheFile;

    public WeatherCache()
    {
        _cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BarrowWeather");
        _cacheFile = Path.Combine(_cacheDir, "weather-cache.json");

        Directory.CreateDirectory(_cacheDir);
    }

    public async Task SaveAsync(WeatherCacheData data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        // Atomic write: write to temp file, then rename
        var tempFile = _cacheFile + ".tmp";
        await File.WriteAllTextAsync(tempFile, json);
        File.Move(tempFile, _cacheFile, overwrite: true);
    }

    public async Task<WeatherCacheData?> LoadAsync()
    {
        if (!File.Exists(_cacheFile)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(_cacheFile);
            return JsonSerializer.Deserialize<WeatherCacheData>(json);
        }
        catch (JsonException)
        {
            // Corrupted cache file - return null to trigger fresh fetch
            return null;
        }
        catch (IOException)
        {
            // File access issue - return null to trigger fresh fetch
            return null;
        }
    }
}

public record WeatherCacheData(
    DateTime CachedAt,
    CurrentConditions? CurrentConditions,
    List<HourlyForecast> HourlyForecasts,
    List<DailyForecast> DailyForecasts,
    List<WeatherAlert> Alerts,
    SunData? SunData
);
