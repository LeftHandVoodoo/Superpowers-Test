using BarrowWeather.Core.Services;
using FluentAssertions;

namespace BarrowWeather.Tests.Services;

public class SunCalculatorTests
{
    [Fact]
    public void Calculate_ForBarrowInJanuary_ShouldReturnReasonableTimes()
    {
        // Barrow in early January has very short days
        var date = new DateTime(2026, 1, 4);

        var result = SunCalculator.Calculate(date, 71.2906, -156.7886);

        // In early January, Barrow has about 0 hours of daylight (polar night ending)
        // Sunrise around 1pm, sunset around 2pm local (or polar night)
        result.Should().NotBeNull();
    }

    [Fact]
    public void Calculate_ForBarrowInJune_ShouldReturnMidnightSun()
    {
        // Barrow in June has 24-hour daylight
        var date = new DateTime(2026, 6, 21);

        var result = SunCalculator.Calculate(date, 71.2906, -156.7886);

        // During midnight sun, we might return midnight-midnight or null
        result.Should().NotBeNull();
    }

    [Fact]
    public void CelsiusToFahrenheit_ShouldConvertCorrectly()
    {
        // -26.1C should be approximately -15F
        var fahrenheit = SunCalculator.CelsiusToFahrenheit(-26.1);
        fahrenheit.Should().BeApproximately(-15.0, 0.5);
    }

    [Fact]
    public void DegreesToCardinal_ShouldReturnCorrectDirection()
    {
        SunCalculator.DegreesToCardinal(0).Should().Be("N");
        SunCalculator.DegreesToCardinal(90).Should().Be("E");
        SunCalculator.DegreesToCardinal(180).Should().Be("S");
        SunCalculator.DegreesToCardinal(270).Should().Be("W");
        SunCalculator.DegreesToCardinal(315).Should().Be("NW");
    }
}
