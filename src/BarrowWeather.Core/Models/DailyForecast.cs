namespace BarrowWeather.Core.Models;

public record DailyForecast(
    string DayName,
    double HighF,
    double LowF,
    string Description,
    string IconUrl
);
