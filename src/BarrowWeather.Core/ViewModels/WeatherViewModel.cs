using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarrowWeather.Core.Models;
using BarrowWeather.Core.Services;

namespace BarrowWeather.Core.ViewModels;

public partial class WeatherViewModel : ObservableObject
{
    private readonly IWeatherService _weatherService;
    private readonly WeatherCache _cache = new();

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

            // Save to cache
            await _cache.SaveAsync(new WeatherCacheData(
                DateTime.Now,
                CurrentConditions,
                HourlyForecasts.ToList(),
                DailyForecasts.ToList(),
                Alerts.ToList(),
                SunData
            ));
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
