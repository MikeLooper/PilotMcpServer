using OpenTelemetry;
using OpenTelemetry.Logs;
using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.OpenTelemetry.Processors;

/// <summary>
/// Adds the current <see cref="Models.RequestSettings"/> to every log record written during a tool call: its
/// correlation ID and, when known, its request ID and caller name. An attribute the record already carries (compared
/// case-insensitively, e.g. one written by the request start and end entries' message templates) is not added again.
/// Must be registered before the exporter so it runs synchronously on the logging call's execution context.
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
		var added = false;
		foreach (var attribute in requestSettings.GetLogAttributes())
		{
			if (!attributes.Exists(existing => string.Equals(existing.Key, attribute.Key, StringComparison.OrdinalIgnoreCase)))
			{
				attributes.Add(attribute);
				added = true;
			}
		}

		if (added)
		{
			data.Attributes = attributes;
		}
	}
}
