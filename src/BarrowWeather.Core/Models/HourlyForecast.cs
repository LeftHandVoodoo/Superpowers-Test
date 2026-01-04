namespace BarrowWeather.Core.Models;

public record HourlyForecast(
    DateTime Time,
    double TemperatureF,
    string ShortDescription,
    int PrecipitationChance
);
