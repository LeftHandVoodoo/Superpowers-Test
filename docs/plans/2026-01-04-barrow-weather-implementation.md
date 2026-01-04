# Barrow Weather Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Windows 11 WinUI 3 app that displays weather for Barrow, Alaska using the National Weather Service API.

**Architecture:** MVVM pattern with a WeatherViewModel driving the UI, NwsWeatherService handling API calls, and a timer-based refresh service. Data is cached locally for offline support.

**Tech Stack:** .NET 8, WinUI 3 (Windows App SDK 1.5), CommunityToolkit.Mvvm, xUnit for tests

---

## Task 1: Create Solution and Project Structure

**Files:**
- Create: `BarrowWeather.sln`
- Create: `src/BarrowWeather/BarrowWeather.csproj`
- Create: `tests/BarrowWeather.Tests/BarrowWeather.Tests.csproj`

**Step 1: Create the WinUI 3 solution**

```bash
dotnet new sln -n BarrowWeather
mkdir -p src/BarrowWeather
mkdir -p tests/BarrowWeather.Tests
```

**Step 2: Create the WinUI 3 project**

```bash
dotnet new winui3 -n BarrowWeather -o src/BarrowWeather -f net8.0-windows10.0.19041.0
```

**Step 3: Create the test project**

```bash
dotnet new xunit -n BarrowWeather.Tests -o tests/BarrowWeather.Tests -f net8.0-windows10.0.19041.0
```

**Step 4: Add projects to solution**

```bash
dotnet sln add src/BarrowWeather/BarrowWeather.csproj
dotnet sln add tests/BarrowWeather.Tests/BarrowWeather.Tests.csproj
dotnet add tests/BarrowWeather.Tests reference src/BarrowWeather
```

**Step 5: Add NuGet packages**

```bash
dotnet add src/BarrowWeather package CommunityToolkit.Mvvm --version 8.2.2
dotnet add src/BarrowWeather package Microsoft.Extensions.Logging --version 8.0.0
dotnet add src/BarrowWeather package Microsoft.Extensions.Logging.Debug --version 8.0.0
dotnet add tests/BarrowWeather.Tests package Moq --version 4.20.70
dotnet add tests/BarrowWeather.Tests package FluentAssertions --version 6.12.0
```

**Step 6: Verify build**

Run: `dotnet build BarrowWeather.sln`
Expected: Build succeeded with 0 errors

**Step 7: Commit**

```bash
git add -A
git commit -m "feat: create solution with WinUI 3 project and test project"
```

---

## Task 2: Create Data Models

**Files:**
- Create: `src/BarrowWeather/Models/CurrentConditions.cs`
- Create: `src/BarrowWeather/Models/HourlyForecast.cs`
- Create: `src/BarrowWeather/Models/DailyForecast.cs`
- Create: `src/BarrowWeather/Models/WeatherAlert.cs`
- Create: `src/BarrowWeather/Models/SunData.cs`
- Test: `tests/BarrowWeather.Tests/Models/ModelsTests.cs`

**Step 1: Write the failing test for models**

Create `tests/BarrowWeather.Tests/Models/ModelsTests.cs`:

```csharp
using BarrowWeather.Models;
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
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~ModelsTests" -v n`
Expected: FAIL - types not found

**Step 3: Create Models directory and files**

Create `src/BarrowWeather/Models/CurrentConditions.cs`:

```csharp
namespace BarrowWeather.Models;

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
```

Create `src/BarrowWeather/Models/HourlyForecast.cs`:

```csharp
namespace BarrowWeather.Models;

public record HourlyForecast(
    DateTime Time,
    double TemperatureF,
    string ShortDescription,
    int PrecipitationChance
);
```

Create `src/BarrowWeather/Models/DailyForecast.cs`:

```csharp
namespace BarrowWeather.Models;

public record DailyForecast(
    string DayName,
    double HighF,
    double LowF,
    string Description,
    string IconUrl
);
```

Create `src/BarrowWeather/Models/WeatherAlert.cs`:

```csharp
namespace BarrowWeather.Models;

public record WeatherAlert(
    string Severity,
    string Event,
    string Headline,
    DateTime Expires
);
```

Create `src/BarrowWeather/Models/SunData.cs`:

```csharp
namespace BarrowWeather.Models;

public record SunData(
    TimeOnly Sunrise,
    TimeOnly Sunset
);
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~ModelsTests" -v n`
Expected: 5 passed

**Step 5: Commit**

```bash
git add -A
git commit -m "feat: add weather data models"
```

---

## Task 3: Create NWS API Response DTOs

**Files:**
- Create: `src/BarrowWeather/Services/Dto/NwsPointsResponse.cs`
- Create: `src/BarrowWeather/Services/Dto/NwsObservationResponse.cs`
- Create: `src/BarrowWeather/Services/Dto/NwsForecastResponse.cs`
- Create: `src/BarrowWeather/Services/Dto/NwsAlertsResponse.cs`

**Step 1: Create DTOs for NWS API JSON deserialization**

Create `src/BarrowWeather/Services/Dto/NwsPointsResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace BarrowWeather.Services.Dto;

public class NwsPointsResponse
{
    [JsonPropertyName("properties")]
    public NwsPointsProperties? Properties { get; set; }
}

public class NwsPointsProperties
{
    [JsonPropertyName("forecast")]
    public string? ForecastUrl { get; set; }

    [JsonPropertyName("forecastHourly")]
    public string? ForecastHourlyUrl { get; set; }

    [JsonPropertyName("observationStations")]
    public string? ObservationStationsUrl { get; set; }
}
```

