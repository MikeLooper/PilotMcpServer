namespace PilotMcpServer.Tests.Extensions;

[TestFixture]
public class McpServerInfoFactoryTests
{
	[Test]
	public void McpServerInfoFactory_Create_ReturnsImplementationWithExpectedDescription_Test()
	{
		var info = PilotMcpServer.Extensions.McpServerInfoFactory.Create();

		Assert.That(info.Description, Is.EqualTo(PilotMcpServer.Extensions.McpServerInfoFactory.ServerDescription));
	}

	[Test]
	public void McpServerInfoFactory_Create_ReturnsImplementationWithExpectedName_Test()
	{
		var info = PilotMcpServer.Extensions.McpServerInfoFactory.Create();

		Assert.That(info.Name, Is.EqualTo(PilotMcpServer.Extensions.McpServerInfoFactory.ServerName));
	}

	[Test]
	public void McpServerInfoFactory_Create_ReturnsImplementationWithExpectedTitle_Test()
	{
		var info = PilotMcpServer.Extensions.McpServerInfoFactory.Create();

		Assert.That(info.Title, Is.EqualTo(PilotMcpServer.Extensions.McpServerInfoFactory.ServerTitle));
	}

	[Test]
	public void McpServerInfoFactory_Create_ReturnsImplementationWithExpectedVersion_Test()
	{
		var info = PilotMcpServer.Extensions.McpServerInfoFactory.Create();

		Assert.That(info.Version, Is.EqualTo(PilotMcpServer.Extensions.McpServerInfoFactory.ServerVersion));
	}

	[Test]
	public void McpServerInfoFactory_Create_ReturnsImplementationWithExpectedWebsiteUrl_Test()
	{
		var info = PilotMcpServer.Extensions.McpServerInfoFactory.Create();

		Assert.That(info.WebsiteUrl, Is.EqualTo(PilotMcpServer.Extensions.McpServerInfoFactory.ServerWebsiteUrl));
	}
}
