using System.Text.Json.Serialization;

namespace BarrowWeather.Core.Services.Dto;

public class NwsAlertsResponse
{
    [JsonPropertyName("features")]
    public List<NwsAlertFeature>? Features { get; set; }
}

public class NwsAlertFeature
{
    [JsonPropertyName("properties")]
    public NwsAlertProperties? Properties { get; set; }
}

public class NwsAlertProperties
{
    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }
}
