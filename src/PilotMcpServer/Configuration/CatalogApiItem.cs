namespace PilotMcpServer.Configuration
{
	/// <summary>
	/// Represents a single Pilot API deployment, including its name, host, container name, and port.
	/// </summary>
	public sealed class CatalogApiItem
	{
		/// <summary>
		/// Gets or sets the name of the Pilot API deployment.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// Gets or sets the host of the Pilot API deployment.
		/// </summary>
		public required string Host { get; init; }

		/// <summary>
		/// Gets or sets the container name of the Pilot API deployment.
		/// </summary>
		public required string ContainerName { get; init; }

		/// <summary>
		/// Gets or sets the port of the Pilot API deployment.
		/// </summary>
		public required int Port { get; init; }
	}
}
