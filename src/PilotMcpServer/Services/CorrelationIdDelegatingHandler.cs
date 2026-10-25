namespace PilotMcpServer.Services;

/// <summary>
/// Ensures every outgoing domain Pilot API request carries an <c>X-Correlation-Id</c> header holding a valid
/// GUID, as required by the Pilot API OpenAPI contract. A caller-supplied value is kept when it parses as a
/// GUID; a missing, empty, duplicated or malformed value is replaced with a new GUID. Skips the System
/// endpoints (<c>/about</c> and <c>/healthcheck</c>), which do not define the header.
/// </summary>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
	public const string HeaderName = "X-Correlation-Id";

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var path = request.RequestUri?.AbsolutePath ?? string.Empty;
		if (!path.EndsWith("/about", StringComparison.OrdinalIgnoreCase) &&
			!path.EndsWith("/healthcheck", StringComparison.OrdinalIgnoreCase) &&
			!HasValidCorrelationId(request))
		{
			request.Headers.Remove(HeaderName);
			request.Headers.Add(HeaderName, Guid.NewGuid().ToString());
		}

		return base.SendAsync(request, cancellationToken);
	}

	private static bool HasValidCorrelationId(HttpRequestMessage request)
		=> request.Headers.TryGetValues(HeaderName, out var values) &&
			values.ToArray() is [var value] &&
			Guid.TryParse(value, out _);
}
