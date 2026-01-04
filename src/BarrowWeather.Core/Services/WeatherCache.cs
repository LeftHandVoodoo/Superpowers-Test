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
        await File.WriteAllTextAsync(_cacheFile, json);
    }

    public async Task<WeatherCacheData?> LoadAsync()
    {
        if (!File.Exists(_cacheFile)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(_cacheFile);
            return JsonSerializer.Deserialize<WeatherCacheData>(json);
        }
        catch
        {
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
