# Changelog

All notable changes to Barrow Weather will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure with WinUI 3 app and core class library
- Solution setup with test project using xUnit, Moq, and FluentAssertions
- Weather data models (CurrentConditions, HourlyForecast, DailyForecast, WeatherAlert, SunData)
- NWS API response DTOs for JSON deserialization
- NWS weather service with API integration for current conditions, forecasts, and alerts
- Sunrise/sunset calculator for polar regions with Julian date calculations
- WeatherViewModel with MVVM pattern using CommunityToolkit.Mvvm
