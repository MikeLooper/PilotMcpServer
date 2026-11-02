using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.Services;

/// <summary>
/// Logs the start and end of every Pilot API call: this server has no database of its own, so these calls are its
/// data access. Each entry names the API, the HTTP method and path, and the correlation ID actually sent in
/// <c>X-Correlation-Id</c>, plus the current <see cref="Models.RequestSettings.RequestId"/> and
/// <see cref="Models.RequestSettings.CallerName"/>. The end entry adds the status code and elapsed time; a call that
/// gets no response (e.g. a connection failure or timeout) is logged as failed, with its exception, and rethrown.
/// Register it after <see cref="CorrelationIdDelegatingHandler"/>, so the header is already set.
/// </summary>
public sealed partial class PilotApiLoggingDelegatingHandler(
	ILogger<PilotApiLoggingDelegatingHandler> logger,
	IRequestSettingsAccessor? requestSettingsAccessor = null) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var method = request.Method.Method;
		var path = request.RequestUri?.PathAndQuery ?? string.Empty;
		var apiName = GetApiName(request.RequestUri);
		var correlationId = request.Headers.TryGetValues(CorrelationIdDelegatingHandler.HeaderName, out var values)
			? string.Join(",", values)
			: null;
		var requestSettings = requestSettingsAccessor?.Current;

		LogCallStarted(logger, method, path, apiName, correlationId, requestSettings?.RequestId, requestSettings?.CallerName);
		var startTimestamp = Stopwatch.GetTimestamp();
		try
		{
			var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
			LogCallCompleted(logger, method, path, apiName, (int)response.StatusCode, GetElapsedMilliseconds(startTimestamp),
				correlationId, requestSettings?.RequestId, requestSettings?.CallerName);
			return response;
		}
		catch (Exception ex)
		{
			LogCallFailed(logger, ex, method, path, apiName, GetElapsedMilliseconds(startTimestamp),
				correlationId, requestSettings?.RequestId, requestSettings?.CallerName);
			throw;
		}
	}

	private static string GetApiName(Uri? uri)
		=> uri is null
			? string.Empty
			: PilotApiCatalog.All.FirstOrDefault(api =>
				string.Equals(api.Host, uri.Host, StringComparison.OrdinalIgnoreCase) && api.Port == uri.Port)?.Name
				?? uri.Authority;

	private static double GetElapsedMilliseconds(long startTimestamp)
		=> Math.Round(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds, 1);

	[LoggerMessage(EventId = 2000, Level = LogLevel.Information,
		Message = "Pilot API call {HttpMethod} {PilotApiPath} to {PilotApiName} started. CorrelationId: {CorrelationId}, McpRequestId: {McpRequestId}, CallerName: {CallerName}")]
	private static partial void LogCallStarted(ILogger logger, string httpMethod, string pilotApiPath, string pilotApiName,
		string? correlationId, string? mcpRequestId, string? callerName);

	[LoggerMessage(EventId = 2001, Level = LogLevel.Information,
		Message = "Pilot API call {HttpMethod} {PilotApiPath} to {PilotApiName} completed with status {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}, McpRequestId: {McpRequestId}, CallerName: {CallerName}")]
	private static partial void LogCallCompleted(ILogger logger, string httpMethod, string pilotApiPath, string pilotApiName, int statusCode,
		double elapsedMilliseconds, string? correlationId, string? mcpRequestId, string? callerName);

	// Warning rather than Error: some callers expect and handle failures, e.g. list_apis reports an unreachable API.
	[LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
		Message = "Pilot API call {HttpMethod} {PilotApiPath} to {PilotApiName} failed after {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}, McpRequestId: {McpRequestId}, CallerName: {CallerName}")]
	private static partial void LogCallFailed(ILogger logger, Exception exception, string httpMethod, string pilotApiPath, string pilotApiName,
		double elapsedMilliseconds, string? correlationId, string? mcpRequestId, string? callerName);
}
