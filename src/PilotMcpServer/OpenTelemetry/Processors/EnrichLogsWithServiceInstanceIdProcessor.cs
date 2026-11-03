using OpenTelemetry;
using OpenTelemetry.Logs;

namespace PilotMcpServer.OpenTelemetry.Processors;

/// <summary>
/// Adds this server's instance ID (<see cref="OtelAttributeNames.ServiceInstanceIdAttribute"/>) to every log record,
/// built from the Pilot API selected when the record is written. Must be registered before the exporter.
/// </summary>
public sealed class EnrichLogsWithServiceInstanceIdProcessor(ServiceInstanceIdProvider serviceInstanceIdProvider) : BaseProcessor<LogRecord>
{
	public override void OnEnd(LogRecord data)
	{
		var attributes = data.Attributes?.ToList() ?? [];
		if (attributes.Exists(existing => string.Equals(existing.Key, OtelAttributeNames.ServiceInstanceIdAttribute, StringComparison.OrdinalIgnoreCase)))
		{
			return;
		}

		attributes.Add(new(OtelAttributeNames.ServiceInstanceIdAttribute, serviceInstanceIdProvider.Current));
		data.Attributes = attributes;
	}
}
