using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace BarrowWeather;

public sealed partial class MainWindow : Window
{
    private Storyboard? _refreshRotation;

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

        // Create refresh icon rotation animation
        CreateRefreshAnimation();
    }

    private void CreateRefreshAnimation()
    {
        _refreshRotation = new Storyboard();
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(1)),
            RepeatBehavior = RepeatBehavior.Forever
        };
        Storyboard.SetTarget(animation, RefreshIcon);
        Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(RotateTransform.Angle)");
        _refreshRotation.Children.Add(animation);

        RefreshIcon.RenderTransform = new RotateTransform();
        RefreshIcon.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
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

            // Animate refresh icon
            if (vm.IsLoading)
            {
                _refreshRotation?.Begin();
            }
            else
            {
                _refreshRotation?.Stop();
            }

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

                // Show fresh indicator if data is less than 5 minutes old
                var dataAge = DateTime.Now - vm.LastUpdated;
                FreshIndicator.Visibility = dataAge.TotalMinutes < 5
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                // Update background based on weather condition
                UpdateBackgroundForCondition(vm.CurrentConditions?.Description);
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

    private void UpdateBackgroundForCondition(string? condition)
    {
        if (string.IsNullOrEmpty(condition))
            return;

        var lowerCondition = condition.ToLowerInvariant();
        string resourceKey;

        if (lowerCondition.Contains("snow") || lowerCondition.Contains("rain") ||
            lowerCondition.Contains("sleet") || lowerCondition.Contains("freezing"))
        {
            resourceKey = "SnowGradient";
        }
        else if (lowerCondition.Contains("cloud") || lowerCondition.Contains("overcast") ||
                 lowerCondition.Contains("fog"))
        {
            resourceKey = "CloudyGradient";
        }
        else if (DateTime.Now.Hour < 6 || DateTime.Now.Hour > 20)
        {
            resourceKey = "NightGradient";
        }
        else
        {
            resourceKey = "ClearSkyGradient";
        }

        if (Application.Current.Resources.TryGetValue(resourceKey, out var brush))
        {
            RootGrid.Background = (Brush)brush;
        }
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
