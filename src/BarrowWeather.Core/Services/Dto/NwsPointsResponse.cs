using System.Text.Json.Serialization;

namespace BarrowWeather.Core.Services.Dto;

public class NwsPointsResponse
{
    [JsonPropertyName("properties")]
    public NwsPointsProperties? Properties { get; set; }
}

public class NwsPointsProperties
{
    [JsonPropertyName("forecast")]
    public string? ForecastUrl { get; set; }

    [JsonPropertyName("forecastHourly")]
    public string? ForecastHourlyUrl { get; set; }

    [JsonPropertyName("observationStations")]
    public string? ObservationStationsUrl { get; set; }
}
