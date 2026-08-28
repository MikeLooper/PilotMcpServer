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
	}
}
