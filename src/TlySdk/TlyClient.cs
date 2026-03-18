using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Tly;

public sealed class TlyClient : IDisposable
{
    private const string JsonMediaType = "application/json";
    private const string TextMediaType = "text/plain";
    private static readonly string[] DefaultAcceptMediaTypes = { JsonMediaType, TextMediaType };
    private static readonly string[] TextPreferredAcceptMediaTypes = { TextMediaType, JsonMediaType };
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private readonly string _apiKey;
    private readonly string _userAgent;
    private readonly JsonSerializerOptions _serializerOptions;

    public TlyClient(string apiKey, HttpClient? httpClient = null)
        : this(new TlyClientOptions { ApiKey = apiKey }, httpClient)
    {
    }

    public TlyClient(TlyClientOptions options, HttpClient? httpClient = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        EnsureNotNullOrWhiteSpace(options.ApiKey, nameof(options.ApiKey));

        if (options.BaseUri is null || !options.BaseUri.IsAbsoluteUri)
        {
            throw new ArgumentException("BaseUri must be an absolute URI.", nameof(options));
        }

        _apiKey = options.ApiKey;
        _userAgent = options.UserAgent;
        _serializerOptions = new JsonSerializerOptions(options.JsonSerializerOptions ?? TlyClientOptions.CreateDefaultJsonSerializerOptions());
        _httpClient = httpClient ?? new HttpClient();
        _disposeHttpClient = httpClient is null;

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = options.BaseUri;
        }
    }

    public Task<ShortLink> CreateShortLinkAsync(CreateShortLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.LongUrl, nameof(request.LongUrl));

        if (string.Equals(request.Format, "text", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("CreateShortLinkAsync only supports JSON responses. Use CreateShortLinkTextAsync for plain-text output.", nameof(request));
        }

        return SendAsync<ShortLink>(HttpMethod.Post, "/api/v1/link/shorten", request, cancellationToken: cancellationToken);
    }

    public Task<string> CreateShortLinkTextAsync(CreateShortLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.LongUrl, nameof(request.LongUrl));
        return SendAsync<string>(
            HttpMethod.Post,
            "/api/v1/link/shorten",
            CreateCreateShortLinkBody(request, "text"),
            cancellationToken: cancellationToken,
            acceptMediaTypes: TextPreferredAcceptMediaTypes);
    }

    public Task<ShortLink> GetShortLinkAsync(string shortUrl, CancellationToken cancellationToken = default)
    {
        EnsureNotNullOrWhiteSpace(shortUrl, nameof(shortUrl));
        var query = new QueryStringBuilder().Add("short_url", shortUrl);
        return SendAsync<ShortLink>(HttpMethod.Get, "/api/v1/link", query: query, cancellationToken: cancellationToken);
    }

    public Task<ShortLink> UpdateShortLinkAsync(UpdateShortLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.ShortUrl, nameof(request.ShortUrl));
        return SendAsync<ShortLink>(HttpMethod.Put, "/api/v1/link", request, cancellationToken: cancellationToken);
    }

    public Task DeleteShortLinkAsync(string shortUrl, CancellationToken cancellationToken = default)
    {
        EnsureNotNullOrWhiteSpace(shortUrl, nameof(shortUrl));
        return SendWithoutResultAsync(HttpMethod.Delete, "/api/v1/link", new { short_url = shortUrl }, cancellationToken: cancellationToken);
    }

    public Task<ExpandShortLinkResponse> ExpandShortLinkAsync(ExpandShortLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.ShortUrl, nameof(request.ShortUrl));
        return SendAsync<ExpandShortLinkResponse>(HttpMethod.Post, "/api/v1/link/expand", request, cancellationToken: cancellationToken);
    }

    public Task<PaginatedResponse<ShortLink>> ListShortLinksAsync(ListShortLinksRequest? request = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryStringBuilder();

        if (request is not null)
        {
            query
                .Add("search", request.Search)
                .AddList("tag_ids", request.TagIds)
                .AddList("pixel_ids", request.PixelIds)
                .Add("start_date", FormatDateTime(request.StartDate))
                .Add("end_date", FormatDateTime(request.EndDate))
                .AddList("domains", request.Domains)
                .AddList("user_ids", request.UserIds)
                .Add("limit", request.Limit)
                .Add("page", request.Page);
        }

        return SendAsync<PaginatedResponse<ShortLink>>(HttpMethod.Get, "/api/v1/link/list", query: query, cancellationToken: cancellationToken);
    }

    public Task<bool> BulkCreateShortLinksAsync(BulkCreateShortLinksRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.Domain, nameof(request.Domain));

        if (request.Links.Count == 0)
        {
            throw new ArgumentException("At least one link is required.", nameof(request));
        }

        return SendAsync<bool>(HttpMethod.Post, "/api/v1/link/bulk", request, cancellationToken: cancellationToken);
    }

    public Task<bool> BulkUpdateShortLinksAsync(BulkUpdateShortLinksRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Links.Count == 0)
        {
            throw new ArgumentException("At least one link is required.", nameof(request));
        }

        return SendAsync<bool>(HttpMethod.Post, "/api/v1/link/bulk/update", request, cancellationToken: cancellationToken);
    }

    public Task<LinkStatsResponse> GetLinkStatsAsync(string shortUrl, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken cancellationToken = default)
    {
        EnsureNotNullOrWhiteSpace(shortUrl, nameof(shortUrl));
        var query = new QueryStringBuilder()
            .Add("short_url", shortUrl)
            .Add("start_date", FormatDate(startDate))
            .Add("end_date", FormatDate(endDate));

        return SendAsync<LinkStatsResponse>(HttpMethod.Get, "/api/v1/link/stats", query: query, cancellationToken: cancellationToken);
    }

    public Task<UtmPreset> CreateUtmPresetAsync(UpsertUtmPresetRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUtmPresetRequest(request);
        return SendAsync<UtmPreset>(HttpMethod.Post, "/api/v1/link/utm-preset", request, cancellationToken: cancellationToken);
    }

    public Task<List<UtmPreset>> ListUtmPresetsAsync(CancellationToken cancellationToken = default)
    {
        return SendListAsync<UtmPreset>(HttpMethod.Get, "/api/v1/link/utm-preset", cancellationToken: cancellationToken);
    }

    public Task<UtmPreset> GetUtmPresetAsync(int id, CancellationToken cancellationToken = default)
    {
        return SendAsync<UtmPreset>(HttpMethod.Get, $"/api/v1/link/utm-preset/{id}", cancellationToken: cancellationToken);
    }

    public Task<UtmPreset> UpdateUtmPresetAsync(int id, UpsertUtmPresetRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUtmPresetRequest(request);
        return SendAsync<UtmPreset>(HttpMethod.Put, $"/api/v1/link/utm-preset/{id}", request, cancellationToken: cancellationToken);
    }

    public Task DeleteUtmPresetAsync(int id, CancellationToken cancellationToken = default)
    {
        return SendWithoutResultAsync(HttpMethod.Delete, $"/api/v1/link/utm-preset/{id}", cancellationToken: cancellationToken);
    }

    public Task<PaginatedResponse<OneLink>> ListOneLinksAsync(int? page = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryStringBuilder().Add("page", page);
        return SendAsync<PaginatedResponse<OneLink>>(HttpMethod.Get, "/api/v1/onelink/list", query: query, cancellationToken: cancellationToken);
    }

    public Task<LinkStatsResponse> GetOneLinkStatsAsync(string shortUrl, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken cancellationToken = default)
    {
        EnsureNotNullOrWhiteSpace(shortUrl, nameof(shortUrl));
        var query = new QueryStringBuilder()
            .Add("short_url", shortUrl)
            .Add("start_date", FormatDate(startDate))
            .Add("end_date", FormatDate(endDate));

        return SendAsync<LinkStatsResponse>(HttpMethod.Get, "/api/v1/onelink/stats", query: query, cancellationToken: cancellationToken);
    }

    public Task DeleteOneLinkStatsAsync(string shortUrl, CancellationToken cancellationToken = default)
    {
        EnsureNotNullOrWhiteSpace(shortUrl, nameof(shortUrl));
        return SendWithoutResultAsync(HttpMethod.Delete, "/api/v1/onelink/stat", new { short_url = shortUrl }, cancellationToken: cancellationToken);
    }

    public Task<Pixel> CreatePixelAsync(CreatePixelRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePixelCreateRequest(request);
        return SendAsync<Pixel>(HttpMethod.Post, "/api/v1/link/pixel", request, cancellationToken: cancellationToken);
    }

    public Task<List<Pixel>> ListPixelsAsync(CancellationToken cancellationToken = default)
    {
        return SendListAsync<Pixel>(HttpMethod.Get, "/api/v1/link/pixel", cancellationToken: cancellationToken);
    }

    public Task<Pixel> GetPixelAsync(int id, CancellationToken cancellationToken = default)
    {
        return SendAsync<Pixel>(HttpMethod.Get, $"/api/v1/link/pixel/{id}", cancellationToken: cancellationToken);
    }

    public Task<Pixel> UpdatePixelAsync(int id, UpdatePixelRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePixelUpdateRequest(request);
        return SendAsync<Pixel>(HttpMethod.Put, $"/api/v1/link/pixel/{id}", request, cancellationToken: cancellationToken);
    }

    public Task DeletePixelAsync(int id, CancellationToken cancellationToken = default)
    {
        return SendWithoutResultAsync(HttpMethod.Delete, $"/api/v1/link/pixel/{id}", cancellationToken: cancellationToken);
    }

    public Task<QrCodeResponse> GetQrCodeAsync(string shortUrl, string? output = null, CancellationToken cancellationToken = default)
    {
        EnsureNotNullOrWhiteSpace(shortUrl, nameof(shortUrl));
        var query = new QueryStringBuilder()
            .Add("short_url", shortUrl)
            .Add("output", output);

        return SendQrCodeAsync("/api/v1/link/qr-code", query, output, cancellationToken);
    }

    [Obsolete("The format parameter is not supported by the current T.LY API. Use the overload without format.")]
    public Task<QrCodeResponse> GetQrCodeAsync(string shortUrl, string? output, string? format, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("The current T.LY API does not support the format query parameter.", nameof(format));
        }

        return GetQrCodeAsync(shortUrl, output, cancellationToken);
    }

    public Task<QrCodeSettings> UpdateQrCodeAsync(UpdateQrCodeRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.ShortUrl, nameof(request.ShortUrl));
        return SendAsync<QrCodeSettings>(HttpMethod.Put, "/api/v1/link/qr-code", request, cancellationToken: cancellationToken);
    }

    public Task<List<Tag>> ListTagsAsync(CancellationToken cancellationToken = default)
    {
        return SendListAsync<Tag>(HttpMethod.Get, "/api/v1/link/tag", cancellationToken: cancellationToken);
    }

    public Task<Tag> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTagCreateRequest(request);
        return SendAsync<Tag>(HttpMethod.Post, "/api/v1/link/tag", request, cancellationToken: cancellationToken);
    }

    public Task<Tag> GetTagAsync(int id, CancellationToken cancellationToken = default)
    {
        return SendAsync<Tag>(HttpMethod.Get, $"/api/v1/link/tag/{id}", cancellationToken: cancellationToken);
    }

    public Task<Tag> UpdateTagAsync(int id, UpdateTagRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTagUpdateRequest(request);
        return SendAsync<Tag>(HttpMethod.Put, $"/api/v1/link/tag/{id}", request, cancellationToken: cancellationToken);
    }

    public Task DeleteTagAsync(int id, CancellationToken cancellationToken = default)
    {
        return SendWithoutResultAsync(HttpMethod.Delete, $"/api/v1/link/tag/{id}", cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private Task<List<T>> SendListAsync<T>(HttpMethod method, string path, object? body = null, QueryStringBuilder? query = null, CancellationToken cancellationToken = default)
    {
        return SendListAsyncCore<T>(method, path, body, query, cancellationToken);
    }

    private async Task<List<T>> SendListAsyncCore<T>(HttpMethod method, string path, object? body, QueryStringBuilder? query, CancellationToken cancellationToken)
    {
        var response = await SendAsync<List<T>>(method, path, body, query, cancellationToken).ConfigureAwait(false);
        return response ?? new List<T>();
    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        QueryStringBuilder? query = null,
        CancellationToken cancellationToken = default,
        IEnumerable<string>? acceptMediaTypes = null)
    {
        using var request = CreateRequest(method, BuildPath(path, query), body, acceptMediaTypes ?? DefaultAcceptMediaTypes);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var responseBody = await ReadContentAsync(response).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new TlyApiException(response.StatusCode, responseBody);
        }

        return DeserializeContent<T>(responseBody);
    }

    private async Task SendWithoutResultAsync(
        HttpMethod method,
        string path,
        object? body = null,
        QueryStringBuilder? query = null,
        CancellationToken cancellationToken = default,
        IEnumerable<string>? acceptMediaTypes = null)
    {
        using var request = CreateRequest(method, BuildPath(path, query), body, acceptMediaTypes ?? DefaultAcceptMediaTypes);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var responseBody = await ReadContentAsync(response).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new TlyApiException(response.StatusCode, responseBody);
        }
    }

    private async Task<QrCodeResponse> SendQrCodeAsync(string path, QueryStringBuilder? query, string? output, CancellationToken cancellationToken)
    {
        using var request = CreateQrCodeRequest(BuildPath(path, query));
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var content = await ReadContentBytesAsync(response).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new TlyApiException(response.StatusCode, DecodeContent(content));
        }

        return CreateQrCodeResponse(content, response.Content?.Headers.ContentType?.MediaType, output);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body, IEnumerable<string> acceptMediaTypes)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        foreach (var acceptMediaType in acceptMediaTypes)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptMediaType));
        }

        if (!string.IsNullOrWhiteSpace(_userAgent))
        {
            request.Headers.UserAgent.ParseAdd(_userAgent);
        }

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, _serializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, JsonMediaType);
        }

        return request;
    }

    private HttpRequestMessage CreateQrCodeRequest(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        if (!string.IsNullOrWhiteSpace(_userAgent))
        {
            request.Headers.UserAgent.ParseAdd(_userAgent);
        }

        return request;
    }

    private T DeserializeContent<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return default!;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)content;
        }

        if (typeof(T) == typeof(bool) && TryParseBoolean(content, out var booleanValue))
        {
            return (T)(object)booleanValue;
        }

        var trimmed = content.Trim();

        if (trimmed.StartsWith("{", StringComparison.Ordinal) ||
            trimmed.StartsWith("[", StringComparison.Ordinal) ||
            trimmed.StartsWith("\"", StringComparison.Ordinal) ||
            trimmed.StartsWith("true", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("false", StringComparison.OrdinalIgnoreCase))
        {
            var value = JsonSerializer.Deserialize<T>(trimmed, _serializerOptions);
            return value!;
        }

        throw new JsonException($"The T.LY API returned content that could not be deserialized into {typeof(T).Name}.");
    }

    private static bool TryParseBoolean(string content, out bool value)
    {
        var trimmed = content.Trim().Trim('{', '}').Trim();
        return bool.TryParse(trimmed, out value);
    }

    private static async Task<string> ReadContentAsync(HttpResponseMessage response)
    {
        if (response.Content is null)
        {
            return string.Empty;
        }

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private static async Task<byte[]> ReadContentBytesAsync(HttpResponseMessage response)
    {
        if (response.Content is null)
        {
            return Array.Empty<byte>();
        }

        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }

    private static string BuildPath(string path, QueryStringBuilder? query)
    {
        var queryString = query?.ToString();
        return string.IsNullOrWhiteSpace(queryString) ? path : $"{path}?{queryString}";
    }

    private static string FormatDate(DateTimeOffset? value)
    {
        return value?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatDateTime(DateTimeOffset? value)
    {
        return value?.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static object CreateCreateShortLinkBody(CreateShortLinkRequest request, string format)
    {
        return new
        {
            include_qr_code = request.IncludeQrCode,
            long_url = request.LongUrl,
            format,
            short_id = request.ShortId,
            expire_at_time = request.ExpireAtTime,
            expire_at_datetime = request.ExpireAtDatetime.HasValue ? FormatDateTime(request.ExpireAtDatetime) : null,
            expire_at_views = request.ExpireAtViews,
            public_stats = request.PublicStats,
            password = request.Password,
            description = request.Description,
            tags = request.Tags,
            pixels = request.Pixels,
            meta = request.Meta,
            domain = request.Domain,
        };
    }

    private static QrCodeResponse CreateQrCodeResponse(byte[] content, string? contentType, string? output)
    {
        var response = new QrCodeResponse
        {
            ContentType = contentType,
            Content = content,
        };

        var expectsText = string.Equals(output, "base64", StringComparison.OrdinalIgnoreCase) || IsTextContentType(contentType);

        if (!expectsText || content.Length == 0)
        {
            return response;
        }

        var text = DecodeContent(content).Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return response;
        }

        if (TryExtractQrCodeBase64(text, out var base64))
        {
            return new QrCodeResponse
            {
                ContentType = contentType,
                Content = content,
                Text = text,
                Base64 = base64,
            };
        }

        return new QrCodeResponse
        {
            ContentType = contentType,
            Content = content,
            Text = text,
            Base64 = string.Equals(output, "base64", StringComparison.OrdinalIgnoreCase) ? text : null,
        };
    }

    private static bool TryExtractQrCodeBase64(string content, out string? base64)
    {
        base64 = null;

        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        if (content.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(content);

                if (document.RootElement.ValueKind == JsonValueKind.Object &&
                    document.RootElement.TryGetProperty("base64", out var base64Property) &&
                    base64Property.ValueKind == JsonValueKind.String)
                {
                    base64 = base64Property.GetString();
                    return !string.IsNullOrWhiteSpace(base64);
                }
            }
            catch (JsonException)
            {
                return false;
            }

            return false;
        }

        if (content.StartsWith("\"", StringComparison.Ordinal))
        {
            base64 = JsonSerializer.Deserialize<string>(content);
            return !string.IsNullOrWhiteSpace(base64);
        }

        return false;
    }

    private static string DecodeContent(byte[] content)
    {
        return content.Length == 0 ? string.Empty : Encoding.UTF8.GetString(content);
    }

    private static bool IsTextContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        var mediaType = contentType!;
        return mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
               mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }
    }

    private static void ValidateTagCreateRequest(CreateTagRequest? request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.Tag, nameof(request.Tag));
    }

    private static void ValidateTagUpdateRequest(UpdateTagRequest? request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.Tag, nameof(request.Tag));
    }

    private static void ValidatePixelCreateRequest(CreatePixelRequest? request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.Name, nameof(request.Name));
        EnsureNotNullOrWhiteSpace(request.PixelId, nameof(request.PixelId));
        EnsureNotNullOrWhiteSpace(request.PixelType, nameof(request.PixelType));
    }

    private static void ValidatePixelUpdateRequest(UpdatePixelRequest? request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.Name, nameof(request.Name));
        EnsureNotNullOrWhiteSpace(request.PixelId, nameof(request.PixelId));
        EnsureNotNullOrWhiteSpace(request.PixelType, nameof(request.PixelType));
    }

    private static void ValidateUtmPresetRequest(UpsertUtmPresetRequest? request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureNotNullOrWhiteSpace(request.Name, nameof(request.Name));
    }

    private sealed class QueryStringBuilder
    {
        private readonly List<KeyValuePair<string, string>> _values = new();

        public QueryStringBuilder Add(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _values.Add(new KeyValuePair<string, string>(key, value!));
            }

            return this;
        }

        public QueryStringBuilder Add(string key, int? value)
        {
            if (value.HasValue)
            {
                _values.Add(new KeyValuePair<string, string>(key, value.Value.ToString(CultureInfo.InvariantCulture)));
            }

            return this;
        }

        public QueryStringBuilder AddList(string key, IEnumerable<int>? values)
        {
            if (values is null)
            {
                return this;
            }

            foreach (var value in values)
            {
                _values.Add(new KeyValuePair<string, string>(key, value.ToString(CultureInfo.InvariantCulture)));
            }

            return this;
        }

        public override string ToString()
        {
            var encoded = new List<string>(_values.Count);

            foreach (var pair in _values)
            {
                encoded.Add($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
            }

            return string.Join("&", encoded);
        }
    }
}
