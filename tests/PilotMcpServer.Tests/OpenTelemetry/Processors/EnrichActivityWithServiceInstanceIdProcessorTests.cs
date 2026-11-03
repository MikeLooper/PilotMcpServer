using System.Diagnostics;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.OpenTelemetry.Processors;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.OpenTelemetry.Processors;

[TestFixture]
public class EnrichActivityWithServiceInstanceIdProcessorTests
{
	[Test]
	public void EnrichActivityWithServiceInstanceIdProcessor_OnStart_TagsServiceInstanceId_Test()
	{
		var provider = new ServiceInstanceIdProvider("PilotMcpServer", new PilotApiSelectionState());
		using var activity = new Activity("test");

		new EnrichActivityWithServiceInstanceIdProcessor(provider).OnStart(activity);

		Assert.That(activity.GetTagItem("service.instance.id"), Is.EqualTo("pilot-mcp-server-dotnet-mssql"));
	}
}
