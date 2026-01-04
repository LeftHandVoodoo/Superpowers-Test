using BarrowWeather.Core.Models;

namespace BarrowWeather.Core.Services;

public interface IWeatherService
{
    Task<CurrentConditions?> GetCurrentConditionsAsync();
    Task<IReadOnlyList<HourlyForecast>> GetHourlyForecastAsync();
    Task<IReadOnlyList<DailyForecast>> GetDailyForecastAsync();
    Task<IReadOnlyList<WeatherAlert>> GetAlertsAsync();
}
