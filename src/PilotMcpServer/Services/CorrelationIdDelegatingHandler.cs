using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.Services;

/// <summary>
/// Ensures every outgoing domain Pilot API request carries an <c>X-Correlation-Id</c> header holding a valid
/// GUID, as required by the Pilot API OpenAPI contract. A caller-supplied value is kept when it parses as a
/// GUID; a missing, empty, duplicated or malformed value is replaced with the current tool call's
/// <see cref="Models.RequestSettings.CorrelationId"/>, or a new GUID outside of a tool call. The System endpoints
/// (<c>/about</c> and <c>/healthcheck</c>) do not define the header, so it is only sent to them during a tool call,
/// which keeps the API's side of that tool call under the same correlation ID.
/// </summary>
public sealed class CorrelationIdDelegatingHandler(IRequestSettingsAccessor? requestSettingsAccessor = null) : DelegatingHandler
{
	public const string HeaderName = "X-Correlation-Id";

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if ((!IsSystemRequest(request) || requestSettingsAccessor?.Current is not null) && !HasValidCorrelationId(request))
		{
			request.Headers.Remove(HeaderName);
			request.Headers.Add(HeaderName, ResolveCorrelationId());
		}

		return base.SendAsync(request, cancellationToken);
	}

	private static bool IsSystemRequest(HttpRequestMessage request)
	{
		var path = request.RequestUri?.AbsolutePath ?? string.Empty;
		return path.EndsWith("/about", StringComparison.OrdinalIgnoreCase) ||
			path.EndsWith("/healthcheck", StringComparison.OrdinalIgnoreCase);
	}

	private string ResolveCorrelationId()
		=> requestSettingsAccessor?.Current?.CorrelationId is { } correlationId && Guid.TryParse(correlationId, out _)
			? correlationId
			: Guid.NewGuid().ToString();

	private static bool HasValidCorrelationId(HttpRequestMessage request)
		=> request.Headers.TryGetValues(HeaderName, out var values) &&
			values.ToArray() is [var value] &&
			Guid.TryParse(value, out _);
}