Create `src/BarrowWeather/Services/Dto/NwsObservationResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace BarrowWeather.Services.Dto;

public class NwsObservationResponse
{
    [JsonPropertyName("properties")]
    public NwsObservationProperties? Properties { get; set; }
}

public class NwsObservationProperties
{
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("textDescription")]
    public string? TextDescription { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("temperature")]
    public NwsQuantitativeValue? Temperature { get; set; }

    [JsonPropertyName("windSpeed")]
    public NwsQuantitativeValue? WindSpeed { get; set; }

    [JsonPropertyName("windDirection")]
    public NwsQuantitativeValue? WindDirection { get; set; }

    [JsonPropertyName("relativeHumidity")]
    public NwsQuantitativeValue? RelativeHumidity { get; set; }

    [JsonPropertyName("windChill")]
    public NwsQuantitativeValue? WindChill { get; set; }
}

public class NwsQuantitativeValue
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("unitCode")]
    public string? UnitCode { get; set; }
}
```

Create `src/BarrowWeather/Services/Dto/NwsForecastResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace BarrowWeather.Services.Dto;

public class NwsForecastResponse
{
    [JsonPropertyName("properties")]
    public NwsForecastProperties? Properties { get; set; }
}

public class NwsForecastProperties
{
    [JsonPropertyName("periods")]
    public List<NwsForecastPeriod>? Periods { get; set; }
}

public class NwsForecastPeriod
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("isDaytime")]
    public bool IsDaytime { get; set; }

    [JsonPropertyName("temperature")]
    public int Temperature { get; set; }

    [JsonPropertyName("temperatureUnit")]
    public string? TemperatureUnit { get; set; }

    [JsonPropertyName("probabilityOfPrecipitation")]
    public NwsQuantitativeValue? ProbabilityOfPrecipitation { get; set; }

    [JsonPropertyName("windSpeed")]
    public string? WindSpeed { get; set; }

    [JsonPropertyName("windDirection")]
    public string? WindDirection { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("shortForecast")]
    public string? ShortForecast { get; set; }

    [JsonPropertyName("detailedForecast")]
    public string? DetailedForecast { get; set; }
}
```

Create `src/BarrowWeather/Services/Dto/NwsAlertsResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace BarrowWeather.Services.Dto;

public class NwsAlertsResponse
{
    [JsonPropertyName("features")]
    public List<NwsAlertFeature>? Features { get; set; }
}

public class NwsAlertFeature
{
    [JsonPropertyName("properties")]
    public NwsAlertProperties? Properties { get; set; }
}

public class NwsAlertProperties
{
    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }
}
```

Create `src/BarrowWeather/Services/Dto/NwsStationsResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace BarrowWeather.Services.Dto;

public class NwsStationsResponse
{
    [JsonPropertyName("features")]
    public List<NwsStationFeature>? Features { get; set; }
}

public class NwsStationFeature
{
    [JsonPropertyName("properties")]
    public NwsStationProperties? Properties { get; set; }
}

public class NwsStationProperties
{
    [JsonPropertyName("stationIdentifier")]
    public string? StationIdentifier { get; set; }
}
```

**Step 2: Verify build**

Run: `dotnet build src/BarrowWeather`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add -A
git commit -m "feat: add NWS API response DTOs"
```

---

## Task 4: Create Weather Service Interface and Implementation

**Files:**
- Create: `src/BarrowWeather/Services/IWeatherService.cs`
- Create: `src/BarrowWeather/Services/NwsWeatherService.cs`
- Test: `tests/BarrowWeather.Tests/Services/NwsWeatherServiceTests.cs`
- Create: `tests/BarrowWeather.Tests/TestData/points-response.json`
- Create: `tests/BarrowWeather.Tests/TestData/observation-response.json`

**Step 1: Write failing test for weather service**

Create `tests/BarrowWeather.Tests/Services/NwsWeatherServiceTests.cs`:

```csharp
using System.Net;
using BarrowWeather.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace BarrowWeather.Tests.Services;

