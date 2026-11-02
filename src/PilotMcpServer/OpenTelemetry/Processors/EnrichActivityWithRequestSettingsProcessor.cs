using System.Diagnostics;
using OpenTelemetry;
using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.OpenTelemetry.Processors;

/// <summary>
/// Tags every span started during a tool call (e.g. the outgoing Pilot API HTTP calls) with the current
/// <see cref="Models.RequestSettings"/>: its correlation ID and, when known, its request ID and caller name. The tags
/// are set when the span starts, while the tool call's execution context, and so its request settings, is still current.
/// </summary>
public sealed class EnrichActivityWithRequestSettingsProcessor(IRequestSettingsAccessor requestSettingsAccessor) : BaseProcessor<Activity>
{
	public override void OnStart(Activity data)
	{
		var requestSettings = requestSettingsAccessor.Current;
		if (requestSettings is not null)
		{
			data.SetRequestSettingsTags(requestSettings);
		}
	}
}
