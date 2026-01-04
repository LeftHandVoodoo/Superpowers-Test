using System.Net.Http.Json;
using BarrowWeather.Core.Models;
using BarrowWeather.Core.Services.Dto;

namespace BarrowWeather.Core.Services;

public class NwsWeatherService : IWeatherService
{
    private const double BarrowLatitude = 71.2906;
    private const double BarrowLongitude = -156.7886;
    private const string BaseUrl = "https://api.weather.gov";

    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private NwsPointsProperties? _cachedPointsData;
    private string? _cachedStationId;

    public NwsWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "(BarrowWeather, contact@example.com)");
    }

    public async Task<CurrentConditions?> GetCurrentConditionsAsync()
    {
        await EnsurePointsDataAsync();
        await EnsureStationIdAsync();

        if (_cachedStationId == null) return null;

        var url = $"{BaseUrl}/stations/{_cachedStationId}/observations/latest";
        var response = await _httpClient.GetFromJsonAsync<NwsObservationResponse>(url);
        var props = response?.Properties;

        if (props == null) return null;

        return new CurrentConditions(
            Timestamp: props.Timestamp ?? DateTime.UtcNow,
            TemperatureF: CelsiusToFahrenheit(props.Temperature?.Value),
            WindSpeedMph: KmhToMph(props.WindSpeed?.Value),
            WindDirection: DegreesToCardinal(props.WindDirection?.Value),
            HumidityPercent: (int)(props.RelativeHumidity?.Value ?? 0),
            FeelsLikeF: CelsiusToFahrenheit(props.WindChill?.Value ?? props.Temperature?.Value),
            Description: props.TextDescription ?? "Unknown",
            IconUrl: props.Icon ?? ""
        );
    }

    public async Task<IReadOnlyList<HourlyForecast>> GetHourlyForecastAsync()
    {
        await EnsurePointsDataAsync();

        if (_cachedPointsData?.ForecastHourlyUrl == null)
            return Array.Empty<HourlyForecast>();

        var response = await _httpClient.GetFromJsonAsync<NwsForecastResponse>(_cachedPointsData.ForecastHourlyUrl);
        var periods = response?.Properties?.Periods;

        if (periods == null) return Array.Empty<HourlyForecast>();

        return periods.Take(24).Select(p => new HourlyForecast(
            Time: p.StartTime,
            TemperatureF: p.Temperature,
            ShortDescription: p.ShortForecast ?? "",
            PrecipitationChance: (int)(p.ProbabilityOfPrecipitation?.Value ?? 0)
        )).ToList();
    }

    public async Task<IReadOnlyList<DailyForecast>> GetDailyForecastAsync()
    {
        await EnsurePointsDataAsync();

        if (_cachedPointsData?.ForecastUrl == null)
            return Array.Empty<DailyForecast>();

        var response = await _httpClient.GetFromJsonAsync<NwsForecastResponse>(_cachedPointsData.ForecastUrl);
        var periods = response?.Properties?.Periods;

        if (periods == null) return Array.Empty<DailyForecast>();

        // NWS returns day/night pairs, combine them
        var dailyForecasts = new List<DailyForecast>();
        for (int i = 0; i < periods.Count - 1; i += 2)
        {
            var dayPeriod = periods[i].IsDaytime ? periods[i] : periods[i + 1];
            var nightPeriod = periods[i].IsDaytime ? periods[i + 1] : periods[i];

            dailyForecasts.Add(new DailyForecast(
                DayName: dayPeriod.Name ?? "",
                HighF: dayPeriod.Temperature,
                LowF: nightPeriod.Temperature,
                Description: dayPeriod.ShortForecast ?? "",
                IconUrl: dayPeriod.Icon ?? ""
            ));
        }

        return dailyForecasts.Take(7).ToList();
    }

    public async Task<IReadOnlyList<WeatherAlert>> GetAlertsAsync()
    {
        var url = $"{BaseUrl}/alerts/active?point={BarrowLatitude},{BarrowLongitude}";
        var response = await _httpClient.GetFromJsonAsync<NwsAlertsResponse>(url);

        if (response?.Features == null) return Array.Empty<WeatherAlert>();

        return response.Features
            .Where(f => f.Properties != null)
            .Select(f => new WeatherAlert(
                Severity: f.Properties!.Severity ?? "Unknown",
                Event: f.Properties.Event ?? "Unknown",
                Headline: f.Properties.Headline ?? "",
                Expires: f.Properties.Expires ?? DateTime.UtcNow
            ))
            .ToList();
    }

    private async Task EnsurePointsDataAsync()
    {
        if (_cachedPointsData != null) return;

        await _initLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedPointsData != null) return;

            var url = $"{BaseUrl}/points/{BarrowLatitude},{BarrowLongitude}";
            var response = await _httpClient.GetFromJsonAsync<NwsPointsResponse>(url);
            _cachedPointsData = response?.Properties;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task EnsureStationIdAsync()
    {
        // Ensure points data first (has its own lock)
        await EnsurePointsDataAsync();

        if (_cachedStationId != null) return;

        await _initLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedStationId != null) return;
            if (_cachedPointsData?.ObservationStationsUrl == null) return;

            var response = await _httpClient.GetFromJsonAsync<NwsStationsResponse>(_cachedPointsData.ObservationStationsUrl);
            _cachedStationId = response?.Features?.FirstOrDefault()?.Properties?.StationIdentifier;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static double CelsiusToFahrenheit(double? celsius)
    {
        if (celsius == null) return 0;
        return Math.Round(celsius.Value * 9 / 5 + 32, 1);
    }

    private static double KmhToMph(double? kmh)
    {
        if (kmh == null) return 0;
        return Math.Round(kmh.Value * 0.621371, 1);
    }

    private static string DegreesToCardinal(double? degrees)
    {
        if (degrees == null) return "N/A";
        string[] cardinals = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        int index = (int)Math.Round(degrees.Value / 22.5) % 16;
        return cardinals[index];
    }
}
