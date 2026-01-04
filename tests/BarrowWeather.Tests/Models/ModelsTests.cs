using BarrowWeather.Core.Models;
using FluentAssertions;

namespace BarrowWeather.Tests.Models;

public class ModelsTests
{
    [Fact]
    public void CurrentConditions_ShouldStoreAllProperties()
    {
        var timestamp = DateTime.UtcNow;
        var conditions = new CurrentConditions(
            Timestamp: timestamp,
            TemperatureF: -15.5,
            WindSpeedMph: 25.0,
            WindDirection: "NW",
            HumidityPercent: 78,
            FeelsLikeF: -32.0,
            Description: "Snow",
            IconUrl: "https://api.weather.gov/icons/land/night/snow"
        );

        conditions.TemperatureF.Should().Be(-15.5);
        conditions.WindDirection.Should().Be("NW");
        conditions.Description.Should().Be("Snow");
    }

    [Fact]
    public void HourlyForecast_ShouldStoreAllProperties()
    {
        var time = DateTime.UtcNow;
        var forecast = new HourlyForecast(
            Time: time,
            TemperatureF: -17.0,
            ShortDescription: "Mostly Cloudy",
            PrecipitationChance: 20
        );

        forecast.TemperatureF.Should().Be(-17.0);
        forecast.PrecipitationChance.Should().Be(20);
    }

    [Fact]
    public void DailyForecast_ShouldStoreAllProperties()
    {
        var forecast = new DailyForecast(
            DayName: "Monday",
            HighF: -12.0,
            LowF: -22.0,
            Description: "Snow likely",
            IconUrl: "https://api.weather.gov/icons/land/day/snow"
        );

        forecast.DayName.Should().Be("Monday");
        forecast.HighF.Should().Be(-12.0);
        forecast.LowF.Should().Be(-22.0);
    }

    [Fact]
    public void WeatherAlert_ShouldStoreAllProperties()
    {
        var expires = DateTime.UtcNow.AddHours(12);
        var alert = new WeatherAlert(
            Severity: "Warning",
            Event: "Blizzard Warning",
            Headline: "Blizzard Warning in effect until Tuesday morning",
            Expires: expires
        );

        alert.Severity.Should().Be("Warning");
        alert.Event.Should().Be("Blizzard Warning");
    }

    [Fact]
    public void SunData_ShouldStoreAllProperties()
    {
        var sunrise = new TimeOnly(10, 45);
        var sunset = new TimeOnly(15, 12);
        var sunData = new SunData(Sunrise: sunrise, Sunset: sunset);

        sunData.Sunrise.Should().Be(sunrise);
        sunData.Sunset.Should().Be(sunset);
    }
}
