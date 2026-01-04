using System.Text.Json.Serialization;

namespace BarrowWeather.Core.Services.Dto;

public class NwsStationsResponse
{
    [JsonPropertyName("features")]
    public List<NwsStationFeature>? Features { get; set; }
}

public class NwsStationFeature
{
    [JsonPropertyName("properties")]
    public NwsStationProperties? Properties { get; set; }
}

public class NwsStationProperties
{
    [JsonPropertyName("stationIdentifier")]
    public string? StationIdentifier { get; set; }
}
