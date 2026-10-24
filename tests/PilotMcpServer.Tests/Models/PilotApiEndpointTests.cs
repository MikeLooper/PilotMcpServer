using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class PilotApiEndpointTests
{
    [Test]
    public void PilotApiEndpoint_BaseUrl_CombinesHostAndPort_Test()
    {
        var endpoint = new PilotApiEndpoint(".NET Core with SQL Server", "localhost", 11001);

        Assert.That(endpoint.BaseUrl, Is.EqualTo("http://localhost:11001"));
    }

    [Test]
    public void PilotApiEndpoint_Constructor_SetsNameHostAndPort_Test()
    {
        var endpoint = new PilotApiEndpoint("Python with PostgreSQL", "localhost", 11701);

        Assert.That(endpoint.Name, Is.EqualTo("Python with PostgreSQL"));
        Assert.That(endpoint.Host, Is.EqualTo("localhost"));
        Assert.That(endpoint.Port, Is.EqualTo(11701));
    }

    [Test]
    public void PilotApiEndpoint_Equals_SameValues_AreEqual_Test()
    {
        var first = new PilotApiEndpoint("Shared", "localhost", 11001);
        var second = new PilotApiEndpoint("Shared", "localhost", 11001);

        Assert.That(first, Is.EqualTo(second));
    }
}
