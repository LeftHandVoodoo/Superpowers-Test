# Architecture

## Overview

Barrow Weather is a Windows 11 desktop application built with WinUI 3 that displays weather conditions for Barrow (Utqiaġvik), Alaska. The application follows the **Model-View-ViewModel (MVVM)** pattern and uses a layered architecture to separate concerns between the UI, business logic, and data access.

## Architecture Pattern

The application uses **MVVM (Model-View-ViewModel)** pattern:

- **Model**: Domain models representing weather data (`CurrentConditions`, `HourlyForecast`, `DailyForecast`, `WeatherAlert`, `SunData`)
- **View**: XAML UI components (`MainWindow.xaml`, card controls)
- **ViewModel**: `WeatherViewModel` that coordinates between the view and services, using CommunityToolkit.Mvvm for property change notifications and commands

## Project Structure

```text
src/
├── BarrowWeather/              # WinUI 3 Desktop Application (UI Layer)
│   ├── Controls/              # Reusable UI card components
│   ├── Converters/            # Value converters for data binding
│   ├── Services/              # UI-specific services (RefreshTimer)
│   ├── Themes/                # XAML styles and resources
│   ├── App.xaml.cs            # Application entry point and DI setup
│   └── MainWindow.xaml        # Main application window
│
└── BarrowWeather.Core/         # Core Library (Business Logic & Data)
    ├── Models/                # Domain models
    ├── Services/              # Weather service implementations
    │   └── Dto/              # Data Transfer Objects for API responses
    └── ViewModels/            # ViewModels

tests/
└── BarrowWeather.Tests/        # xUnit test suite
```

## Key Components

### UI Layer (`BarrowWeather`)

#### App.xaml.cs

- Application entry point
- Sets up dependency injection (creates `HttpClient`, `NwsWeatherService`, `WeatherViewModel`)
- Initializes `RefreshTimer` for auto-refresh functionality
- Exposes static properties for global access to services and ViewModel

#### MainWindow.xaml.cs

- Main window controller
- Subscribes to `WeatherViewModel` property changes
- Updates UI elements based on ViewModel state
- Handles window state changes (pauses timer when minimized)
- Manages refresh button click events
- Dynamically updates background gradient based on weather conditions

#### Card Controls

- `CurrentConditionsCard`: Displays current temperature, wind, humidity, feels-like, and sun times
- `HourlyForecastCard`: Scrollable horizontal list of 24-hour forecast
- `DailyForecastCard`: 7-day forecast grid
- `AlertsCard`: Weather alerts display

#### RefreshTimer

- UI-specific service that triggers automatic refresh every 15 minutes
- Can be paused/resumed based on window state

### Core Layer (`BarrowWeather.Core`)

#### ViewModels

`WeatherViewModel`:

- Coordinates data fetching from `IWeatherService`
- Manages observable collections for forecasts and alerts
- Handles loading states and error messages
- Implements caching via `WeatherCache`
- Calculates sun data using `SunCalculator`
- Exposes `RefreshCommand` for manual refresh

#### Services

`IWeatherService` (interface):

- Defines contract for weather data retrieval
- Methods: `GetCurrentConditionsAsync()`, `GetHourlyForecastAsync()`, `GetDailyForecastAsync()`, `GetAlertsAsync()`

`NwsWeatherService` (implementation):

- Implements `IWeatherService` using National Weather Service API
- Caches points data and station ID to minimize API calls
- Converts API responses (Celsius, km/h) to user-friendly units (Fahrenheit, mph)
- Handles coordinate-to-cardinal direction conversion
- Uses DTO classes in `Services/Dto/` for deserializing JSON responses

`WeatherCache`:

- Persists weather data to local JSON file (`%LocalAppData%\BarrowWeather\weather-cache.json`)
- Enables offline functionality by loading cached data on startup
- Saves data after successful refresh

`SunCalculator`:

- Calculates sunrise/sunset times for a given date and coordinates

#### Models

