using Microsoft.UI.Xaml;

namespace BarrowWeather.Services;

public class RefreshTimer
{
    private readonly DispatcherTimer _timer;
    private readonly Func<Task> _refreshAction;

    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);
    public bool IsRunning => _timer.IsEnabled;

    public RefreshTimer(Func<Task> refreshAction)
    {
        _refreshAction = refreshAction;
        _timer = new DispatcherTimer
        {
            Interval = Interval
        };
        _timer.Tick += async (s, e) => await _refreshAction();
    }

    public void Start()
    {
        _timer.Interval = Interval;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }
}
