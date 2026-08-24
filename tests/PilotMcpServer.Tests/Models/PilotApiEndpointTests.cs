using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class PilotApiEndpointTests
{
    [Test]
    public void PilotApiEndpoint_BaseUrl_CombinesHostAndPort_Test()
    {
        var endpoint = new PilotApiEndpoint(".NET Core with SQL Server", "localhost", 55101);

        Assert.That(endpoint.BaseUrl, Is.EqualTo("http://localhost:55101"));
    }

    [Test]
    public void PilotApiEndpoint_Constructor_SetsNameHostAndPort_Test()
    {
        var endpoint = new PilotApiEndpoint("Python with PostgreSQL", "localhost", 55801);

        Assert.That(endpoint.Name, Is.EqualTo("Python with PostgreSQL"));
        Assert.That(endpoint.Host, Is.EqualTo("localhost"));
        Assert.That(endpoint.Port, Is.EqualTo(55801));
    }

    [Test]
    public void PilotApiEndpoint_Equals_SameValues_AreEqual_Test()
    {
        var first = new PilotApiEndpoint("Shared", "localhost", 55101);
        var second = new PilotApiEndpoint("Shared", "localhost", 55101);

        Assert.That(first, Is.EqualTo(second));
    }
}
