using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class PilotApiEndpointTests
{
    [Test]
    public void PilotApiEndpoint_BaseUrl_CombinesHostAndPort_Test()
    {
        var endpoint = new PilotApiEndpoint(".NET Core with SQL Server", "localhost", 11001, "pilot-api-dotnet-mssql");

        Assert.That(endpoint.BaseUrl, Is.EqualTo("http://localhost:11001"));
    }

    [Test]
    public void PilotApiEndpoint_Constructor_SetsNameHostPortAndContainerName_Test()
    {
        var endpoint = new PilotApiEndpoint("Python with PostgreSQL", "localhost", 11701, "pilot-api-python-postgres");

        Assert.That(endpoint.Name, Is.EqualTo("Python with PostgreSQL"));
        Assert.That(endpoint.Host, Is.EqualTo("localhost"));
        Assert.That(endpoint.Port, Is.EqualTo(11701));
        Assert.That(endpoint.ContainerName, Is.EqualTo("pilot-api-python-postgres"));
    }

    [Test]
    public void PilotApiEndpoint_Equals_SameValues_AreEqual_Test()
    {
        var first = new PilotApiEndpoint("Shared", "localhost", 11001, "pilot-api-dotnet-mssql");
        var second = new PilotApiEndpoint("Shared", "localhost", 11001, "pilot-api-dotnet-mssql");

        Assert.That(first, Is.EqualTo(second));
    }
}
