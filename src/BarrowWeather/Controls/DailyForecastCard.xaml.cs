using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using BarrowWeather.Core.Models;
using Windows.UI;

namespace BarrowWeather.Controls;

public sealed partial class DailyForecastCard : UserControl
{
    // Sky blue highlight for today
    private static readonly SolidColorBrush TodayBackground =
        new(Color.FromArgb(51, 0, 120, 212)); // #330078D4
    private static readonly SolidColorBrush DefaultBackground =
        new(Microsoft.UI.Colors.Transparent);

    public DailyForecastCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<DailyForecast> forecasts)
    {
        var items = forecasts.Select((f, index) => new
        {
            DayName = f.DayName,
            HighText = $"{f.HighF:F0}°",
            LowText = $"{f.LowF:F0}°",
            Description = f.Description,
            Background = index == 0 ? TodayBackground : DefaultBackground,
            DayFontWeight = index == 0 ? FontWeights.Bold : FontWeights.SemiBold
        }).ToList();

        DailyList.ItemsSource = items;
    }
}
