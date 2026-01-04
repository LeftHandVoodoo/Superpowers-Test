using Microsoft.UI.Xaml.Controls;
using BarrowWeather.Core.Models;

namespace BarrowWeather.Controls;

public sealed partial class CurrentConditionsCard : UserControl
{
    public CurrentConditionsCard()
    {
        this.InitializeComponent();
    }

    public void Update(CurrentConditions? conditions, SunData? sunData)
    {
        if (conditions == null)
        {
            TemperatureText.Text = "--°F";
            FeelsLikeText.Text = "Feels like --°F";
            ConditionText.Text = "--";
            WindText.Text = "Wind: -- mph";
            HumidityText.Text = "Humidity: --%";
        }
        else
        {
            TemperatureText.Text = $"{conditions.TemperatureF:F0}°F";
            FeelsLikeText.Text = $"Feels like {conditions.FeelsLikeF:F0}°F";
            ConditionText.Text = conditions.Description;
            WindText.Text = $"Wind: {conditions.WindSpeedMph:F0} mph {conditions.WindDirection}";
            HumidityText.Text = $"Humidity: {conditions.HumidityPercent}%";
        }

        if (sunData == null)
        {
            SunriseText.Text = "Sunrise: --:--";
            SunsetText.Text = "Sunset: --:--";
        }
        else
        {
            SunriseText.Text = $"Sunrise: {sunData.Sunrise:h:mm tt}";
            SunsetText.Text = $"Sunset: {sunData.Sunset:h:mm tt}";
        }
    }
}
