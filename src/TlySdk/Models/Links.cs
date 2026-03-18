using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tly;

public sealed class CreateShortLinkRequest
{
    [JsonPropertyName("include_qr_code")]
    public bool? IncludeQrCode { get; set; }

    [JsonPropertyName("long_url")]
    public string LongUrl { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("short_id")]
    public string? ShortId { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("expire_at_time")]
    public string? ExpireAtTime { get; set; }

    [JsonPropertyName("expire_at_datetime")]
    [JsonConverter(typeof(TlyDateTimeJsonConverter))]
    public DateTimeOffset? ExpireAtDatetime { get; set; }

    [JsonPropertyName("expire_at_views")]
    public int? ExpireAtViews { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("public_stats")]
    public bool? PublicStats { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }

    [JsonPropertyName("pixels")]
    public List<int>? Pixels { get; set; }

    [JsonPropertyName("meta")]
    public object? Meta { get; set; }
}

public sealed class UpdateShortLinkRequest
{
    [JsonPropertyName("short_url")]
    public string ShortUrl { get; set; } = string.Empty;

    [JsonPropertyName("short_id")]
    public string? ShortId { get; set; }

    [JsonPropertyName("long_url")]
    public string? LongUrl { get; set; }

    [JsonPropertyName("expire_at_time")]
    public string? ExpireAtTime { get; set; }

    [JsonPropertyName("expire_at_datetime")]
    [JsonConverter(typeof(TlyDateTimeJsonConverter))]
    public DateTimeOffset? ExpireAtDatetime { get; set; }

    [JsonPropertyName("expire_at_views")]
    public int? ExpireAtViews { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("public_stats")]
    public bool? PublicStats { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }

    [JsonPropertyName("pixels")]
    public List<int>? Pixels { get; set; }

    [JsonPropertyName("meta")]
    public object? Meta { get; set; }
}

public sealed class ExpandShortLinkRequest
{
    [JsonPropertyName("short_url")]
    public string ShortUrl { get; set; } = string.Empty;

    [JsonPropertyName("is_qr_code")]
    public bool? IsQrCode { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

public sealed class ExpandShortLinkResponse
{
    [JsonPropertyName("long_url")]
    public string? LongUrl { get; set; }

    [JsonPropertyName("expired")]
    public bool Expired { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class ListShortLinksRequest
{
    public string? Search { get; set; }

    public List<int>? TagIds { get; set; }

    public List<int>? PixelIds { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public List<int>? Domains { get; set; }

    public List<int>? UserIds { get; set; }

    public int? Limit { get; set; }

    public int? Page { get; set; }
}

public sealed class BulkCreateShortLinksRequest
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("links")]
    public List<BulkCreateShortLinkItem> Links { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }

    [JsonPropertyName("pixels")]
    public List<int>? Pixels { get; set; }
}

public sealed class BulkCreateShortLinkItem
{
    [JsonPropertyName("long_url")]
    public string LongUrl { get; set; } = string.Empty;

    [JsonPropertyName("short_id")]
    public string? ShortId { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class BulkUpdateShortLinksRequest
{
    [JsonPropertyName("links")]
    public List<BulkUpdateShortLinkItem> Links { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }

    [JsonPropertyName("pixels")]
    public List<int>? Pixels { get; set; }
}

public sealed class BulkUpdateShortLinkItem
{
    [JsonPropertyName("short_url")]
    public string ShortUrl { get; set; } = string.Empty;

    [JsonPropertyName("long_url")]
    public string? LongUrl { get; set; }

    [JsonPropertyName("short_id")]
    public string? ShortId { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class ShortLink
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("long_url")]
    public string? LongUrl { get; set; }

    [JsonPropertyName("short_url")]
    public string? ShortUrl { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("short_id")]
    public string? ShortId { get; set; }

    [JsonPropertyName("user_id")]
    public int? UserId { get; set; }

    [JsonPropertyName("team_id")]
    public int? TeamId { get; set; }

    [JsonPropertyName("domain_id")]
    public int? DomainId { get; set; }

    [JsonPropertyName("expire_at_views")]
    public int? ExpireAtViews { get; set; }

    [JsonPropertyName("expire_at_datetime")]
    [JsonConverter(typeof(TlyDateTimeJsonConverter))]
    public DateTimeOffset? ExpireAtDatetime { get; set; }

    [JsonPropertyName("public_stats")]
    public bool? PublicStats { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("qr_code_url")]
    public string? QrCodeUrl { get; set; }

    [JsonPropertyName("qr_code_base64")]
    public string? QrCodeBase64 { get; set; }

    [JsonPropertyName("has_password")]
    public bool? HasPassword { get; set; }

    [JsonPropertyName("meta")]
    public JsonElement? Meta { get; set; }

    [JsonPropertyName("short_stats")]
    public Dictionary<string, JsonElement>? ShortStats { get; set; }

    [JsonPropertyName("user")]
    public UserSummary? User { get; set; }

    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = new();

    [JsonPropertyName("pixels")]
    public List<Pixel> Pixels { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class UserSummary
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public sealed class PaginatedResponse<T>
{
    [JsonPropertyName("current_page")]
    public int? CurrentPage { get; set; }

    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("per_page")]
    public int? PerPage { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
