# Barrow Weather

A Windows 11 desktop application that displays weather conditions for Barrow (Utqiaġvik), Alaska.

## Features

- Current conditions with temperature, wind, humidity, and feels-like
- Hourly forecast (scrollable)
- 7-day forecast
- Weather alerts
- Sunrise/sunset times
- Auto-refresh every 15 minutes
- Offline caching

## Technology

- C# / .NET 8
- WinUI 3 (Windows App SDK)
- National Weather Service API (free, no API key required)

## Building

Requires:
- Visual Studio 2022 with .NET desktop development workload
- Windows App SDK 1.5+

```bash
dotnet build BarrowWeather.sln
```

## License

MIT
