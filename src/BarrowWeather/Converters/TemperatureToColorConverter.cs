using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace BarrowWeather.Converters;

public class TemperatureToColorConverter : IValueConverter
{
    // Temperature thresholds
    private const double FreezingPoint = 32.0;
    private const double ExtremeCode = 0.0;

    // Tint colors
    private static readonly Color IcyBlueTint = Color.FromArgb(40, 227, 242, 253);    // #E3F2FD with alpha
    private static readonly Color CoolBlueTint = Color.FromArgb(30, 187, 222, 251);   // #BBDEFB with alpha
    private static readonly Color WarmTint = Color.FromArgb(25, 255, 248, 225);       // #FFF8E1 with alpha

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not double temperature)
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        Color tintColor;
        if (temperature < ExtremeCode)
        {
            // Below 0°F - Icy blue
            tintColor = IcyBlueTint;
        }
        else if (temperature < FreezingPoint)
        {
            // 0-32°F - Cool blue
            tintColor = CoolBlueTint;
        }
        else
        {
            // Above 32°F - Warm (rare for Barrow!)
            tintColor = WarmTint;
        }

        return new SolidColorBrush(tintColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class TemperatureToTextColorConverter : IValueConverter
{
    private static readonly Color SkyBlue = Color.FromArgb(255, 0, 120, 212);     // #0078D4
    private static readonly Color IcyBlue = Color.FromArgb(255, 100, 181, 246);   // Lighter blue for extreme cold
    private static readonly Color WarmOrange = Color.FromArgb(255, 255, 183, 77); // Warm accent

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not double temperature)
            return new SolidColorBrush(SkyBlue);

        Color textColor;
        if (temperature < 0)
        {
            // Extreme cold - icy blue
            textColor = IcyBlue;
        }
        else if (temperature > 32)
        {
            // Above freezing - warm accent
            textColor = WarmOrange;
        }
        else
        {
            // Normal cold - sky blue
            textColor = SkyBlue;
        }

        return new SolidColorBrush(textColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
