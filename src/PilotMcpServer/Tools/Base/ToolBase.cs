using PilotMcpServer.Configuration;

namespace PilotMcpServer.Tools.Base
{
	public class ToolBase
	{
		public ToolBase()
		{
			this.ApiVersion = PilotApiCatalog.Catalog.ApiVersion;
		}

		protected string? ApiVersion { get; }

		/// <summary>
		/// Appends the optional <c>page</c> and <c>pageSize</c> query parameters defined by the Pilot API "get-all"
		/// endpoints. Parameters left <see langword="null"/> are omitted so the API applies its own defaults.
		/// </summary>
		protected static string WithPaging(string path, int? page, int? pageSize)
		{
			var query = new List<string>(2);
			if (page is not null)
			{
				query.Add($"page={page}");
			}

			if (pageSize is not null)
			{
				query.Add($"pageSize={pageSize}");
			}

			return query.Count == 0 ? path : $"{path}?{string.Join('&', query)}";
		}
	}
}
