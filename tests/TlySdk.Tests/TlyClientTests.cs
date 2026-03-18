using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tly;
using TlySdk.Tests.TestInfrastructure;

namespace TlySdk.Tests;

public sealed class TlyClientTests
{
    [Fact]
    public async Task ShortLinkCrudFlow_UsesExpectedEndpoints()
    {
        var handler = CreateHandler(
            TextResponse("""
            {
              "short_url":"https://t.ly/abc",
              "long_url":"https://example.com",
              "short_id":"abc",
              "domain":"https://t.ly/",
              "expire_at_datetime":"2035-01-17 15:00:00"
            }
            """),
            TextResponse("""
            {
              "short_url":"https://t.ly/abc",
              "long_url":"https://example.com",
              "qr_code_url":"https://api.t.ly/api/v1/link/qr-code?short_url=https://t.ly/abc"
            }
            """),
            TextResponse("""
            {
              "short_url":"https://t.ly/abc",
              "long_url":"https://example.org",
              "description":"Updated"
            }
            """),
            JsonResponse("{}", HttpStatusCode.Accepted));

        using var client = CreateClient(handler);

        var created = await client.CreateShortLinkAsync(new CreateShortLinkRequest
        {
            IncludeQrCode = true,
            LongUrl = "https://example.com",
            Description = "Example",
            PublicStats = true,
            ExpireAtDatetime = new DateTimeOffset(2026, 3, 18, 14, 30, 0, TimeSpan.Zero),
        });

        var fetched = await client.GetShortLinkAsync("https://t.ly/abc");
        var updated = await client.UpdateShortLinkAsync(new UpdateShortLinkRequest
        {
            ShortUrl = "https://t.ly/abc",
            LongUrl = "https://example.org",
            Description = "Updated",
        });

        await client.DeleteShortLinkAsync("https://t.ly/abc");

        Assert.Equal("https://t.ly/abc", created.ShortUrl);
        Assert.Equal("https://example.com", fetched.LongUrl);
        Assert.Equal("Updated", updated.Description);
        Assert.Equal(new DateTimeOffset(2035, 1, 17, 15, 0, 0, TimeSpan.Zero), created.ExpireAtDatetime);
        Assert.Collection(
            handler.Requests,
            request =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("/api/v1/link/shorten", request.PathAndQuery);
                Assert.Contains("\"include_qr_code\":true", request.Body);
                Assert.Contains("\"long_url\":\"https://example.com\"", request.Body);
                Assert.Contains("\"public_stats\":true", request.Body);
                Assert.Contains("\"expire_at_datetime\":\"2026-03-18 14:30:00\"", request.Body);
                Assert.Equal("Bearer test-api-key", request.Headers["Authorization"]);
            },
            request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("/api/v1/link?short_url=https%3A%2F%2Ft.ly%2Fabc", request.PathAndQuery);
            },
            request =>
            {
                Assert.Equal(HttpMethod.Put, request.Method);
                Assert.Equal("/api/v1/link", request.PathAndQuery);
                Assert.Contains("\"description\":\"Updated\"", request.Body);
            },
            request =>
            {
                Assert.Equal(HttpMethod.Delete, request.Method);
                Assert.Equal("/api/v1/link", request.PathAndQuery);
                Assert.Equal("{\"short_url\":\"https://t.ly/abc\"}", request.Body);
            });
    }

    [Fact]
    public async Task ExpandShortLinkAsync_ParsesResponse()
    {
        var handler = CreateHandler(JsonResponse("""
        {
          "long_url":"https://example.com/protected",
          "expired":false
        }
        """));

        using var client = CreateClient(handler);

        var expanded = await client.ExpandShortLinkAsync(new ExpandShortLinkRequest
        {
            ShortUrl = "https://t.ly/secure",
            IsQrCode = true,
            Password = "secret",
        });

        Assert.False(expanded.Expired);
        Assert.Equal("https://example.com/protected", expanded.LongUrl);
        Assert.Equal("/api/v1/link/expand", handler.Requests.Single().PathAndQuery);
        Assert.Contains("\"is_qr_code\":true", handler.Requests.Single().Body);
        Assert.Contains("\"password\":\"secret\"", handler.Requests.Single().Body);
    }

    [Fact]
    public async Task ListShortLinksAsync_SerializesFilters()
    {
        var handler = CreateHandler(TextResponse("""
        {
          "current_page":1,
          "data":[
            {
              "short_url":"https://t.ly/1234",
              "long_url":"https://google.com/"
            }
          ],
          "total":1
        }
        """));

        using var client = CreateClient(handler);

        var response = await client.ListShortLinksAsync(new ListShortLinksRequest
        {
            Search = "google",
            TagIds = new() { 1, 2 },
            PixelIds = new() { 10 },
            Domains = new() { 22 },
            UserIds = new() { 8 },
            Limit = 100,
            Page = 3,
            StartDate = new DateTimeOffset(2026, 3, 1, 10, 30, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 3, 2, 12, 45, 0, TimeSpan.Zero),
        });

        Assert.Single(response.Data);
        var path = handler.Requests.Single().PathAndQuery;
        Assert.Contains("search=google", path);
        Assert.Contains("tag_ids=1", path);
        Assert.Contains("tag_ids=2", path);
        Assert.Contains("pixel_ids=10", path);
        Assert.Contains("domains=22", path);
        Assert.Contains("user_ids=8", path);
        Assert.Contains("limit=100", path);
        Assert.Contains("page=3", path);
        Assert.Contains("start_date=2026-03-01%2010%3A30%3A00", path);
        Assert.Contains("end_date=2026-03-02%2012%3A45%3A00", path);
    }

    [Fact]
    public async Task BulkOperations_ParseBooleanResponses()
    {
        var handler = CreateHandler(
            TextResponse("true"),
            TextResponse("true"));

        using var client = CreateClient(handler);

        var created = await client.BulkCreateShortLinksAsync(new BulkCreateShortLinksRequest
        {
            Domain = "https://t.ly/",
            Links = new()
            {
                new BulkCreateShortLinkItem { LongUrl = "https://example.com", ShortId = "first" },
            },
        });

        var updated = await client.BulkUpdateShortLinksAsync(new BulkUpdateShortLinksRequest
        {
            Links = new()
            {
                new BulkUpdateShortLinkItem { ShortUrl = "https://t.ly/first", LongUrl = "https://example.org" },
            },
        });

        Assert.True(created);
        Assert.True(updated);
        Assert.Equal("/api/v1/link/bulk", handler.Requests[0].PathAndQuery);
        Assert.Equal("/api/v1/link/bulk/update", handler.Requests[1].PathAndQuery);
    }

    [Fact]
    public async Task StatsEndpoints_ParseResponses()
    {
        var handler = CreateHandler(
            JsonResponse("""
            {
              "clicks":5,
              "unique_clicks":4,
              "data":{
                "short_url":"https://t.ly/c55j",
                "long_url":"https://weatherextension.com/"
              }
            }
            """),
            JsonResponse("""
            {
              "clicks":120,
              "unique_clicks":98,
              "total_qr_scans":6,
              "link_clicks":[
                {
                  "link_id":55,
                  "title":"Website",
                  "url":"https://example.com",
                  "total":70,
                  "unique_total":60
                }
              ],
              "data":{
                "short_url":"https://t.ly/one",
                "title":"My OneLink"
              }
            }
            """),
            JsonResponse("{}", HttpStatusCode.Accepted));

        using var client = CreateClient(handler);

        var linkStats = await client.GetLinkStatsAsync(
            "https://t.ly/c55j",
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 7, 0, 0, 0, TimeSpan.Zero));

        var oneLinkStats = await client.GetOneLinkStatsAsync("https://t.ly/one");
        await client.DeleteOneLinkStatsAsync("https://t.ly/one");

        Assert.Equal(5, linkStats.Clicks);
        Assert.Equal("https://weatherextension.com/", linkStats.Data?.LongUrl);
        Assert.Single(oneLinkStats.LinkClicks);
        Assert.Equal("My OneLink", oneLinkStats.Data?.Title);
        Assert.Contains("start_date=2026-03-01", handler.Requests[0].PathAndQuery);
        Assert.Contains("end_date=2026-03-07", handler.Requests[0].PathAndQuery);
        Assert.Equal("/api/v1/onelink/stats?short_url=https%3A%2F%2Ft.ly%2Fone", handler.Requests[1].PathAndQuery);
        Assert.Equal("/api/v1/onelink/stat", handler.Requests[2].PathAndQuery);
    }

    [Fact]
    public async Task OneLinkListAsync_ParsesPagedResults()
    {
        var handler = CreateHandler(JsonResponse("""
        {
          "current_page":1,
          "data":[
            {
              "id":123,
              "short_id":"one",
              "short_url":"https://t.ly/one",
              "title":"My OneLink",
              "links":[
                {
                  "id":55,
                  "title":"Website",
                  "url":"https://example.com",
                  "type":"link"
                }
              ]
            }
          ],
          "per_page":30,
          "total":1
        }
        """));

        using var client = CreateClient(handler);

        var page = await client.ListOneLinksAsync(1);

        Assert.Single(page.Data);
        Assert.Equal("My OneLink", page.Data[0].Title);
        Assert.Single(page.Data[0].Links);
        Assert.Equal("/api/v1/onelink/list?page=1", handler.Requests.Single().PathAndQuery);
    }

    [Fact]
    public async Task PixelCrudFlow_UsesExpectedEndpoints()
    {
        var handler = CreateHandler(
            JsonResponse("""
            {"id":12345,"name":"GTM Pixel","pixel_id":"GTM-xxxx","pixel_type":"googleTagManager"}
            """),
            JsonResponse("""
            [{"id":12345,"name":"GTM Pixel","pixel_id":"GTM-xxxx","pixel_type":"googleTagManager"}]
            """),
            JsonResponse("""
            {"id":12345,"name":"GTM Pixel","pixel_id":"GTM-xxxx","pixel_type":"googleTagManager"}
            """),
            JsonResponse("""
            {"id":12345,"name":"Updated Pixel","pixel_id":"GTM-yyyy","pixel_type":"googleTagManager"}
            """),
            JsonResponse("{}", HttpStatusCode.Accepted));

        using var client = CreateClient(handler);

        var created = await client.CreatePixelAsync(new CreatePixelRequest
        {
            Name = "GTM Pixel",
            PixelId = "GTM-xxxx",
            PixelType = "googleTagManager",
        });
        var list = await client.ListPixelsAsync();
        var fetched = await client.GetPixelAsync(12345);
        var updated = await client.UpdatePixelAsync(12345, new UpdatePixelRequest
        {
            Id = 12345,
            Name = "Updated Pixel",
            PixelId = "GTM-yyyy",
            PixelType = "googleTagManager",
        });
        await client.DeletePixelAsync(12345);

        Assert.Equal("GTM Pixel", created.Name);
        Assert.Single(list);
        Assert.Equal(12345, fetched.Id);
        Assert.Equal("Updated Pixel", updated.Name);
        Assert.Equal("/api/v1/link/pixel", handler.Requests[0].PathAndQuery);
        Assert.Equal("/api/v1/link/pixel", handler.Requests[1].PathAndQuery);
        Assert.Equal("/api/v1/link/pixel/12345", handler.Requests[2].PathAndQuery);
        Assert.Equal("/api/v1/link/pixel/12345", handler.Requests[3].PathAndQuery);
        Assert.Equal("/api/v1/link/pixel/12345", handler.Requests[4].PathAndQuery);
        Assert.DoesNotContain("\"id\":", handler.Requests[3].Body);
    }

    [Fact]
    public async Task TagCrudFlow_UsesExpectedEndpoints()
    {
        var handler = CreateHandler(
            JsonResponse("""
            {"id":12345,"tag":"fall2024"}
            """),
            JsonResponse("""
            [{"id":12345,"tag":"fall2024"}]
            """),
            JsonResponse("""
            {"id":12345,"tag":"fall2024"}
            """),
            JsonResponse("""
            {"id":12345,"tag":"fall2025"}
            """),
            JsonResponse("{}", HttpStatusCode.Accepted));

        using var client = CreateClient(handler);

        var created = await client.CreateTagAsync(new CreateTagRequest { Tag = "fall2024" });
        var list = await client.ListTagsAsync();
        var fetched = await client.GetTagAsync(12345);
        var updated = await client.UpdateTagAsync(12345, new UpdateTagRequest { Tag = "fall2025" });
        await client.DeleteTagAsync(12345);

        Assert.Equal("fall2024", created.Value);
        Assert.Single(list);
        Assert.Equal(12345, fetched.Id);
        Assert.Equal("fall2025", updated.Value);
        Assert.Equal("/api/v1/link/tag", handler.Requests[0].PathAndQuery);
        Assert.Equal("/api/v1/link/tag", handler.Requests[1].PathAndQuery);
        Assert.Equal("/api/v1/link/tag/12345", handler.Requests[2].PathAndQuery);
        Assert.Equal("/api/v1/link/tag/12345", handler.Requests[3].PathAndQuery);
        Assert.Equal("/api/v1/link/tag/12345", handler.Requests[4].PathAndQuery);
    }

    [Fact]
    public async Task UtmPresetCrudFlow_UsesExpectedEndpoints()
    {
        var handler = CreateHandler(
            JsonResponse("""
            {"id":7,"name":"Newsletter Launch","source":"newsletter","medium":"email"}
            """),
            JsonResponse("""
            [{"id":7,"name":"Newsletter Launch","source":"newsletter","medium":"email"}]
            """),
            JsonResponse("""
            {"id":7,"name":"Newsletter Launch","source":"newsletter","medium":"email"}
            """),
            JsonResponse("""
            {"id":7,"name":"Newsletter Launch V2","source":"newsletter","medium":"email"}
            """),
            JsonResponse("{}", HttpStatusCode.Accepted));

        using var client = CreateClient(handler);

        var created = await client.CreateUtmPresetAsync(new UpsertUtmPresetRequest
        {
            Name = "Newsletter Launch",
            Source = "newsletter",
            Medium = "email",
        });
        var list = await client.ListUtmPresetsAsync();
        var fetched = await client.GetUtmPresetAsync(7);
        var updated = await client.UpdateUtmPresetAsync(7, new UpsertUtmPresetRequest
        {
            Name = "Newsletter Launch V2",
            Source = "newsletter",
            Medium = "email",
        });
        await client.DeleteUtmPresetAsync(7);

        Assert.Equal("Newsletter Launch", created.Name);
        Assert.Single(list);
        Assert.Equal(7, fetched.Id);
        Assert.Equal("Newsletter Launch V2", updated.Name);
        Assert.Equal("/api/v1/link/utm-preset", handler.Requests[0].PathAndQuery);
        Assert.Equal("/api/v1/link/utm-preset", handler.Requests[1].PathAndQuery);
        Assert.Equal("/api/v1/link/utm-preset/7", handler.Requests[2].PathAndQuery);
        Assert.Equal("/api/v1/link/utm-preset/7", handler.Requests[3].PathAndQuery);
        Assert.Equal("/api/v1/link/utm-preset/7", handler.Requests[4].PathAndQuery);
    }

    [Fact]
    public async Task QrCodeEndpoints_UseExpectedPayloads()
    {
        var handler = CreateHandler(
            TextResponse("data:image/png;base64,iVBORw0KGgoA..."),
            JsonResponse("""
            {
              "id":123,
              "short_url":"https://t.ly/c55j",
              "qr_code_options":{
                "background_color":"#ffffff",
                "dots_color":"#000000"
              }
            }
            """));

        using var client = CreateClient(handler);

        var base64 = await client.GetQrCodeAsync("https://t.ly/c55j", output: "base64", format: "png");
        var updated = await client.UpdateQrCodeAsync(new UpdateQrCodeRequest
        {
            ShortUrl = "https://t.ly/c55j",
            BackgroundColor = "#ffffff",
            DotsColor = "#000000",
        });

        Assert.StartsWith("data:image/png;base64", base64.Base64);
        Assert.Equal("data:image/png;base64,iVBORw0KGgoA...", base64.Text);
        Assert.Equal("text/plain", base64.ContentType);
        Assert.Equal("#ffffff", updated.QrCodeOptions?.BackgroundColor);
        Assert.Equal("/api/v1/link/qr-code?short_url=https%3A%2F%2Ft.ly%2Fc55j&output=base64", handler.Requests[0].PathAndQuery);
        Assert.Contains("\"background_color\":\"#ffffff\"", handler.Requests[1].Body);
    }

    [Fact]
    public async Task QrCodeEndpoint_ReturnsImageBytesByDefault()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        var handler = CreateHandler(response);
        using var client = CreateClient(handler);

        var qrCode = await client.GetQrCodeAsync("https://t.ly/c55j");

        Assert.Equal("image/png", qrCode.ContentType);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, qrCode.Content);
        Assert.Null(qrCode.Base64);
        Assert.Equal("/api/v1/link/qr-code?short_url=https%3A%2F%2Ft.ly%2Fc55j", handler.Requests.Single().PathAndQuery);
    }

    [Fact]
    public async Task CreateShortLinkTextAsync_ReturnsPlainTextShortUrl()
    {
        var handler = CreateHandler(TextResponse("https://t.ly/plain"));
        using var client = CreateClient(handler);

        var shortUrl = await client.CreateShortLinkTextAsync(new CreateShortLinkRequest
        {
            LongUrl = "https://example.com/plain-text-response",
        });

        Assert.Equal("https://t.ly/plain", shortUrl);
        Assert.Contains("\"format\":\"text\"", handler.Requests.Single().Body);
    }

    [Fact]
    public async Task NonSuccessResponses_ThrowTlyApiException()
    {
        var handler = CreateHandler(JsonResponse("""
        {
          "message":"The given data was invalid."
        }
        """, HttpStatusCode.UnprocessableEntity));

        using var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<TlyApiException>(() => client.CreateTagAsync(new CreateTagRequest { Tag = "bad" }));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("invalid", exception.ResponseBody, StringComparison.OrdinalIgnoreCase);
    }

    private static TlyClient CreateClient(StubHttpMessageHandler handler)
    {
        return new TlyClient(
            "test-api-key",
            new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.t.ly/"),
            });
    }

    private static StubHttpMessageHandler CreateHandler(params HttpResponseMessage[] responses)
    {
        var handler = new StubHttpMessageHandler();

        foreach (var response in responses)
        {
            handler.Enqueue(response);
        }

        return handler;
    }

    private static HttpResponseMessage JsonResponse(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }

    private static HttpResponseMessage TextResponse(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/plain"),
        };
    }
}
