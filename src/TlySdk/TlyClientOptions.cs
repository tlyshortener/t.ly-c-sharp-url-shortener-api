using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tly;

public sealed class TlyClientOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public Uri BaseUri { get; set; } = new("https://api.t.ly/");

    public string UserAgent { get; set; } = "Tly.UrlShortener/1.0.0";

    public JsonSerializerOptions JsonSerializerOptions { get; set; } = CreateDefaultJsonSerializerOptions();

    internal static JsonSerializerOptions CreateDefaultJsonSerializerOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
        };
    }
}
