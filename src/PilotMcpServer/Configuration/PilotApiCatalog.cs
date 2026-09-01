using PilotMcpServer.Models;
using System.Text.Json;

namespace PilotMcpServer.Configuration;

/// <summary>
/// The fixed set of Pilot API deployments this server can talk to. The catalog is stored in an
/// embedded JSON configuration resource so it ships inside the application and cannot be altered
/// post-deployment by editing a file on disk.
/// </summary>
public static class PilotApiCatalog
{
	private const string ResourceName = "PilotMcpServer.appsettings.json";
	private const string RunningInDockerVariableName = "RUNNING_IN_DOCKER";

	public static readonly CatalogDocument Catalog = LoadCatalog();
	private static readonly bool IsRunningInDocker = DetectDockerDeployment();

	public static readonly IReadOnlyList<PilotApiEndpoint> All = Catalog.Apis
		.Select(api => new PilotApiEndpoint(
			api.Name,
			IsRunningInDocker ? api.ContainerName : api.Host,
			api.Port))
		.ToArray();

	/// <summary>The API used when no explicit selection has been made.</summary>
	public static PilotApiEndpoint Default { get; } = ResolveDefault(Catalog.DefaultApiName, All);

	/// <summary>Looks up a catalog entry by its exact (case-insensitive) name.</summary>
	public static bool TryGet(string name, out PilotApiEndpoint endpoint)
	{
		foreach (var candidate in All)
		{
			if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				endpoint = candidate;
				return true;
			}
		}

		endpoint = null!;
		return false;
	}

	private static bool DetectDockerDeployment()
	{
		var value = Environment.GetEnvironmentVariable(RunningInDockerVariableName);
		return bool.TryParse(value, out var isRunningInDocker) && isRunningInDocker;
	}

	private static CatalogDocument LoadCatalog()
	{
		var assembly = typeof(PilotApiCatalog).Assembly;

		using var stream = assembly.GetManifestResourceStream(ResourceName)
			?? throw new InvalidOperationException($"Embedded configuration resource '{ResourceName}' was not found.");

		var document = JsonSerializer.Deserialize<CatalogDocument>(stream, JsonSerializerOptions.Web)
			?? throw new InvalidOperationException("Embedded Pilot API catalog configuration could not be deserialized.");

		if (document.Apis.Count == 0)
		{
			throw new InvalidOperationException("Embedded Pilot API catalog configuration does not contain any API entries.");
		}

		return document;
	}

	private static PilotApiEndpoint ResolveDefault(string? defaultApiName, IReadOnlyList<PilotApiEndpoint> apis)
	{
		if (!string.IsNullOrWhiteSpace(defaultApiName))
		{
			foreach (var candidate in apis)
			{
				if (string.Equals(candidate.Name, defaultApiName, StringComparison.OrdinalIgnoreCase))
				{
					return candidate;
				}
			}

			throw new InvalidOperationException(
				$"The configured default API '{defaultApiName}' does not exist in the embedded Pilot API catalog.");
		}

		return apis[0];
	}
}