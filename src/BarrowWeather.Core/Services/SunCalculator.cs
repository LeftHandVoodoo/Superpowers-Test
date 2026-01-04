using BarrowWeather.Core.Models;

namespace BarrowWeather.Core.Services;

public static class SunCalculator
{
    /// <summary>
    /// Calculates sunrise and sunset times for a given date and location.
    /// Uses a simplified algorithm that works for most cases.
    /// Returns null sunrise/sunset for polar day/night conditions.
    /// </summary>
    public static SunData Calculate(DateTime date, double latitude, double longitude)
    {
        // Convert to Julian date
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        double a = Math.Floor(year / 100.0);
        double b = 2 - a + Math.Floor(a / 4.0);
        double jd = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;

        // Calculate solar noon
        double n = jd - 2451545.0 + 0.0008;
        double jStar = n - longitude / 360.0;
        double m = (357.5291 + 0.98560028 * jStar) % 360;
        double c = 1.9148 * Math.Sin(ToRadians(m)) + 0.02 * Math.Sin(ToRadians(2 * m)) + 0.0003 * Math.Sin(ToRadians(3 * m));
        double lambda = (m + c + 180 + 102.9372) % 360;
        double jTransit = 2451545.0 + jStar + 0.0053 * Math.Sin(ToRadians(m)) - 0.0069 * Math.Sin(ToRadians(2 * lambda));

        // Calculate declination
        double sinDec = Math.Sin(ToRadians(lambda)) * Math.Sin(ToRadians(23.44));
        double cosDec = Math.Cos(Math.Asin(sinDec));

        // Calculate hour angle
        double cosOmega = (Math.Sin(ToRadians(-0.83)) - Math.Sin(ToRadians(latitude)) * sinDec) / (Math.Cos(ToRadians(latitude)) * cosDec);

        // Handle polar day/night
        if (cosOmega < -1)
        {
            // Polar day - sun never sets
            return new SunData(new TimeOnly(0, 0), new TimeOnly(23, 59));
        }
        if (cosOmega > 1)
        {
            // Polar night - sun never rises
            return new SunData(new TimeOnly(12, 0), new TimeOnly(12, 0));
        }

        double omega = ToDegrees(Math.Acos(cosOmega));

        // Calculate sunrise and sunset Julian dates
        double jRise = jTransit - omega / 360;
        double jSet = jTransit + omega / 360;

        // Convert to local time (Alaska time is UTC-9)
        var sunriseUtc = JulianToDateTime(jRise);
        var sunsetUtc = JulianToDateTime(jSet);

        // Convert to Alaska time (UTC-9, ignoring DST for simplicity)
        var sunriseLocal = sunriseUtc.AddHours(-9);
        var sunsetLocal = sunsetUtc.AddHours(-9);

        return new SunData(
            TimeOnly.FromDateTime(sunriseLocal),
            TimeOnly.FromDateTime(sunsetLocal)
        );
    }

    public static double CelsiusToFahrenheit(double celsius)
    {
        return Math.Round(celsius * 9 / 5 + 32, 1);
    }

    public static string DegreesToCardinal(double degrees)
    {
        string[] cardinals = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        int index = (int)Math.Round(degrees / 22.5) % 16;
        return cardinals[index];
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    private static DateTime JulianToDateTime(double jd)
    {
        double z = Math.Floor(jd + 0.5);
        double f = jd + 0.5 - z;
        double a;

        if (z < 2299161)
            a = z;
        else
        {
            double alpha = Math.Floor((z - 1867216.25) / 36524.25);
            a = z + 1 + alpha - Math.Floor(alpha / 4);
        }

        double b = a + 1524;
        double c = Math.Floor((b - 122.1) / 365.25);
        double d = Math.Floor(365.25 * c);
        double e = Math.Floor((b - d) / 30.6001);

        int day = (int)(b - d - Math.Floor(30.6001 * e));
        int month = (int)(e < 14 ? e - 1 : e - 13);
        int year = (int)(month > 2 ? c - 4716 : c - 4715);

        double hours = f * 24;
        int hour = (int)hours;
        int minute = (int)((hours - hour) * 60);

        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }
}
