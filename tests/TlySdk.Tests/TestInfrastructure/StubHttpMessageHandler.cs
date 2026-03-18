using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TlySdk.Tests.TestInfrastructure;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();

    public List<RecordedRequest> Requests { get; } = new();

    public void Enqueue(HttpResponseMessage response)
    {
        _responses.Enqueue(() => response);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(await RecordedRequest.CreateAsync(request).ConfigureAwait(false));

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No response has been queued for the test handler.");
        }

        return _responses.Dequeue().Invoke();
    }
}
