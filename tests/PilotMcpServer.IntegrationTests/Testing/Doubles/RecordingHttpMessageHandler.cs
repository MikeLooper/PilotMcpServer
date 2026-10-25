using System.Net;

namespace PilotMcpServer.IntegrationTests.Testing.Doubles;

/// <summary>Terminal handler that records the last request it saw and answers 200 OK without any network access.</summary>
public sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request });
    }
}
