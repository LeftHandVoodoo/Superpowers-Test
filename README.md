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
- WinUI 3 (Windows App SDK 1.5)
- National Weather Service API (free, no API key required)

## Prerequisites

- Windows 10 (build 17763) or Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building

```bash
dotnet build BarrowWeather.sln -p:Platform=x64
```

Supported platforms: `x64`, `x86`, `ARM64`

## Running

```bash
dotnet run --project src/BarrowWeather/BarrowWeather.csproj -p:Platform=x64
```

Or run the built executable directly:
```
src\BarrowWeather\bin\x64\Debug\net8.0-windows10.0.19041.0\BarrowWeather.exe
```

## Testing

```bash
dotnet test
```

## Project Structure

```
src/
├── BarrowWeather/        # WinUI 3 desktop app
└── BarrowWeather.Core/   # Core library (models, services, API)
tests/
└── BarrowWeather.Tests/  # xUnit tests
```

## License

MIT
