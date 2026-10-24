using PilotMcpServer.Configuration;

namespace PilotMcpServer.Tests.Configuration;

[TestFixture]
public class PilotApiCatalogTests
{
    [Test]
    public void PilotApiCatalog_All_ContainsExactlySixEntries_Test()
    {
        var all = PilotApiCatalog.All;

        Assert.That(all, Has.Count.EqualTo(6));
    }

    [TestCase(".NET Core with SQL Server", "localhost", 11001)]
    [TestCase(".NET Core with PostgreSQL", "localhost", 11101)]
    [TestCase("Java Spring Boot with SQL Server", "localhost", 11201)]
    [TestCase("Java Spring Boot with PostgreSQL", "localhost", 11301)]
    [TestCase("Python with SQL Server", "localhost", 11401)]
    [TestCase("Python with PostgreSQL", "localhost", 11501)]
    public void PilotApiCatalog_All_ContainsExpectedEntry_Test(string name, string host, int port)
    {
        var all = PilotApiCatalog.All;

        var entry = all.SingleOrDefault(a => a.Name == name);

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Host, Is.EqualTo(host));
        Assert.That(entry.Port, Is.EqualTo(port));
    }

    [Test]
    public void PilotApiCatalog_Default_IsDotNetCoreWithSqlServer_Test()
    {
        var defaultEntry = PilotApiCatalog.Default;

        Assert.That(defaultEntry.Name, Is.EqualTo(".NET Core with SQL Server"));
        Assert.That(defaultEntry.Port, Is.EqualTo(11001));
    }

    [Test]
    public void PilotApiCatalog_TryGet_KnownNameCaseInsensitive_ReturnsTrueAndEntry_Test()
    {
        var found = PilotApiCatalog.TryGet(".net core with sql server", out var endpoint);

        Assert.That(found, Is.True);
        Assert.That(endpoint.Name, Is.EqualTo(".NET Core with SQL Server"));
    }

    [Test]
    public void PilotApiCatalog_TryGet_UnknownName_ReturnsFalse_Test()
    {
        var found = PilotApiCatalog.TryGet("Not A Real Api", out _);

        Assert.That(found, Is.False);
    }
}
