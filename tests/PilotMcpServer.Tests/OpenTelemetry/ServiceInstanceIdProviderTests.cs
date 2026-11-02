using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.OpenTelemetry;

[TestFixture]
public class ServiceInstanceIdProviderTests
{
	[Test]
	public void ServiceInstanceIdProvider_Current_UsesCatalogDefault_Test()
	{
		var provider = new ServiceInstanceIdProvider("PilotMcpServer", new PilotApiSelectionState());

		Assert.That(provider.Current, Is.EqualTo("pilot-mcp-server-dotnet-mssql"));
	}

	[Test]
	public void ServiceInstanceIdProvider_Current_FollowsSelectionChanges_Test()
	{
		var selection = new PilotApiSelectionState();
		var provider = new ServiceInstanceIdProvider("PilotMcpServer", selection);
		_ = provider.Current;

		selection.SetCurrent("Python with PostgreSQL");

		Assert.That(provider.Current, Is.EqualTo("pilot-mcp-server-python-postgres"));
	}
}
