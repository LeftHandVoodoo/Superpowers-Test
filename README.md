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

## Building Releases

### Local Build

Build release packages for all platforms:

```bash
python build_release.py
```

Build for a specific platform:

```bash
python build_release.py --platform x64
```

### Creating a GitHub Release

1. Update the version in `VERSION` file
2. Update `CHANGELOG.md` with release notes
3. Create and push a git tag:

```bash
python create_release.py
```

Or manually:

```bash
git tag -a v0.2.3 -m "Release 0.2.3"
git push origin v0.2.3
```

The GitHub Actions workflow will automatically:
- Build the application for all platforms (x64, x86, ARM64)
- Create MSIX installers
- Create ZIP archives
- Create a GitHub release with all artifacts

### Installing from Release

1. Download the MSIX package from the [Releases](https://github.com/LeftHandVoodoo/Superpowers-Test/releases) page
2. Double-click the `.msix` file to install
3. The app will appear in your Start menu

**Note:** MSIX packages require Windows 10 (build 17763) or later.

## Project Structure

```
src/
├── BarrowWeather/        # WinUI 3 desktop app
│   ├── Assets/          # App icons and images for MSIX packaging
│   └── Package.appxmanifest  # MSIX package manifest
└── BarrowWeather.Core/   # Core library (models, services, API)
tests/
└── BarrowWeather.Tests/  # xUnit tests
.github/
└── workflows/
    └── release.yml       # GitHub Actions release workflow
```

Root-level scripts:
- `run.py` - Launch the application
- `build_release.py` - Build release packages
- `create_release.py` - Create GitHub releases

## License

MIT