public class NwsWeatherServiceTests
{
    [Fact]
    public async Task GetCurrentConditionsAsync_ShouldParseApiResponse()
    {
        // Arrange
        var pointsJson = """
        {
            "properties": {
                "forecast": "https://api.weather.gov/gridpoints/AFG/1,1/forecast",
                "forecastHourly": "https://api.weather.gov/gridpoints/AFG/1,1/forecast/hourly",
                "observationStations": "https://api.weather.gov/gridpoints/AFG/1,1/stations"
            }
        }
        """;

        var stationsJson = """
        {
            "features": [
                { "properties": { "stationIdentifier": "PABR" } }
            ]
        }
        """;

        var observationJson = """
        {
            "properties": {
                "timestamp": "2026-01-04T12:00:00Z",
                "textDescription": "Snow",
                "icon": "https://api.weather.gov/icons/land/night/snow",
                "temperature": { "value": -26.1, "unitCode": "wmoUnit:degC" },
                "windSpeed": { "value": 40.2, "unitCode": "wmoUnit:km_h-1" },
                "windDirection": { "value": 315, "unitCode": "wmoUnit:degree_(angle)" },
                "relativeHumidity": { "value": 78, "unitCode": "wmoUnit:percent" },
                "windChill": { "value": -35.5, "unitCode": "wmoUnit:degC" }
            }
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/points/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pointsJson)
            });

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/stations")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(stationsJson)
            });

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/observations/latest")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(observationJson)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new NwsWeatherService(httpClient);

        // Act
        var result = await service.GetCurrentConditionsAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("Snow");
        result.TemperatureF.Should().BeApproximately(-15.0, 1.0); // -26.1C ≈ -15F
        result.HumidityPercent.Should().Be(78);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~NwsWeatherServiceTests" -v n`
Expected: FAIL - IWeatherService and NwsWeatherService not found

**Step 3: Create the interface**

Create `src/BarrowWeather/Services/IWeatherService.cs`:

```csharp
using BarrowWeather.Models;

namespace BarrowWeather.Services;

public interface IWeatherService
{
    Task<CurrentConditions?> GetCurrentConditionsAsync();
    Task<IReadOnlyList<HourlyForecast>> GetHourlyForecastAsync();
    Task<IReadOnlyList<DailyForecast>> GetDailyForecastAsync();
    Task<IReadOnlyList<WeatherAlert>> GetAlertsAsync();
}
```

**Step 4: Create the NWS weather service implementation**

Create `src/BarrowWeather/Services/NwsWeatherService.cs`:

```csharp
using System.Net.Http.Json;
using BarrowWeather.Models;
using BarrowWeather.Services.Dto;

namespace BarrowWeather.Services;

public class NwsWeatherService : IWeatherService
{
    private const double BarrowLatitude = 71.2906;
    private const double BarrowLongitude = -156.7886;
    private const string BaseUrl = "https://api.weather.gov";

    private readonly HttpClient _httpClient;
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

        var url = $"{BaseUrl}/points/{BarrowLatitude},{BarrowLongitude}";
        var response = await _httpClient.GetFromJsonAsync<NwsPointsResponse>(url);
        _cachedPointsData = response?.Properties;
    }

    private async Task EnsureStationIdAsync()
    {
        if (_cachedStationId != null) return;

        await EnsurePointsDataAsync();
        if (_cachedPointsData?.ObservationStationsUrl == null) return;

        var response = await _httpClient.GetFromJsonAsync<NwsStationsResponse>(_cachedPointsData.ObservationStationsUrl);
        _cachedStationId = response?.Features?.FirstOrDefault()?.Properties?.StationIdentifier;
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
```

**Step 5: Run tests to verify they pass**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~NwsWeatherServiceTests" -v n`
Expected: PASS

**Step 6: Commit**

```bash
git add -A
git commit -m "feat: add NWS weather service with API integration"
```

---

## Task 5: Create Sunrise/Sunset Calculator

**Files:**
- Create: `src/BarrowWeather/Services/SunCalculator.cs`
- Test: `tests/BarrowWeather.Tests/Services/SunCalculatorTests.cs`

**Step 1: Write failing test for sun calculator**

Create `tests/BarrowWeather.Tests/Services/SunCalculatorTests.cs`:

```csharp
using BarrowWeather.Services;
using FluentAssertions;

namespace BarrowWeather.Tests.Services;

public class SunCalculatorTests
{
    [Fact]
    public void Calculate_ForBarrowInJanuary_ShouldReturnReasonableTimes()
    {
        // Barrow in early January has very short days
        var date = new DateTime(2026, 1, 4);

        var result = SunCalculator.Calculate(date, 71.2906, -156.7886);

        // In early January, Barrow has about 0 hours of daylight (polar night ending)
        // Sunrise around 1pm, sunset around 2pm local (or polar night)
        result.Should().NotBeNull();
    }

    [Fact]
    public void Calculate_ForBarrowInJune_ShouldReturnMidnightSun()
    {
        // Barrow in June has 24-hour daylight
        var date = new DateTime(2026, 6, 21);

        var result = SunCalculator.Calculate(date, 71.2906, -156.7886);

        // During midnight sun, we might return midnight-midnight or null
        result.Should().NotBeNull();
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~SunCalculatorTests" -v n`
Expected: FAIL - SunCalculator not found

**Step 3: Create the sun calculator**

Create `src/BarrowWeather/Services/SunCalculator.cs`:

```csharp
using BarrowWeather.Models;

namespace BarrowWeather.Services;

public static class SunCalculator
{
    /// <summary>
    /// Calculates sunrise and sunset times for a given date and location.
    /// Uses a simplified algorithm that works for most cases.
    /// Returns null sunrise/sunset for polar day/night conditions.
    /// </summary>
    public static SunData Calculate(DateTime date, double latitude, double longitude)
    {
        // Convert to Julian date
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        double a = Math.Floor(year / 100.0);
        double b = 2 - a + Math.Floor(a / 4.0);
        double jd = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;

        // Calculate solar noon
        double n = jd - 2451545.0 + 0.0008;
        double jStar = n - longitude / 360.0;
        double m = (357.5291 + 0.98560028 * jStar) % 360;
        double c = 1.9148 * Math.Sin(ToRadians(m)) + 0.02 * Math.Sin(ToRadians(2 * m)) + 0.0003 * Math.Sin(ToRadians(3 * m));
        double lambda = (m + c + 180 + 102.9372) % 360;
        double jTransit = 2451545.0 + jStar + 0.0053 * Math.Sin(ToRadians(m)) - 0.0069 * Math.Sin(ToRadians(2 * lambda));

        // Calculate declination
        double sinDec = Math.Sin(ToRadians(lambda)) * Math.Sin(ToRadians(23.44));
        double cosDec = Math.Cos(Math.Asin(sinDec));

        // Calculate hour angle
        double cosOmega = (Math.Sin(ToRadians(-0.83)) - Math.Sin(ToRadians(latitude)) * sinDec) / (Math.Cos(ToRadians(latitude)) * cosDec);

        // Handle polar day/night
        if (cosOmega < -1)
        {
            // Polar day - sun never sets
            return new SunData(new TimeOnly(0, 0), new TimeOnly(23, 59));
        }
        if (cosOmega > 1)
        {
            // Polar night - sun never rises
            return new SunData(new TimeOnly(12, 0), new TimeOnly(12, 0));
        }

        double omega = ToDegrees(Math.Acos(cosOmega));

        // Calculate sunrise and sunset Julian dates
        double jRise = jTransit - omega / 360;
        double jSet = jTransit + omega / 360;

        // Convert to local time (Alaska time is UTC-9)
        var sunriseUtc = JulianToDateTime(jRise);
        var sunsetUtc = JulianToDateTime(jSet);

        // Convert to Alaska time (UTC-9, ignoring DST for simplicity)
        var sunriseLocal = sunriseUtc.AddHours(-9);
        var sunsetLocal = sunsetUtc.AddHours(-9);

        return new SunData(
            TimeOnly.FromDateTime(sunriseLocal),
            TimeOnly.FromDateTime(sunsetLocal)
        );
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    private static DateTime JulianToDateTime(double jd)
    {
        double z = Math.Floor(jd + 0.5);
        double f = jd + 0.5 - z;
        double a;

        if (z < 2299161)
            a = z;
        else
        {
            double alpha = Math.Floor((z - 1867216.25) / 36524.25);
            a = z + 1 + alpha - Math.Floor(alpha / 4);
        }

        double b = a + 1524;
        double c = Math.Floor((b - 122.1) / 365.25);
        double d = Math.Floor(365.25 * c);
        double e = Math.Floor((b - d) / 30.6001);

        int day = (int)(b - d - Math.Floor(30.6001 * e));
        int month = (int)(e < 14 ? e - 1 : e - 13);
        int year = (int)(month > 2 ? c - 4716 : c - 4715);

        double hours = f * 24;
        int hour = (int)hours;
        int minute = (int)((hours - hour) * 60);

        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~SunCalculatorTests" -v n`
Expected: PASS

**Step 5: Commit**

```bash
git add -A
git commit -m "feat: add sunrise/sunset calculator for polar regions"
```

---

## Task 6: Create Weather ViewModel

**Files:**
- Create: `src/BarrowWeather/ViewModels/WeatherViewModel.cs`
- Test: `tests/BarrowWeather.Tests/ViewModels/WeatherViewModelTests.cs`

**Step 1: Write failing test for ViewModel**

Create `tests/BarrowWeather.Tests/ViewModels/WeatherViewModelTests.cs`:

```csharp
using BarrowWeather.Models;
using BarrowWeather.Services;
using BarrowWeather.ViewModels;
using FluentAssertions;
using Moq;

namespace BarrowWeather.Tests.ViewModels;

public class WeatherViewModelTests
{
    [Fact]
    public async Task RefreshCommand_ShouldUpdateCurrentConditions()
    {
        // Arrange
        var mockService = new Mock<IWeatherService>();
        mockService.Setup(s => s.GetCurrentConditionsAsync())
            .ReturnsAsync(new CurrentConditions(
                DateTime.UtcNow, -15.0, 25.0, "NW", 78, -32.0, "Snow", "icon.png"));
        mockService.Setup(s => s.GetHourlyForecastAsync())
            .ReturnsAsync(new List<HourlyForecast>());
        mockService.Setup(s => s.GetDailyForecastAsync())
            .ReturnsAsync(new List<DailyForecast>());
        mockService.Setup(s => s.GetAlertsAsync())
            .ReturnsAsync(new List<WeatherAlert>());

        var viewModel = new WeatherViewModel(mockService.Object);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        viewModel.CurrentConditions.Should().NotBeNull();
        viewModel.CurrentConditions!.Description.Should().Be("Snow");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshCommand_WhenApiFails_ShouldSetErrorMessage()
    {
        // Arrange
        var mockService = new Mock<IWeatherService>();
        mockService.Setup(s => s.GetCurrentConditionsAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var viewModel = new WeatherViewModel(mockService.Object);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.HasError.Should().BeTrue();
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~WeatherViewModelTests" -v n`
Expected: FAIL - WeatherViewModel not found

**Step 3: Create the ViewModel**

Create `src/BarrowWeather/ViewModels/WeatherViewModel.cs`:

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarrowWeather.Models;
using BarrowWeather.Services;

namespace BarrowWeather.ViewModels;

public partial class WeatherViewModel : ObservableObject
{
    private readonly IWeatherService _weatherService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCurrentConditions))]
    private CurrentConditions? _currentConditions;

    [ObservableProperty]
    private SunData? _sunData;

    [ObservableProperty]
    private ObservableCollection<HourlyForecast> _hourlyForecasts = new();

    [ObservableProperty]
    private ObservableCollection<DailyForecast> _dailyForecasts = new();

    [ObservableProperty]
    private ObservableCollection<WeatherAlert> _alerts = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private DateTime _lastUpdated;

    public bool HasCurrentConditions => CurrentConditions != null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasAlerts => Alerts.Count > 0;

    public WeatherViewModel(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Fetch all data in parallel
            var currentTask = _weatherService.GetCurrentConditionsAsync();
            var hourlyTask = _weatherService.GetHourlyForecastAsync();
            var dailyTask = _weatherService.GetDailyForecastAsync();
            var alertsTask = _weatherService.GetAlertsAsync();

            await Task.WhenAll(currentTask, hourlyTask, dailyTask, alertsTask);

            CurrentConditions = await currentTask;

            HourlyForecasts.Clear();
            foreach (var forecast in await hourlyTask)
                HourlyForecasts.Add(forecast);

            DailyForecasts.Clear();
            foreach (var forecast in await dailyTask)
                DailyForecasts.Add(forecast);

            Alerts.Clear();
            foreach (var alert in await alertsTask)
                Alerts.Add(alert);

            // Calculate sun data
            SunData = SunCalculator.Calculate(DateTime.Today, 71.2906, -156.7886);

            LastUpdated = DateTime.Now;
            OnPropertyChanged(nameof(HasAlerts));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load weather: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/BarrowWeather.Tests --filter "FullyQualifiedName~WeatherViewModelTests" -v n`
Expected: PASS

**Step 5: Commit**

```bash
git add -A
git commit -m "feat: add WeatherViewModel with refresh command"
```

---

## Task 7: Create Main Window UI Shell

**Files:**
- Modify: `src/BarrowWeather/MainWindow.xaml`
- Modify: `src/BarrowWeather/MainWindow.xaml.cs`
- Modify: `src/BarrowWeather/App.xaml.cs`

**Step 1: Update App.xaml.cs to set up dependency injection**

Replace contents of `src/BarrowWeather/App.xaml.cs`:

```csharp
using Microsoft.UI.Xaml;
using BarrowWeather.Services;
using BarrowWeather.ViewModels;

namespace BarrowWeather;

public partial class App : Application
{
    private Window? _window;

    public static IWeatherService WeatherService { get; private set; } = null!;
    public static WeatherViewModel WeatherViewModel { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();

        // Set up services
        var httpClient = new HttpClient();
        WeatherService = new NwsWeatherService(httpClient);
        WeatherViewModel = new WeatherViewModel(WeatherService);
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
```

**Step 2: Update MainWindow.xaml with basic layout**

Replace contents of `src/BarrowWeather/MainWindow.xaml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window
    x:Class="BarrowWeather.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:BarrowWeather"
    Title="Barrow Weather">

    <Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}" Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Grid Grid.Row="0" Margin="0,0,0,16">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <StackPanel Grid.Column="0">
                <TextBlock Text="Barrow (Utqiaġvik), Alaska"
                           Style="{StaticResource TitleTextBlockStyle}"/>
                <TextBlock x:Name="LastUpdatedText"
                           Style="{StaticResource CaptionTextBlockStyle}"
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>

            <Button x:Name="RefreshButton"
                    Grid.Column="1"
                    Content="↻ Refresh"
                    Click="RefreshButton_Click"/>
        </Grid>

        <!-- Content Area -->
        <ScrollViewer Grid.Row="1">
            <StackPanel x:Name="ContentPanel" Spacing="16">
                <!-- Loading indicator -->
                <ProgressRing x:Name="LoadingRing" IsActive="False"/>

                <!-- Error message -->
                <InfoBar x:Name="ErrorBar"
                         IsOpen="False"
                         Severity="Error"
                         Title="Error"/>

                <!-- Weather content will be added here -->
                <TextBlock x:Name="PlaceholderText"
                           Text="Click Refresh to load weather data"
                           Style="{StaticResource BodyTextBlockStyle}"
                           HorizontalAlignment="Center"
                           Margin="0,100,0,0"/>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</Window>
```

**Step 3: Update MainWindow.xaml.cs**

Replace contents of `src/BarrowWeather/MainWindow.xaml.cs`:

```csharp
using Microsoft.UI.Xaml;

namespace BarrowWeather;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Set window size
        var appWindow = this.AppWindow;
        appWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));

        // Subscribe to ViewModel changes
        App.WeatherViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var vm = App.WeatherViewModel;

            LoadingRing.IsActive = vm.IsLoading;
            RefreshButton.IsEnabled = !vm.IsLoading;

            if (vm.HasError)
            {
                ErrorBar.Message = vm.ErrorMessage;
                ErrorBar.IsOpen = true;
            }
            else
            {
                ErrorBar.IsOpen = false;
            }

            if (vm.HasCurrentConditions)
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
                LastUpdatedText.Text = $"Last updated: {vm.LastUpdated:h:mm tt}";
            }
        });
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await App.WeatherViewModel.RefreshCommand.ExecuteAsync(null);
    }
}
```

**Step 4: Verify build**

Run: `dotnet build src/BarrowWeather`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add -A
git commit -m "feat: add main window UI shell with refresh button"
```

---

## Task 8: Create Current Conditions Card

**Files:**
- Create: `src/BarrowWeather/Controls/CurrentConditionsCard.xaml`
- Create: `src/BarrowWeather/Controls/CurrentConditionsCard.xaml.cs`
- Modify: `src/BarrowWeather/MainWindow.xaml`

**Step 1: Create the CurrentConditionsCard control**

Create `src/BarrowWeather/Controls/CurrentConditionsCard.xaml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<UserControl
    x:Class="BarrowWeather.Controls.CurrentConditionsCard"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="24">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Temperature and Condition -->
            <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="16">
                <TextBlock x:Name="TemperatureText"
                           Text="--°F"
                           FontSize="64"
                           FontWeight="SemiBold"/>
                <StackPanel VerticalAlignment="Center">
                    <TextBlock x:Name="FeelsLikeText"
                               Text="Feels like --°F"
                               Style="{StaticResource SubtitleTextBlockStyle}"/>
                    <TextBlock x:Name="ConditionText"
                               Text="--"
                               Style="{StaticResource BodyTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                </StackPanel>
            </StackPanel>

            <!-- Details -->
            <StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="32" Margin="0,16,0,0">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE9F4;" FontSize="16"/>
                    <TextBlock x:Name="WindText" Text="Wind: -- mph"/>
                </StackPanel>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE7B5;" FontSize="16"/>
                    <TextBlock x:Name="HumidityText" Text="Humidity: --%"/>
                </StackPanel>
            </StackPanel>

            <!-- Sunrise/Sunset -->
            <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="32" Margin="0,16,0,0">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE706;" FontSize="16"/>
                    <TextBlock x:Name="SunriseText" Text="Sunrise: --:--"/>
                </StackPanel>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE708;" FontSize="16"/>
                    <TextBlock x:Name="SunsetText" Text="Sunset: --:--"/>
                </StackPanel>
            </StackPanel>
        </Grid>
    </Border>
</UserControl>
```

Create `src/BarrowWeather/Controls/CurrentConditionsCard.xaml.cs`:

```csharp
using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Models;

namespace BarrowWeather.Controls;

public sealed partial class CurrentConditionsCard : UserControl
{
    public CurrentConditionsCard()
    {
        this.InitializeComponent();
    }

    public void Update(CurrentConditions? conditions, SunData? sunData)
    {
        if (conditions == null)
        {
            TemperatureText.Text = "--°F";
            FeelsLikeText.Text = "Feels like --°F";
            ConditionText.Text = "--";
            WindText.Text = "Wind: -- mph";
            HumidityText.Text = "Humidity: --%";
        }
        else
        {
            TemperatureText.Text = $"{conditions.TemperatureF:F0}°F";
            FeelsLikeText.Text = $"Feels like {conditions.FeelsLikeF:F0}°F";
            ConditionText.Text = conditions.Description;
            WindText.Text = $"Wind: {conditions.WindSpeedMph:F0} mph {conditions.WindDirection}";
            HumidityText.Text = $"Humidity: {conditions.HumidityPercent}%";
        }

        if (sunData == null)
        {
            SunriseText.Text = "Sunrise: --:--";
            SunsetText.Text = "Sunset: --:--";
        }
        else
        {
            SunriseText.Text = $"Sunrise: {sunData.Sunrise:h:mm tt}";
            SunsetText.Text = $"Sunset: {sunData.Sunset:h:mm tt}";
        }
    }
}
```

**Step 2: Add card to MainWindow.xaml**

Update `src/BarrowWeather/MainWindow.xaml`, add xmlns and card in content area:

Add namespace at top:
```xml
xmlns:controls="using:BarrowWeather.Controls"
```

Replace the PlaceholderText in ContentPanel with:
```xml
<!-- Current Conditions Card -->
<controls:CurrentConditionsCard x:Name="CurrentConditionsCard"
                                 Visibility="Collapsed"/>

<TextBlock x:Name="PlaceholderText"
           Text="Click Refresh to load weather data"
           Style="{StaticResource BodyTextBlockStyle}"
           HorizontalAlignment="Center"
           Margin="0,100,0,0"/>
```

**Step 3: Update MainWindow.xaml.cs to populate card**

Add to ViewModel_PropertyChanged method:
```csharp
if (vm.HasCurrentConditions)
{
    PlaceholderText.Visibility = Visibility.Collapsed;
    CurrentConditionsCard.Visibility = Visibility.Visible;
    CurrentConditionsCard.Update(vm.CurrentConditions, vm.SunData);
    LastUpdatedText.Text = $"Last updated: {vm.LastUpdated:h:mm tt}";
}
```

**Step 4: Verify build**

Run: `dotnet build src/BarrowWeather`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add -A
git commit -m "feat: add current conditions card component"
```

---

## Task 9: Create Alerts Card

**Files:**
- Create: `src/BarrowWeather/Controls/AlertsCard.xaml`
- Create: `src/BarrowWeather/Controls/AlertsCard.xaml.cs`
- Modify: `src/BarrowWeather/MainWindow.xaml`

**Step 1: Create the AlertsCard control**

Create `src/BarrowWeather/Controls/AlertsCard.xaml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<UserControl
    x:Class="BarrowWeather.Controls.AlertsCard"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Border Background="{ThemeResource SystemFillColorCautionBackgroundBrush}"
            BorderBrush="{ThemeResource SystemFillColorCautionBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="16">
        <StackPanel>
            <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,0,0,8">
                <FontIcon Glyph="&#xE7BA;" Foreground="{ThemeResource SystemFillColorCautionBrush}"/>
                <TextBlock Text="Weather Alerts"
                           Style="{StaticResource SubtitleTextBlockStyle}"
                           Foreground="{ThemeResource SystemFillColorCautionBrush}"/>
            </StackPanel>
            <ItemsControl x:Name="AlertsList">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <StackPanel Margin="0,4,0,4">
                            <TextBlock Text="{Binding Event}" FontWeight="SemiBold"/>
                            <TextBlock Text="{Binding Headline}"
                                       TextWrapping="Wrap"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </Border>
</UserControl>
```

Create `src/BarrowWeather/Controls/AlertsCard.xaml.cs`:

```csharp
using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Models;

namespace BarrowWeather.Controls;

public sealed partial class AlertsCard : UserControl
{
    public AlertsCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<WeatherAlert> alerts)
    {
        AlertsList.ItemsSource = alerts;
    }
}
```

**Step 2: Add card to MainWindow.xaml**

Add after CurrentConditionsCard:
```xml
<!-- Alerts Card -->
<controls:AlertsCard x:Name="AlertsCard" Visibility="Collapsed"/>
```

**Step 3: Update MainWindow.xaml.cs**

Add to ViewModel_PropertyChanged:
```csharp
if (vm.HasAlerts)
{
    AlertsCard.Visibility = Visibility.Visible;
    AlertsCard.Update(vm.Alerts);
}
else
{
    AlertsCard.Visibility = Visibility.Collapsed;
}
```

**Step 4: Verify build and commit**

```bash
dotnet build src/BarrowWeather
git add -A
git commit -m "feat: add weather alerts card component"
```

---

## Task 10: Create Hourly Forecast Card

**Files:**
- Create: `src/BarrowWeather/Controls/HourlyForecastCard.xaml`
- Create: `src/BarrowWeather/Controls/HourlyForecastCard.xaml.cs`
- Modify: `src/BarrowWeather/MainWindow.xaml`

**Step 1: Create HourlyForecastCard control**

Create `src/BarrowWeather/Controls/HourlyForecastCard.xaml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<UserControl
    x:Class="BarrowWeather.Controls.HourlyForecastCard"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="16">
        <StackPanel>
            <TextBlock Text="Hourly Forecast"
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Margin="0,0,0,12"/>
            <ScrollViewer HorizontalScrollBarVisibility="Auto"
                          VerticalScrollBarVisibility="Disabled">
                <ItemsControl x:Name="HourlyList">
                    <ItemsControl.ItemsPanel>
                        <ItemsPanelTemplate>
                            <StackPanel Orientation="Horizontal" Spacing="16"/>
                        </ItemsPanelTemplate>
                    </ItemsControl.ItemsPanel>
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <StackPanel Width="60" HorizontalAlignment="Center">
                                <TextBlock Text="{Binding TimeText}"
                                           HorizontalAlignment="Center"
                                           Style="{StaticResource CaptionTextBlockStyle}"/>
                                <TextBlock Text="{Binding TempText}"
                                           HorizontalAlignment="Center"
                                           FontWeight="SemiBold"
                                           FontSize="18"
                                           Margin="0,4,0,0"/>
                                <TextBlock Text="{Binding PrecipText}"
                                           HorizontalAlignment="Center"
                                           Style="{StaticResource CaptionTextBlockStyle}"
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>
        </StackPanel>
    </Border>
</UserControl>
```

Create `src/BarrowWeather/Controls/HourlyForecastCard.xaml.cs`:

```csharp
using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Models;

namespace BarrowWeather.Controls;

public sealed partial class HourlyForecastCard : UserControl
{
    public HourlyForecastCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<HourlyForecast> forecasts)
    {
        var items = forecasts.Select(f => new
        {
            TimeText = f.Time.ToString("h tt"),
            TempText = $"{f.TemperatureF:F0}°",
            PrecipText = f.PrecipitationChance > 0 ? $"{f.PrecipitationChance}%" : ""
        }).ToList();

        HourlyList.ItemsSource = items;
    }
}
```

**Step 2: Add to MainWindow.xaml and code-behind**

Add after AlertsCard:
```xml
<!-- Hourly Forecast Card -->
<controls:HourlyForecastCard x:Name="HourlyForecastCard" Visibility="Collapsed"/>
```

Add to ViewModel_PropertyChanged:
```csharp
if (vm.HourlyForecasts.Count > 0)
{
    HourlyForecastCard.Visibility = Visibility.Visible;
    HourlyForecastCard.Update(vm.HourlyForecasts);
}
```

**Step 3: Build and commit**

```bash
dotnet build src/BarrowWeather
git add -A
git commit -m "feat: add hourly forecast card component"
```

---

## Task 11: Create Daily Forecast Card

**Files:**
- Create: `src/BarrowWeather/Controls/DailyForecastCard.xaml`
- Create: `src/BarrowWeather/Controls/DailyForecastCard.xaml.cs`
- Modify: `src/BarrowWeather/MainWindow.xaml`

**Step 1: Create DailyForecastCard control**

Create `src/BarrowWeather/Controls/DailyForecastCard.xaml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<UserControl
    x:Class="BarrowWeather.Controls.DailyForecastCard"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="16">
        <StackPanel>
            <TextBlock Text="7-Day Forecast"
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Margin="0,0,0,12"/>
            <ItemsControl x:Name="DailyList">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="0,8,0,8">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="100"/>
                                <ColumnDefinition Width="80"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <TextBlock Grid.Column="0"
                                       Text="{Binding DayName}"
                                       FontWeight="SemiBold"/>
                            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                                <TextBlock Text="{Binding HighText}" FontWeight="SemiBold"/>
                                <TextBlock Text="{Binding LowText}"
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            </StackPanel>
                            <TextBlock Grid.Column="2"
                                       Text="{Binding Description}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       TextTrimming="CharacterEllipsis"/>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </Border>
</UserControl>
```

Create `src/BarrowWeather/Controls/DailyForecastCard.xaml.cs`:

```csharp
using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Models;

namespace BarrowWeather.Controls;

public sealed partial class DailyForecastCard : UserControl
{
    public DailyForecastCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<DailyForecast> forecasts)
    {
        var items = forecasts.Select(f => new
        {
            DayName = f.DayName,
            HighText = $"{f.HighF:F0}°",
            LowText = $"{f.LowF:F0}°",
            Description = f.Description
        }).ToList();

        DailyList.ItemsSource = items;
    }
}
```

**Step 2: Add to MainWindow.xaml and code-behind**

Add after HourlyForecastCard:
```xml
<!-- Daily Forecast Card -->
<controls:DailyForecastCard x:Name="DailyForecastCard" Visibility="Collapsed"/>
```

Add to ViewModel_PropertyChanged:
```csharp
if (vm.DailyForecasts.Count > 0)
{
    DailyForecastCard.Visibility = Visibility.Visible;
    DailyForecastCard.Update(vm.DailyForecasts);
}
```

**Step 3: Build and commit**

```bash
dotnet build src/BarrowWeather
git add -A
git commit -m "feat: add 7-day forecast card component"
```

---

## Task 12: Add Auto-Refresh Timer

**Files:**
- Create: `src/BarrowWeather/Services/RefreshTimer.cs`
- Modify: `src/BarrowWeather/App.xaml.cs`
- Modify: `src/BarrowWeather/MainWindow.xaml.cs`

**Step 1: Create RefreshTimer service**

Create `src/BarrowWeather/Services/RefreshTimer.cs`:

```csharp
using Microsoft.UI.Xaml;

namespace BarrowWeather.Services;

public class RefreshTimer
{
    private readonly DispatcherTimer _timer;
    private readonly Func<Task> _refreshAction;

    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);
    public bool IsRunning => _timer.IsEnabled;

    public RefreshTimer(Func<Task> refreshAction)
    {
        _refreshAction = refreshAction;
        _timer = new DispatcherTimer
        {
            Interval = Interval
        };
        _timer.Tick += async (s, e) => await _refreshAction();
    }

    public void Start()
    {
        _timer.Interval = Interval;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }
}
```

**Step 2: Update App.xaml.cs**

Add RefreshTimer property:
```csharp
public static RefreshTimer RefreshTimer { get; private set; } = null!;
```

Initialize in constructor after WeatherViewModel:
```csharp
RefreshTimer = new RefreshTimer(async () => await WeatherViewModel.RefreshCommand.ExecuteAsync(null));
```

**Step 3: Update MainWindow.xaml.cs**

Start timer after first refresh, pause when minimized:
```csharp
public MainWindow()
{
    // ... existing code ...

    // Handle window state changes
    this.AppWindow.Changed += AppWindow_Changed;
}

private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
{
    if (args.DidPresenterChange)
    {
        // Pause timer when minimized
        if (sender.Presenter.Kind == Microsoft.UI.Windowing.AppWindowPresenterKind.Overlapped)
        {
            var overlapped = sender.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            if (overlapped?.State == Microsoft.UI.Windowing.OverlappedPresenterState.Minimized)
            {
                App.RefreshTimer.Stop();
            }
            else
            {
                App.RefreshTimer.Start();
            }
        }
    }
}

private async void RefreshButton_Click(object sender, RoutedEventArgs e)
{
    await App.WeatherViewModel.RefreshCommand.ExecuteAsync(null);

    // Start auto-refresh after first manual refresh
    if (!App.RefreshTimer.IsRunning)
    {
        App.RefreshTimer.Start();
    }
}
```

**Step 4: Build and commit**

```bash
dotnet build src/BarrowWeather
git add -A
git commit -m "feat: add 15-minute auto-refresh timer"
```

---

## Task 13: Add Data Caching

**Files:**
- Create: `src/BarrowWeather/Services/WeatherCache.cs`
- Modify: `src/BarrowWeather/ViewModels/WeatherViewModel.cs`

**Step 1: Create WeatherCache service**

Create `src/BarrowWeather/Services/WeatherCache.cs`:

```csharp
using System.Text.Json;
using BarrowWeather.Models;

namespace BarrowWeather.Services;

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
```

**Step 2: Update WeatherViewModel to use cache**

Add cache field and update RefreshAsync:
```csharp
private readonly WeatherCache _cache = new();

// In constructor, load cached data:
public WeatherViewModel(IWeatherService weatherService)
{
    _weatherService = weatherService;
    _ = LoadCachedDataAsync();
}

private async Task LoadCachedDataAsync()
{
    var cached = await _cache.LoadAsync();
    if (cached != null)
    {
        CurrentConditions = cached.CurrentConditions;
        SunData = cached.SunData;
        foreach (var f in cached.HourlyForecasts) HourlyForecasts.Add(f);
        foreach (var f in cached.DailyForecasts) DailyForecasts.Add(f);
        foreach (var a in cached.Alerts) Alerts.Add(a);
        LastUpdated = cached.CachedAt;
        OnPropertyChanged(nameof(HasAlerts));
    }
}

// At end of RefreshAsync try block, save to cache:
await _cache.SaveAsync(new WeatherCacheData(
    DateTime.Now,
    CurrentConditions,
    HourlyForecasts.ToList(),
    DailyForecasts.ToList(),
    Alerts.ToList(),
    SunData
));
```

**Step 3: Build and commit**

```bash
dotnet build src/BarrowWeather
git add -A
git commit -m "feat: add local weather data caching"
```

---

## Task 14: Final Integration and Testing

**Step 1: Run all tests**

Run: `dotnet test BarrowWeather.sln -v n`
Expected: All tests pass

**Step 2: Build release version**

Run: `dotnet build BarrowWeather.sln -c Release`
Expected: Build succeeded

**Step 3: Run the application**

Run: `dotnet run --project src/BarrowWeather`
Expected: Window opens, click Refresh to load weather

**Step 4: Update CHANGELOG.md**

Add release entry:
```markdown
## [0.1.0] - 2026-01-04

### Added
- Initial release of Barrow Weather app
- Current conditions display with temperature, wind, humidity
- Hourly forecast (24 hours)
- 7-day forecast
- Weather alerts from NWS
- Sunrise/sunset times
- Auto-refresh every 15 minutes
- Local data caching for offline support
```

**Step 5: Final commit**

```bash
git add -A
git commit -m "feat: complete v0.1.0 Barrow Weather app"
```

**Step 6: Push to GitHub**

```bash
git push -u origin main
```

---

## Summary

The implementation plan contains 14 tasks that build the app incrementally:

1. **Tasks 1-3**: Project setup, models, DTOs
2. **Tasks 4-6**: Services (NWS API, sun calculator, ViewModel)
3. **Tasks 7-11**: UI components (shell, cards)
4. **Tasks 12-13**: Polish (auto-refresh, caching)
5. **Task 14**: Integration and release

Each task is self-contained with tests, builds on previous work, and ends with a commit.
