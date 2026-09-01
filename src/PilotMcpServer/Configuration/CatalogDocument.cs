namespace PilotMcpServer.Configuration
{
	/// <summary>
	/// A static, resource-grouped summary of the endpoints defined by the Pilot API OpenAPI contract
	/// </summary>
	public sealed class CatalogDocument
	{
		/// <summary>
		/// The version of the API that this catalog document describes. This is useful for clients to know which version of the API they are interacting with.
		/// </summary>
		public string? ApiVersion { get; init; }

		/// <summary>
		/// The default API name for this catalog document. This is useful for clients to know which API to use when multiple APIs are available.
		/// </summary>
		public string? DefaultApiName { get; init; }

		/// <summary>
		///	The list of APIs described in this catalog document. Each API is represented by a CatalogApiItem, which contains information about the API's endpoints and resources.
		/// </summary>
		public List<CatalogApiItem> Apis { get; init; } = [];
	}
}
