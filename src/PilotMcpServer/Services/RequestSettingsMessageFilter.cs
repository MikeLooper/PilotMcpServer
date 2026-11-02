using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;
using PilotMcpServer.OpenTelemetry;

namespace PilotMcpServer.Services;

/// <summary>
/// Incoming MCP message filter that gives each JSON-RPC request (e.g. a <c>tools/call</c>) its own
/// <see cref="RequestSettings"/>, and so its own correlation ID, for the whole time the request is handled, tags the
/// current span with those settings, and logs the start and end of the request. A message filter is used rather than
/// a call-tool filter because the SDK writes its "tool completed" / "tool threw" log entries outside the call-tool
/// filter pipeline.
/// The caller can supply the correlation ID as a GUID in <c>params._meta.correlationId</c> on any request, or in
/// <c>params.arguments.correlationId</c> on a tool call. <c>_meta</c> wins if both hold a GUID; if neither does, a
/// new one is generated. The request ID is the JSON-RPC <c>id</c>, and the caller name is the MCP client's
/// <c>clientInfo.name</c>. Notifications pass through unchanged. <see cref="CreateOutgoing"/> returns the IDs on the
/// response's <c>result._meta</c>, so a caller can look the request up, or reuse a generated ID for its next call.
/// </summary>
public static partial class RequestSettingsMessageFilter
{
	public const string CorrelationIdKey = "correlationId";

	public const string TraceIdKey = "traceId";

	/// <summary>The logger category of the request start and end entries.</summary>
	public const string LoggerCategory = "PilotMcpServer.Services.RequestSettingsMessageFilter";

	public static McpMessageHandler Create(McpMessageHandler next)
		=> async (context, cancellationToken) =>
		{
			if (context.JsonRpcMessage is not JsonRpcRequest request)
			{
				await next(context, cancellationToken).ConfigureAwait(false);
				return;
			}

			var accessor = context.Services?.GetService<IRequestSettingsAccessor>() ?? new RequestSettingsAccessor();
			var logger = context.Services?.GetService<ILoggerFactory>()?.CreateLogger(LoggerCategory) ?? NullLogger.Instance;
			var previous = accessor.Current;
			var requestSettings = CreateRequestSettings(request, context.Server);
			var operation = GetOperationName(request);

			accessor.Current = requestSettings;
			Activity.Current?.SetRequestSettingsTags(requestSettings);

			LogRequestStarted(logger, operation, requestSettings.CorrelationId, requestSettings.RequestId, requestSettings.CallerName);
			var startTimestamp = Stopwatch.GetTimestamp();
			try
			{
				await next(context, cancellationToken).ConfigureAwait(false);
				LogRequestCompleted(logger, operation, GetElapsedMilliseconds(startTimestamp), requestSettings.CorrelationId, requestSettings.RequestId, requestSettings.CallerName);
			}
			catch (Exception ex)
			{
				LogRequestFailed(logger, ex, operation, GetElapsedMilliseconds(startTimestamp), requestSettings.CorrelationId, requestSettings.RequestId, requestSettings.CallerName);
				throw;
			}
			finally
			{
				accessor.Current = previous;
			}
		};

	public static McpMessageHandler CreateOutgoing(McpMessageHandler next)
		=> (context, cancellationToken) =>
		{
			var requestSettings = context.Services?.GetService<IRequestSettingsAccessor>()?.Current;
			if (requestSettings is not null && context.JsonRpcMessage is JsonRpcResponse { Result: JsonObject result })
			{
				if (result["_meta"] is not JsonObject meta)
				{
					meta = [];
					result["_meta"] = meta;
				}

				meta[CorrelationIdKey] = requestSettings.CorrelationId;
				if (Activity.Current is { } activity)
				{
					meta[TraceIdKey] = activity.TraceId.ToString();
				}
			}

			return next(context, cancellationToken);
		};

	private static RequestSettings CreateRequestSettings(JsonRpcRequest request, McpServer? server)
	{
		var parameters = request.Params as JsonObject;
		var guid = ReadCorrelationId(parameters?["_meta"])
			?? (request.Method == RequestMethods.ToolsCall ? ReadCorrelationId(parameters?["arguments"]) : null);

		return new RequestSettings
		{
			CorrelationId = guid?.ToString() ?? Guid.NewGuid().ToString(),
			RequestId = request.Id.Id is null ? null : request.Id.ToString(),
			CallerName = ReadCallerName(request, parameters, server),
		};
	}

	private static Guid? ReadCorrelationId(JsonNode? container)
		=> container is JsonObject values &&
			values[CorrelationIdKey] is JsonValue value &&
			value.TryGetValue<string>(out var correlationId) &&
			Guid.TryParse(correlationId, out var guid)
			? guid
			: null;

	/// <summary>
	/// The client name declared on this request (2026-07-28 protocol revision and later), else the one captured by the
	/// <c>initialize</c> handshake, else, for the <c>initialize</c> request itself, the one it carries.
	/// </summary>
	private static string? ReadCallerName(JsonRpcRequest request, JsonObject? parameters, McpServer? server)
		=> NullIfWhiteSpace(request.Context?.ClientInfo?.Name)
			?? NullIfWhiteSpace(server?.ClientInfo?.Name)
			?? (request.Method == RequestMethods.Initialize &&
				parameters?["clientInfo"] is JsonObject clientInfo &&
				clientInfo["name"] is JsonValue name &&
				name.TryGetValue<string>(out var value)
					? NullIfWhiteSpace(value)
					: null);

	private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

	/// <summary>Names the request the way the SDK names its span, e.g. <c>tools/call get_category</c>.</summary>
	private static string GetOperationName(JsonRpcRequest request)
		=> request.Method == RequestMethods.ToolsCall &&
			request.Params is JsonObject parameters &&
			parameters["name"] is JsonValue name &&
			name.TryGetValue<string>(out var toolName)
			? $"{request.Method} {toolName}"
			: request.Method;

	private static double GetElapsedMilliseconds(long startTimestamp)
		=> Math.Round(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds, 1);

	[LoggerMessage(EventId = 1000, Level = LogLevel.Information,
		Message = "MCP request {McpOperation} started. CorrelationId: {CorrelationId}, McpRequestId: {McpRequestId}, CallerName: {CallerName}")]
	private static partial void LogRequestStarted(ILogger logger, string mcpOperation, string correlationId, string? mcpRequestId, string? callerName);

	[LoggerMessage(EventId = 1001, Level = LogLevel.Information,
		Message = "MCP request {McpOperation} completed in {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}, McpRequestId: {McpRequestId}, CallerName: {CallerName}")]
	private static partial void LogRequestCompleted(ILogger logger, string mcpOperation, double elapsedMilliseconds, string correlationId, string? mcpRequestId, string? callerName);

	[LoggerMessage(EventId = 1002, Level = LogLevel.Error,
		Message = "MCP request {McpOperation} failed after {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}, McpRequestId: {McpRequestId}, CallerName: {CallerName}")]
	private static partial void LogRequestFailed(ILogger logger, Exception exception, string mcpOperation, double elapsedMilliseconds, string correlationId, string? mcpRequestId, string? callerName);
}
