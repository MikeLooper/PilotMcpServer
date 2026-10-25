using OpenTelemetry;
using OpenTelemetry.Logs;
using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.OpenTelemetry.Processors;

/// <summary>
/// Adds the current <see cref="Models.RequestSettings.CorrelationId"/> to every log record written during a tool
/// call. Must be registered before the exporter so it runs synchronously on the logging call's execution context.
/// </summary>
public sealed class EnrichLogsWithRequestSettingsProcessor(IRequestSettingsAccessor requestSettingsAccessor) : BaseProcessor<LogRecord>
{
	public override void OnEnd(LogRecord data)
	{
		var requestSettings = requestSettingsAccessor.Current;
		if (requestSettings is null)
		{
			return;
		}

		var attributes = data.Attributes?.ToList() ?? [];
		attributes.Add(new KeyValuePair<string, object?>(OtelAttributeNames.CorrelationIdAttribute, requestSettings.CorrelationId));
		data.Attributes = attributes;
	}
}
