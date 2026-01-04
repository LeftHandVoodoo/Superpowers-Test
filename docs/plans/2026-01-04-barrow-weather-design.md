# Barrow Weather App - Design Document

**Date:** 2026-01-04
**Status:** Approved
**Version:** 0.1.0

## Overview

A Windows 11 desktop application that displays comprehensive weather information for Barrow (Utqiaġvik), Alaska. The app uses a card-based dashboard layout with auto-refresh functionality.

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Framework | C# + WinUI 3 | Native Windows 11 look and feel |
| Data Source | National Weather Service API | Free, no API key, reliable Alaska coverage |
| Architecture | MVVM | Clean separation of UI and logic |
| Refresh | Auto (15 min timer) | Fresh data without complexity |

## Architecture

### Project Structure

```
BarrowWeather/
├── BarrowWeather.sln
├── BarrowWeather/
│   ├── App.xaml / App.xaml.cs      # App entry point
│   ├── MainWindow.xaml/.cs          # Main window shell
│   ├── Views/
│   │   └── WeatherDashboard.xaml/.cs
│   ├── ViewModels/
│   │   └── WeatherViewModel.cs
│   ├── Models/
│   │   ├── CurrentConditions.cs
│   │   ├── HourlyForecast.cs
│   │   ├── DailyForecast.cs
│   │   └── WeatherAlert.cs
│   ├── Services/
│   │   ├── NwsWeatherService.cs     # NWS API client
│   │   └── WeatherRefreshService.cs # Timer-based refresh
│   └── Assets/
│       └── WeatherIcons/            # Condition icons
```

### Key Dependencies

- WinUI 3 (Microsoft.WindowsAppSDK)
- CommunityToolkit.Mvvm (for MVVM helpers)
- System.Net.Http.Json (for API calls)
- Microsoft.Extensions.Logging (structured logging)

## NWS API Integration

### API Flow

The NWS API requires a two-step process:

1. **Point metadata**: `GET https://api.weather.gov/points/71.2906,-156.7886`
   - Returns forecast URLs specific to Barrow/Utqiaġvik
   - Cache this response (rarely changes)

2. **Weather data** (from URLs returned above):
   - `/forecast` - 7-day forecast with day/night periods
   - `/forecast/hourly` - Hourly forecast for ~156 hours
   - `/stations/{stationId}/observations/latest` - Current conditions
   - `/alerts/active?point=71.2906,-156.7886` - Active weather alerts

### Data Models

```csharp
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

public record HourlyForecast(
    DateTime Time,
    double TemperatureF,
    string ShortDescription,
    int PrecipitationChance
);

public record DailyForecast(
    string DayName,
    double HighF,
    double LowF,
    string Description,
    string IconUrl
);

public record WeatherAlert(
    string Severity,
    string Event,
    string Headline,
    DateTime Expires
);
```

Sunrise/sunset calculated locally using coordinates (NWS doesn't provide this).

## UI Layout

### Dashboard Structure

```
┌─────────────────────────────────────────────────────────┐
│  Barrow (Utqiaġvik), Alaska              [↻ Refresh]    │
│  Last updated: 2:45 PM                                  │
├─────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────┐ │
│ │  CURRENT CONDITIONS (Hero Card)                     │ │
│ │  ❄️  -15°F  Feels like -32°F                        │ │
│ │  Snow | Wind: 25 mph NW | Humidity: 78%             │ │
│ │  ☀ Sunrise: 10:45 AM  🌙 Sunset: 3:12 PM            │ │
│ └─────────────────────────────────────────────────────┘ │
│ ┌──────────────────────┐ ┌──────────────────────────┐   │
│ │  ⚠️ ALERTS           │ │  HOURLY (scrollable)     │   │
│ │  Blizzard Warning    │ │  3PM  4PM  5PM  6PM ...  │   │
│ │  Until Tue 6:00 AM   │ │  -15  -17  -18  -19 ...  │   │
│ └──────────────────────┘ └──────────────────────────┘   │
│ ┌─────────────────────────────────────────────────────┐ │
│ │  7-DAY FORECAST                                     │ │
│ │  Mon    Tue    Wed    Thu    Fri    Sat    Sun      │ │
│ │  -12°   -8°    -5°    -10°   -15°   -18°   -14°     │ │
│ │  -22°   -18°   -15°   -20°   -25°   -28°   -24°     │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### Card Styling

- Rounded corners (8px radius) matching Windows 11 aesthetic
- Subtle shadows and Mica/Acrylic backdrop material
- Weather-appropriate accent colors (blue for cold, white for snow)
- Condition icons from NWS or custom weather icon set

### Window Size

- Default: 800x600
- Minimum: 600x500
- Resizable

## Refresh Logic

### Auto-Refresh Behavior

- Refreshes every 15 minutes by default
- Timer pauses when window is minimized
- Manual refresh button always available
- Visual indicator shows time since last update

### Error Handling

| Scenario | Behavior |
|----------|----------|
| No internet | Show last cached data + "Offline" badge |
| API timeout | Retry once after 30 seconds, then show error |
| API error (500) | Show cached data + "Update failed" message |
| Invalid data | Log error, keep previous valid data |

### Caching

- Cache weather data to local JSON file in AppData
- On startup, show cached data immediately while fetching fresh
- Cache point metadata indefinitely (forecast URLs don't change)

## Testing

### Unit Tests (xUnit)

- `NwsWeatherService` - mock HTTP responses, verify parsing
- `WeatherViewModel` - verify property updates, refresh commands
- Sunrise/sunset calculator - verify against known values for Barrow

### Integration Tests

- Live API call test (marked to skip in CI if needed)
- Verify full data flow from API to ViewModel

### Test Project Structure

```
BarrowWeather.Tests/
├── Services/
│   └── NwsWeatherServiceTests.cs
├── ViewModels/
│   └── WeatherViewModelTests.cs
└── TestData/
    └── sample-nws-responses.json
```

## Build & Packaging

- **Target:** Windows 10 (1809+) and Windows 11
- **Package:** MSIX for clean install/uninstall
- **Deployment:** Self-contained (no .NET runtime required on target)
- **Packaging:** Single-project MSIX via Windows App SDK

### Requirements

- .NET 8
- Windows App SDK 1.5+
- Visual Studio 2022 or `dotnet build` from CLI
