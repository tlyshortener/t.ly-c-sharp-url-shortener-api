using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tly;

public sealed class LinkStatsResponse
{
    [JsonPropertyName("clicks")]
    public int Clicks { get; set; }

    [JsonPropertyName("unique_clicks")]
    public int UniqueClicks { get; set; }

    [JsonPropertyName("total_qr_scans")]
    public int? TotalQrScans { get; set; }

    [JsonPropertyName("browsers")]
    public List<BrowserStat> Browsers { get; set; } = new();

    [JsonPropertyName("countries")]
    public List<CountryStat> Countries { get; set; } = new();

    [JsonPropertyName("cities")]
    public List<CityStat> Cities { get; set; } = new();

    [JsonPropertyName("referrers")]
    public List<ReferrerStat> Referrers { get; set; } = new();

    [JsonPropertyName("platforms")]
    public List<PlatformStat> Platforms { get; set; } = new();

    [JsonPropertyName("daily_clicks")]
    public List<DailyClickStat> DailyClicks { get; set; } = new();

    [JsonPropertyName("link_clicks")]
    public List<LinkClickStat> LinkClicks { get; set; } = new();

    [JsonPropertyName("data")]
    public LinkStatsData? Data { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class LinkStatsData
{
    [JsonPropertyName("short_url")]
    public string? ShortUrl { get; set; }

    [JsonPropertyName("long_url")]
    public string? LongUrl { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("last_clicked")]
    public DateTimeOffset? LastClicked { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class BrowserStat
{
    [JsonPropertyName("browser")]
    public string? Browser { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }
}

public sealed class CountryStat
{
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }
}

public sealed class CityStat
{
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }
}

public sealed class ReferrerStat
{
    [JsonPropertyName("referrer")]
    public string? Referrer { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }
}

public sealed class PlatformStat
{
    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }
}

public sealed class DailyClickStat
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }
}

public sealed class LinkClickStat
{
    [JsonPropertyName("link_id")]
    public int? LinkId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }
}
