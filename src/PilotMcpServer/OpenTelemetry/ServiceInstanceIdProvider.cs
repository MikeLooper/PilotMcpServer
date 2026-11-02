using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;

namespace PilotMcpServer.OpenTelemetry;

/// <summary>
/// Supplies the <see cref="ServiceInstanceId"/> for the currently selected Pilot API. The selection can change while the
/// server runs, so the ID is resolved on each read and cached until the selection changes.
/// </summary>
public sealed class ServiceInstanceIdProvider(string serviceName, IPilotApiSelection selection)
{
	private sealed record Entry(PilotApiEndpoint Endpoint, string InstanceId);

	private Entry? _entry;

	/// <summary>The instance ID for <see cref="IPilotApiSelection.Current"/>.</summary>
	public string Current
	{
		get
		{
			var endpoint = selection.Current;
			var entry = Volatile.Read(ref _entry);
			if (entry is null || !ReferenceEquals(entry.Endpoint, endpoint))
			{
				entry = new Entry(endpoint, ServiceInstanceId.Create(serviceName, endpoint.ContainerName));
				Volatile.Write(ref _entry, entry);
			}

			return entry.InstanceId;
		}
	}
}
