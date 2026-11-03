using System.Diagnostics;
using OpenTelemetry;

namespace PilotMcpServer.OpenTelemetry.Processors;

/// <summary>
/// Tags every span with this server's instance ID (<see cref="OtelAttributeNames.ServiceInstanceIdTag"/>), built from
/// the Pilot API selected when the span starts.
/// </summary>
public sealed class EnrichActivityWithServiceInstanceIdProcessor(ServiceInstanceIdProvider serviceInstanceIdProvider) : BaseProcessor<Activity>
{
	public override void OnStart(Activity data) => data.SetTag(OtelAttributeNames.ServiceInstanceIdTag, serviceInstanceIdProvider.Current);
}
