namespace PilotMcpServer.Configuration;

/// <summary>Describes where the server is running, so hostnames can be chosen accordingly.</summary>
public static class DeploymentEnvironment
{
	public const string RunningInDockerVariableName = "RUNNING_IN_DOCKER";

	/// <summary>
	/// <see langword="true"/> when the <c>RUNNING_IN_DOCKER</c> environment variable is set to <c>true</c>
	/// (case-insensitive), meaning container names should be used instead of host names.
	/// </summary>
	public static bool IsRunningInDocker()
	{
		var value = Environment.GetEnvironmentVariable(RunningInDockerVariableName);
		return bool.TryParse(value, out var isRunningInDocker) && isRunningInDocker;
	}
}
