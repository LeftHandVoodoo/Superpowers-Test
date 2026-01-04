using System.Net;
using BarrowWeather.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace BarrowWeather.Tests.Services;

public class NwsWeatherServiceTests
{
    [Fact]
    public async Task GetCurrentConditionsAsync_ShouldParseApiResponse()
    {
        // Arrange
        var pointsJson = """
        {
            "properties": {
                "forecast": "https://api.weather.gov/gridpoints/AFG/1,1/forecast",
                "forecastHourly": "https://api.weather.gov/gridpoints/AFG/1,1/forecast/hourly",
                "observationStations": "https://api.weather.gov/gridpoints/AFG/1,1/stations"
            }
        }
        """;

        var stationsJson = """
        {
            "features": [
                { "properties": { "stationIdentifier": "PABR" } }
            ]
        }
        """;

        var observationJson = """
        {
            "properties": {
                "timestamp": "2026-01-04T12:00:00Z",
                "textDescription": "Snow",
                "icon": "https://api.weather.gov/icons/land/night/snow",
                "temperature": { "value": -26.1, "unitCode": "wmoUnit:degC" },
                "windSpeed": { "value": 40.2, "unitCode": "wmoUnit:km_h-1" },
                "windDirection": { "value": 315, "unitCode": "wmoUnit:degree_(angle)" },
                "relativeHumidity": { "value": 78, "unitCode": "wmoUnit:percent" },
                "windChill": { "value": -35.5, "unitCode": "wmoUnit:degC" }
            }
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/points/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pointsJson)
            });

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/stations")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(stationsJson)
            });

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/observations/latest")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(observationJson)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new NwsWeatherService(httpClient);

        // Act
        var result = await service.GetCurrentConditionsAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("Snow");
        result.TemperatureF.Should().BeApproximately(-15.0, 1.0); // -26.1C ≈ -15F
        result.HumidityPercent.Should().Be(78);
    }
}
