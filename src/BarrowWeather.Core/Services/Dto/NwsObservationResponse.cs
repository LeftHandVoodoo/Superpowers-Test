using System.Text.Json.Serialization;

namespace BarrowWeather.Core.Services.Dto;

public class NwsObservationResponse
{
    [JsonPropertyName("properties")]
    public NwsObservationProperties? Properties { get; set; }
}

public class NwsObservationProperties
{
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("textDescription")]
    public string? TextDescription { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("temperature")]
    public NwsQuantitativeValue? Temperature { get; set; }

    [JsonPropertyName("windSpeed")]
    public NwsQuantitativeValue? WindSpeed { get; set; }

    [JsonPropertyName("windDirection")]
    public NwsQuantitativeValue? WindDirection { get; set; }

    [JsonPropertyName("relativeHumidity")]
    public NwsQuantitativeValue? RelativeHumidity { get; set; }

    [JsonPropertyName("windChill")]
    public NwsQuantitativeValue? WindChill { get; set; }
}

public class NwsQuantitativeValue
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("unitCode")]
    public string? UnitCode { get; set; }
}
