using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Core.Models;

namespace BarrowWeather.Controls;

public sealed partial class AlertsCard : UserControl
{
    public AlertsCard()
    {
        this.InitializeComponent();
    }

    public void Update(IEnumerable<WeatherAlert> alerts)
    {
        AlertsList.ItemsSource = alerts;
    }
}
