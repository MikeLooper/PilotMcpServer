using PilotMcpServer.OpenTelemetry;

namespace PilotMcpServer.Tests.OpenTelemetry;

[TestFixture]
public class ServiceInstanceIdTests
{
	[TestCase("PilotMcpServer", "pilot-api-dotnet-mssql", "pilot-mcp-server-dotnet-mssql")]
	[TestCase("PilotMcpServer", "pilot-api-python-postgres", "pilot-mcp-server-python-postgres")]
	[TestCase("PilotMcpServer", "pilot-api-java-postgres", "pilot-mcp-server-java-postgres")]
	public void ServiceInstanceId_Create_CombinesServiceNameAndApiSuffix_Test(string serviceName, string containerName, string expected)
	{
		Assert.That(ServiceInstanceId.Create(serviceName, containerName), Is.EqualTo(expected));
	}

	[Test]
	public void ServiceInstanceId_Create_EmptyContainerName_ReturnsServiceNameOnly_Test()
	{
		Assert.That(ServiceInstanceId.Create("PilotMcpServer", ""), Is.EqualTo("pilot-mcp-server"));
	}

	[TestCase("pilot-api-dotnet-mssql", "dotnet-mssql")]
	[TestCase("pilot-api-python-postgres", "python-postgres")]
	[TestCase("dotnet-mssql", "dotnet-mssql")]
	[TestCase("standalone", "standalone")]
	public void ServiceInstanceId_GetSuffix_ReturnsTextAfterSecondToLastDash_Test(string containerName, string expected)
	{
		Assert.That(ServiceInstanceId.GetSuffix(containerName), Is.EqualTo(expected));
	}

	[TestCase("PilotMcpServer", "pilot-mcp-server")]
	[TestCase("pilotMcpServer", "pilot-mcp-server")]
	[TestCase("PilotMCPServer", "pilot-mcp-server")]
	[TestCase("Pilot Mcp Server", "pilot-mcp-server")]
	[TestCase("pilot_mcp_server", "pilot-mcp-server")]
	[TestCase("pilot-mcp-server", "pilot-mcp-server")]
	[TestCase("Server2Go", "server2-go")]
	public void ServiceInstanceId_ToKebabCase_ConvertsServiceName_Test(string value, string expected)
	{
		Assert.That(ServiceInstanceId.ToKebabCase(value), Is.EqualTo(expected));
	}
}
