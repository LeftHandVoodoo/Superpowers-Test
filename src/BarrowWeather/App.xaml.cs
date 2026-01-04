using Microsoft.UI.Xaml;
using BarrowWeather.Core.Services;
using BarrowWeather.Core.ViewModels;
using BarrowWeather.Services;

namespace BarrowWeather;

public partial class App : Application
{
    private Window? _window;

    public static IWeatherService WeatherService { get; private set; } = null!;
    public static WeatherViewModel WeatherViewModel { get; private set; } = null!;
    public static RefreshTimer RefreshTimer { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();

        // Set up services
        var httpClient = new HttpClient();
        WeatherService = new NwsWeatherService(httpClient);
        WeatherViewModel = new WeatherViewModel(WeatherService);
        RefreshTimer = new RefreshTimer(async () => await WeatherViewModel.RefreshCommand.ExecuteAsync(null));
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
