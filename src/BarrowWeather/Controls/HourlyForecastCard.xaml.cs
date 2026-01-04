using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Core.Models;

namespace BarrowWeather.Controls;

public sealed partial class HourlyForecastCard : UserControl
{
    public HourlyForecastCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<HourlyForecast> forecasts)
    {
        var items = forecasts.Select(f => new
        {
            TimeText = f.Time.ToString("h tt"),
            TempText = $"{f.TemperatureF:F0}°",
            PrecipText = f.PrecipitationChance > 0 ? $"{f.PrecipitationChance}%" : ""
        }).ToList();

        HourlyList.ItemsSource = items;
    }
}
