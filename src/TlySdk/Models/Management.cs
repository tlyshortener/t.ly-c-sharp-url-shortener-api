using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tly;

public sealed class Tag
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tag")]
    public string? Value { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class CreateTagRequest
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;
}

public sealed class UpdateTagRequest
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;
}

public sealed class Pixel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("pixel_id")]
    public string? PixelId { get; set; }

    [JsonPropertyName("pixel_type")]
    public string? PixelType { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class CreatePixelRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pixel_id")]
    public string PixelId { get; set; } = string.Empty;

    [JsonPropertyName("pixel_type")]
    public string PixelType { get; set; } = string.Empty;
}

public sealed class UpdatePixelRequest
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pixel_id")]
    public string PixelId { get; set; } = string.Empty;

    [JsonPropertyName("pixel_type")]
    public string PixelType { get; set; } = string.Empty;
}

public sealed class UtmPreset
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    [JsonPropertyName("campaign")]
    public string? Campaign { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("term")]
    public string? Term { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class UpsertUtmPresetRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    [JsonPropertyName("campaign")]
    public string? Campaign { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("term")]
    public string? Term { get; set; }
}

public sealed class QrCodeResponse
{
    public string? ContentType { get; set; }

    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string? Text { get; set; }

    public string? Base64 { get; set; }

    public bool IsBase64 => !string.IsNullOrWhiteSpace(Base64);
}

public sealed class UpdateQrCodeRequest
{
    [JsonPropertyName("short_url")]
    public string ShortUrl { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("corner_dots_color")]
    public string? CornerDotsColor { get; set; }

    [JsonPropertyName("dots_color")]
    public string? DotsColor { get; set; }

    [JsonPropertyName("dots_style")]
    public string? DotsStyle { get; set; }

    [JsonPropertyName("corner_style")]
    public string? CornerStyle { get; set; }
}

public sealed class QrCodeOptions
{
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("corner_dots_color")]
    public string? CornerDotsColor { get; set; }

    [JsonPropertyName("dots_color")]
    public string? DotsColor { get; set; }

    [JsonPropertyName("dots_style")]
    public string? DotsStyle { get; set; }

    [JsonPropertyName("corner_style")]
    public string? CornerStyle { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class QrCodeSettings
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("short_url")]
    public string? ShortUrl { get; set; }

    [JsonPropertyName("qr_code_options")]
    public QrCodeOptions? QrCodeOptions { get; set; }

    [JsonPropertyName("team_id")]
    public int? TeamId { get; set; }

    [JsonPropertyName("user_id")]
    public int? UserId { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class OneLink
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("short_id")]
    public string? ShortId { get; set; }

    [JsonPropertyName("short_url")]
    public string? ShortUrl { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("meta")]
    public JsonElement? Meta { get; set; }

    [JsonPropertyName("template_key")]
    public string? TemplateKey { get; set; }

    [JsonPropertyName("theme_overrides_json")]
    public JsonElement? ThemeOverridesJson { get; set; }

    [JsonPropertyName("blocks_json")]
    public JsonElement? BlocksJson { get; set; }

    [JsonPropertyName("has_password")]
    public bool HasPassword { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("user")]
    public UserSummary? User { get; set; }

    [JsonPropertyName("links")]
    public List<OneLinkLink> Links { get; set; } = new();

    [JsonPropertyName("social_links")]
    public List<OneLinkLink> SocialLinks { get; set; } = new();

    [JsonPropertyName("stat_total")]
    public OneLinkStatTotal? StatTotal { get; set; }

    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class OneLinkLink
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("sort_order")]
    public int? SortOrder { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class OneLinkStatTotal
{
    [JsonPropertyName("one_link_id")]
    public int? OneLinkId { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("unique_total")]
    public int? UniqueTotal { get; set; }

    [JsonPropertyName("qr_total")]
    public int? QrTotal { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
