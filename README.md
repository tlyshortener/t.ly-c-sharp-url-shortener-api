# T.LY URL Shortener SDK for .NET

NuGet-ready C# SDK for the T.LY URL Shortener API. The package wraps the endpoints in the provided OpenAPI spec and includes support for:

- Short link create, retrieve, update, delete, expand, bulk create, bulk update, and listing
- Link analytics and OneLink analytics
- OneLink listing
- QR code retrieval and updates
- Tag CRUD
- Pixel CRUD
- UTM preset CRUD

## Installation

Package page: [nuget.org/packages/Tly.UrlShortener](https://www.nuget.org/packages/Tly.UrlShortener/)

```bash
dotnet add package Tly.UrlShortener
```

## Quick Start

```csharp
using Tly;

var client = new TlyClient("YOUR_TLY_API_KEY");

var shortLink = await client.CreateShortLinkAsync(new CreateShortLinkRequest
{
    LongUrl = "https://example.com/products/spring-launch",
    Description = "Spring launch landing page",
    PublicStats = true,
});

Console.WriteLine(shortLink.ShortUrl);
```

## Register For T.LY

1. Visit [t.ly/register](https://t.ly/register).
2. Create your account and complete any email verification steps shown by T.LY.
3. Sign in to your new account.

## Get Your T.LY API Key

1. Sign in to your account.
2. Open [t.ly/settings#/api](https://t.ly/settings#/api).
3. If you are not already authenticated, T.LY redirects that URL to the login page first. After signing in, go back to the API settings page.
4. Generate or copy your API token and pass it to `TlyClient`.

```csharp
var client = new TlyClient("YOUR_TLY_API_KEY");
```

## Common Examples

### Expand a short link

```csharp
var expanded = await client.ExpandShortLinkAsync(new ExpandShortLinkRequest
{
    ShortUrl = "https://t.ly/demo",
});

Console.WriteLine(expanded.LongUrl);
```

### List short links

```csharp
var links = await client.ListShortLinksAsync(new ListShortLinksRequest
{
    Search = "spring",
    TagIds = new() { 1, 2 },
});

foreach (var link in links.Data)
{
    Console.WriteLine($"{link.ShortUrl} -> {link.LongUrl}");
}
```

### Fetch link stats

```csharp
var stats = await client.GetLinkStatsAsync(
    "https://t.ly/demo",
    startDate: DateTimeOffset.UtcNow.AddDays(-7),
    endDate: DateTimeOffset.UtcNow);

Console.WriteLine($"Clicks: {stats.Clicks}");
```

### Retrieve a QR code

```csharp
var qrCode = await client.GetQrCodeAsync("https://t.ly/demo", output: "base64");

Console.WriteLine(qrCode.Base64);
```

### Create a plain-text short link

```csharp
var shortUrl = await client.CreateShortLinkTextAsync(new CreateShortLinkRequest
{
    LongUrl = "https://example.com/plain-text-response",
});

Console.WriteLine(shortUrl);
```

### Create a tag

```csharp
var tag = await client.CreateTagAsync(new CreateTagRequest
{
    Tag = "spring-launch",
});
```

## Packaging

Create the NuGet package:

```bash
dotnet pack src/TlySdk/TlySdk.csproj -c Release -o ./artifacts
```

Push the package to NuGet:

```bash
dotnet nuget push ./artifacts/Tly.UrlShortener.1.0.1.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

More detail is available in [docs/NUGET_PUBLISH.md](https://github.com/tlyshortener/t.ly-c-sharp-url-shortener-api/blob/main/docs/NUGET_PUBLISH.md) and [docs/ACCOUNT_SETUP.md](https://github.com/tlyshortener/t.ly-c-sharp-url-shortener-api/blob/main/docs/ACCOUNT_SETUP.md).

## Testing

```bash
dotnet test
```

## Notes

- The source OpenAPI file is checked into [spec/openapi.yaml](https://github.com/tlyshortener/t.ly-c-sharp-url-shortener-api/blob/main/spec/openapi.yaml).
- A few response shapes in the source spec are inconsistent or under-specified, so the SDK keeps typed models where the contract is clear and uses extension data for forward compatibility.
- Full publishing steps: [docs/NUGET_PUBLISH.md](https://github.com/tlyshortener/t.ly-c-sharp-url-shortener-api/blob/main/docs/NUGET_PUBLISH.md)
- Account and API key setup: [docs/ACCOUNT_SETUP.md](https://github.com/tlyshortener/t.ly-c-sharp-url-shortener-api/blob/main/docs/ACCOUNT_SETUP.md)

## License

[MIT](https://github.com/tlyshortener/t.ly-c-sharp-url-shortener-api/blob/main/LICENSE)
