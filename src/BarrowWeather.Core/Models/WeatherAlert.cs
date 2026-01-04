namespace BarrowWeather.Core.Models;

public record WeatherAlert(
    string Severity,
    string Event,
    string Headline,
    DateTime Expires
);