Domain models are immutable records:

- `CurrentConditions`: Temperature, wind, humidity, feels-like, description, icon
- `HourlyForecast`: Time, temperature, description, precipitation chance
- `DailyForecast`: Day name, high/low temperatures, description, icon
- `WeatherAlert`: Severity, event type, headline, expiration
- `SunData`: Sunrise and sunset times

#### DTOs (`Services/Dto/`)

Data Transfer Objects for NWS API responses:

- `NwsPointsResponse`: Grid point data
- `NwsStationsResponse`: Weather station list
- `NwsObservationResponse`: Current conditions
- `NwsForecastResponse`: Forecast data (hourly and daily)
- `NwsAlertsResponse`: Active alerts

### Test Layer (`BarrowWeather.Tests`)

- Uses xUnit testing framework
- Tests cover models, services, and ViewModels
- Tests verify data transformations, API response handling, and ViewModel behavior

## Data Flow

1. **Application Startup**:
   - `App.xaml.cs` creates `HttpClient` → `NwsWeatherService` → `WeatherViewModel`
   - `WeatherViewModel` constructor loads cached data from `WeatherCache`
   - UI displays cached data immediately (if available)

2. **Manual Refresh**:
   - User clicks refresh button → `MainWindow` calls `WeatherViewModel.RefreshCommand`
   - `WeatherViewModel` sets `IsLoading = true`
   - Parallel API calls via `IWeatherService`:
     - Current conditions
     - Hourly forecast
     - Daily forecast
     - Alerts
   - `SunCalculator` calculates sun data
   - ViewModel updates observable properties
   - `WeatherCache` saves data to disk
   - UI updates via property change notifications

3. **Auto-Refresh**:
   - `RefreshTimer` triggers every 15 minutes
   - Calls `WeatherViewModel.RefreshCommand` automatically
   - Timer pauses when window is minimized

4. **UI Updates**:
   - `MainWindow` subscribes to `PropertyChanged` events
   - Updates visibility of cards based on data availability
   - Updates loading indicators and error messages
   - Dynamically changes background gradient based on weather condition

## Dependencies

### External Libraries

- **CommunityToolkit.Mvvm**: MVVM helpers (ObservableObject, RelayCommand, source generators)
- **Microsoft.WindowsAppSDK**: WinUI 3 framework
- **System.Text.Json**: JSON serialization for caching

### External APIs

- **National Weather Service API** (`api.weather.gov`):
  - No API key required
  - Free public API
  - Endpoints used:
    - `/points/{lat},{lon}` - Grid point data
    - `/stations/{stationId}/observations/latest` - Current conditions
    - `/gridpoints/{office}/{gridX},{gridY}/forecast` - Daily forecast
    - `/gridpoints/{office}/{gridX},{gridY}/forecast/hourly` - Hourly forecast
    - `/alerts/active?point={lat},{lon}` - Active alerts

## Design Decisions

1. **Layered Architecture**: Separation of UI (`BarrowWeather`) and business logic (`BarrowWeather.Core`) enables testability and reusability

2. **Interface-Based Services**: `IWeatherService` allows for easy mocking in tests and potential future alternative implementations

3. **Offline-First**: Caching strategy ensures the app works offline and provides immediate data on startup

4. **Parallel API Calls**: Weather data is fetched in parallel to minimize total load time

5. **Immutable Models**: Using records ensures data integrity and thread safety

6. **MVVM Pattern**: Clear separation of concerns, testable ViewModels, and data binding support

7. **Static Service Access**: While not ideal for testability, static properties in `App` simplify access across the UI layer (acceptable trade-off for a small desktop app)

8. **Hardcoded Location**: Barrow coordinates are hardcoded since this is a single-location weather app

## Future Considerations

- Dependency injection container for better testability
- Configuration file for location (if multi-location support is needed)
- Settings UI for refresh interval customization
- Unit conversion options (metric/imperial)
- Multiple weather service providers for redundancy
