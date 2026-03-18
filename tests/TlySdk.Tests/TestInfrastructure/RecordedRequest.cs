using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TlySdk.Tests.TestInfrastructure;

internal sealed class RecordedRequest
{
    public required HttpMethod Method { get; init; }

    public required string PathAndQuery { get; init; }

    public required Dictionary<string, string> Headers { get; init; }

    public string? Body { get; init; }

    public static async Task<RecordedRequest> CreateAsync(HttpRequestMessage request)
    {
        return new RecordedRequest
        {
            Method = request.Method,
            PathAndQuery = request.RequestUri?.PathAndQuery ?? string.Empty,
            Headers = request.Headers.ToDictionary(
                header => header.Key,
                header => string.Join(",", header.Value)),
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync().ConfigureAwait(false),
        };
    }
}
