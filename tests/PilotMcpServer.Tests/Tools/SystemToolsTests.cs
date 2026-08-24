using ModelContextProtocol;
using Moq;
using PilotMcpServer.Configuration;
using PilotMcpServer.Models;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class SystemToolsTests
{
    [Test]
    public async Task SystemTools_SelectApiAsync_ValidName_UpdatesSelectionState_Test()
    {
        var client = new Mock<IPilotHttpClient>();
        var selection = new PilotApiSelectionState();
        var tools = new SystemTools(client.Object, selection);

        await tools.SelectApiAsync("Python with PostgreSQL", CancellationToken.None);

        Assert.That(selection.Current.Name, Is.EqualTo("Python with PostgreSQL"));
    }

    [Test]
    public void SystemTools_SelectApiAsync_UnknownName_ThrowsMcpException_Test()
    {
        var client = new Mock<IPilotHttpClient>();
        var selection = new PilotApiSelectionState();
        var tools = new SystemTools(client.Object, selection);

        Assert.ThrowsAsync<McpException>(() => tools.SelectApiAsync("Not A Real Api", CancellationToken.None));
    }

    [Test]
    public async Task SystemTools_ListEndpointsAsync_ReturnsFullEndpointCatalog_Test()
    {
        var client = new Mock<IPilotHttpClient>();
        var selection = new PilotApiSelectionState();
        var tools = new SystemTools(client.Object, selection);

        var result = await tools.ListEndpointsAsync(CancellationToken.None);

        Assert.That(result, Is.SameAs(PilotEndpointCatalog.All));
    }

    [Test]
    public async Task SystemTools_ListApisAsync_MixedAvailability_ReportsEachApiAndFlagsSelection_Test()
    {
        var client = new Mock<IPilotHttpClient>();
        var selection = new PilotApiSelectionState();
        selection.SetCurrent("Python with PostgreSQL");
        var tools = new SystemTools(client.Object, selection);

        foreach (var endpoint in PilotApiCatalog.All)
        {
            if (endpoint.Name == "Python with PostgreSQL")
            {
                client.Setup(c => c.GetAboutAsync(endpoint, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AboutResponse { Name = "PilotApiPython", ApiVersion = "2.0.0", DeployDate = "2026-02-01" });
            }
            else
            {
                client.Setup(c => c.GetAboutAsync(endpoint, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("Connection refused"));
            }
        }

        var result = await tools.ListApisAsync(CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(6));

        var selected = result.Single(r => r.Name == "Python with PostgreSQL");
        Assert.That(selected.IsAvailable, Is.True);
        Assert.That(selected.IsSelected, Is.True);
        Assert.That(selected.IsDefault, Is.False);
        Assert.That(selected.ApiVersion, Is.EqualTo("2.0.0"));

        var defaultApi = result.Single(r => r.Name == ".NET Core with SQL Server");
        Assert.That(defaultApi.IsAvailable, Is.False);
        Assert.That(defaultApi.IsSelected, Is.False);
        Assert.That(defaultApi.IsDefault, Is.True);
        Assert.That(defaultApi.Error, Is.Not.Null.And.Contains("Connection refused"));
    }
}
