using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using BarrowWeather.Core.Models;
using Windows.UI;

namespace BarrowWeather.Controls;

public sealed partial class HourlyForecastCard : UserControl
{
    // Sky blue highlight for current hour
    private static readonly SolidColorBrush CurrentHourBackground =
        new(Color.FromArgb(51, 0, 120, 212)); // #330078D4
    private static readonly SolidColorBrush DefaultBackground =
        new(Microsoft.UI.Colors.Transparent);

    public HourlyForecastCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<HourlyForecast> forecasts)
    {
        var items = forecasts.Select((f, index) => new
        {
            TimeText = f.Time.ToString("h tt"),
            TempText = $"{f.TemperatureF:F0}°",
            PrecipText = f.PrecipitationChance > 0 ? $"{f.PrecipitationChance}%" : "",
            Background = index == 0 ? CurrentHourBackground : DefaultBackground
        }).ToList();

        HourlyList.ItemsSource = items;
    }
}
