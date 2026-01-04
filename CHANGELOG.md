# Changelog

All notable changes to Barrow Weather will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.3] - 2026-01-04

### Fixed
- Cache file corruption on crash (now uses atomic write with temp file + rename)
- Silent exception swallowing in cache load (now catches specific JsonException/IOException)
- Unhandled exceptions in background cache loading (added try/catch with error reporting)
- Potential race condition in NWS API initialization (added SemaphoreSlim for thread safety)
- Unhandled exceptions in auto-refresh timer (wrapped async handler in try/catch)
- Build error in csproj (changed ItemGroup to PropertyGroup for PackageCertificateKeyFile)

## [0.2.2] - 2026-01-04

### Added
- `run.py` Python script for launching the application with platform and configuration options
- `build_release.py` script for building release packages and MSIX installers
- `create_release.py` script for creating GitHub releases
- GitHub Actions workflow for automated release builds and packaging
- MSIX packaging support with `Package.appxmanifest`
- Release documentation in README.md

## [0.2.1] - 2026-01-04

### Added
- ARCHITECTURE.md documenting the application's architecture, design patterns, and component structure

## [0.2.0] - 2026-01-04

### Added
- Centralized theme system with Sky Blue accent color (#0078D4)
- Dynamic gradient backgrounds based on weather conditions (clear, cloudy, snow, night)
- Frosted glass card styling using AcrylicBrush
- Hero temperature display (96px) in sky blue
- Current hour highlight in hourly forecast
- Today row highlight in daily forecast
- Refresh button rotation animation during loading
- Fresh data indicator (green dot) for data less than 5 minutes old
- Temperature-to-color converters for visual temperature cues
- Accent-styled card headers with underline

### Changed
- Updated all cards to use frosted glass appearance
- Improved alerts card with orange warning styling
- Enhanced visual hierarchy with consistent spacing and typography

## [0.1.0] - 2026-01-04

### Added
- Initial release of Barrow Weather app
- Current conditions display with temperature, feels like, wind, humidity
- Hourly forecast (24 hours) with horizontal scrolling
- 7-day forecast with high/low temperatures and descriptions
- Weather alerts from NWS with caution-styled display
- Sunrise/sunset times calculated for polar regions
- Auto-refresh every 15 minutes with pause on minimize
- Local data caching for offline support
- WinUI 3 app with Windows 11 design patterns
- Core class library with MVVM pattern using CommunityToolkit.Mvvm
- NWS API integration for Barrow (Utqiagvik), Alaska
