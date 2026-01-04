using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Core.Models;

namespace BarrowWeather.Controls;

public sealed partial class DailyForecastCard : UserControl
{
    public DailyForecastCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<DailyForecast> forecasts)
    {
        var items = forecasts.Select(f => new
        {
            DayName = f.DayName,
            HighText = $"{f.HighF:F0}°",
            LowText = $"{f.LowF:F0}°",
            Description = f.Description
        }).ToList();

        DailyList.ItemsSource = items;
    }
}
