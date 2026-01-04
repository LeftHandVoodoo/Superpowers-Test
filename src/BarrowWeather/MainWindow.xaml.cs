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
                CurrentConditionsCard.Visibility = Visibility.Visible;
                CurrentConditionsCard.Update(vm.CurrentConditions, vm.SunData);
                LastUpdatedText.Text = $"Last updated: {vm.LastUpdated:h:mm tt}";
            }

            if (vm.HasAlerts)
            {
                AlertsCard.Visibility = Visibility.Visible;
                AlertsCard.Update(vm.Alerts);
            }
            else
            {
                AlertsCard.Visibility = Visibility.Collapsed;
            }

            if (vm.HourlyForecasts.Count > 0)
            {
                HourlyForecastCard.Visibility = Visibility.Visible;
                HourlyForecastCard.Update(vm.HourlyForecasts);
            }

            if (vm.DailyForecasts.Count > 0)
            {
                DailyForecastCard.Visibility = Visibility.Visible;
                DailyForecastCard.Update(vm.DailyForecasts);
            }
        });
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
}
