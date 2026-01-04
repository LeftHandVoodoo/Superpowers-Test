namespace BarrowWeather.Core.Models;

public record CurrentConditions(
    DateTime Timestamp,
    double TemperatureF,
    double WindSpeedMph,
    string WindDirection,
    int HumidityPercent,
    double FeelsLikeF,
    string Description,
    string IconUrl
);
