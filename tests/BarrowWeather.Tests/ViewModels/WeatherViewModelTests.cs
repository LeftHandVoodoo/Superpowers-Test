using BarrowWeather.Core.Models;
using BarrowWeather.Core.Services;
using BarrowWeather.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace BarrowWeather.Tests.ViewModels;

public class WeatherViewModelTests
{
    [Fact]
    public async Task RefreshCommand_ShouldUpdateCurrentConditions()
    {
        // Arrange
        var mockService = new Mock<IWeatherService>();
        mockService.Setup(s => s.GetCurrentConditionsAsync())
            .ReturnsAsync(new CurrentConditions(
                DateTime.UtcNow, -15.0, 25.0, "NW", 78, -32.0, "Snow", "icon.png"));
        mockService.Setup(s => s.GetHourlyForecastAsync())
            .ReturnsAsync(new List<HourlyForecast>());
        mockService.Setup(s => s.GetDailyForecastAsync())
            .ReturnsAsync(new List<DailyForecast>());
        mockService.Setup(s => s.GetAlertsAsync())
            .ReturnsAsync(new List<WeatherAlert>());

        var viewModel = new WeatherViewModel(mockService.Object);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        viewModel.CurrentConditions.Should().NotBeNull();
        viewModel.CurrentConditions!.Description.Should().Be("Snow");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshCommand_WhenApiFails_ShouldSetErrorMessage()
    {
        // Arrange
        var mockService = new Mock<IWeatherService>();
        mockService.Setup(s => s.GetCurrentConditionsAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var viewModel = new WeatherViewModel(mockService.Object);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.HasError.Should().BeTrue();
    }
}
