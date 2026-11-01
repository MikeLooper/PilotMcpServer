using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;
using PilotMcpServer.OpenTelemetry;

namespace PilotMcpServer.Services;

/// <summary>
/// Incoming MCP message filter that gives each JSON-RPC request (e.g. a <c>tools/call</c>) its own
/// <see cref="RequestSettings"/>, and so its own correlation ID, for the whole time the request is handled, and
/// tags the current span with that correlation ID. A message filter is used rather than a call-tool filter because
/// the SDK writes its "tool completed" / "tool threw" log entries outside the call-tool filter pipeline.
/// The caller can supply the correlation ID as a GUID in <c>params._meta.correlationId</c> on any request, or in
/// <c>params.arguments.correlationId</c> on a tool call. <c>_meta</c> wins if both hold a GUID; if neither does, a
/// new one is generated. Notifications and responses pass through unchanged.
/// </summary>
public static class RequestSettingsMessageFilter
{
	public const string CorrelationIdKey = "correlationId";

	public static McpMessageHandler Create(McpMessageHandler next)
		=> async (context, cancellationToken) =>
		{
			if (context.JsonRpcMessage is not JsonRpcRequest request)
			{
				await next(context, cancellationToken).ConfigureAwait(false);
				return;
			}

			var accessor = context.Services?.GetService<IRequestSettingsAccessor>() ?? new RequestSettingsAccessor();
			var previous = accessor.Current;
			var requestSettings = CreateRequestSettings(request);

			accessor.Current = requestSettings;
			Activity.Current?.SetTag(OtelAttributeNames.CorrelationIdTag, requestSettings.CorrelationId);

			try
			{
				await next(context, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				accessor.Current = previous;
			}
		};

	private static RequestSettings CreateRequestSettings(JsonRpcRequest request)
	{
		var parameters = request.Params as JsonObject;
		var guid = ReadCorrelationId(parameters?["_meta"])
			?? (request.Method == RequestMethods.ToolsCall ? ReadCorrelationId(parameters?["arguments"]) : null);

		return guid is null ? new RequestSettings() : new RequestSettings { CorrelationId = guid.Value.ToString() };
	}

	private static Guid? ReadCorrelationId(JsonNode? container)
		=> container is JsonObject values &&
			values[CorrelationIdKey] is JsonValue value &&
			value.TryGetValue<string>(out var correlationId) &&
			Guid.TryParse(correlationId, out var guid)
			? guid
			: null;
}
