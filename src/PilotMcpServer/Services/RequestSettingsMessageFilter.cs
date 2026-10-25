using System.Diagnostics;
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
/// Notifications and responses pass through unchanged.
/// </summary>
public static class RequestSettingsMessageFilter
{
	public static McpMessageHandler Create(McpMessageHandler next)
		=> async (context, cancellationToken) =>
		{
			if (context.JsonRpcMessage is not JsonRpcRequest)
			{
				await next(context, cancellationToken).ConfigureAwait(false);
				return;
			}

			var accessor = context.Services?.GetService<IRequestSettingsAccessor>() ?? new RequestSettingsAccessor();
			var previous = accessor.Current;
			var requestSettings = new RequestSettings();

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
}
