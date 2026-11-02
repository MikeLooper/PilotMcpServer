using PilotMcpServer.Configuration;

namespace PilotMcpServer.Services;

/// <summary>
/// Adds the optional <c>X-Api-Version</c> and <c>X-Caller-Name</c> headers that the Pilot API OpenAPI contract
/// defines on every endpoint, including the System endpoints (<c>/about</c> and <c>/healthcheck</c>).
/// <c>X-Api-Version</c> carries the configured <see cref="CatalogDocument.ApiVersion"/> and <c>X-Caller-Name</c>
/// carries <see cref="CallerName"/>, so the Pilot API's logs show which application made the call. A value already
/// on the request is kept.
/// </summary>
public sealed class PilotApiHeadersDelegatingHandler : DelegatingHandler
{
	public const string ApiVersionHeaderName = "X-Api-Version";

	public const string CallerNameHeaderName = "X-Caller-Name";

	/// <summary>The name this server sends to the Pilot API as <c>X-Caller-Name</c>.</summary>
	public const string CallerName = "PilotMcpServer";

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (!request.Headers.Contains(ApiVersionHeaderName) && !string.IsNullOrWhiteSpace(PilotApiCatalog.Catalog.ApiVersion))
		{
			request.Headers.Add(ApiVersionHeaderName, PilotApiCatalog.Catalog.ApiVersion);
		}

		if (!request.Headers.Contains(CallerNameHeaderName))
		{
			request.Headers.Add(CallerNameHeaderName, CallerName);
		}

		return base.SendAsync(request, cancellationToken);
	}
}
