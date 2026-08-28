using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models.Response;

[TestFixture]
public class AboutResponseTests
{
    [Test]
    public void AboutResponse_Properties_SetAndRead_ReturnSameValues_Test()
    {
        var about = new AboutResponse
        {
            Name = "PilotApiDotNet",
            ApiVersion = "1.0.0",
            BuildVersion = "1.0.0.456",
            DeployDate = "2026-01-15",
        };

        Assert.That(about.Name, Is.EqualTo("PilotApiDotNet"));
        Assert.That(about.ApiVersion, Is.EqualTo("1.0.0"));
        Assert.That(about.BuildVersion, Is.EqualTo("1.0.0.456"));
        Assert.That(about.DeployDate, Is.EqualTo("2026-01-15"));
    }

    [Test]
    public void AboutResponse_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """{"name":"PilotApiDotNet","apiVersion":"1.0.0","buildVersion":"1.0.0.456","deployDate":"2026-01-15"}""";

        var about = JsonSerializer.Deserialize<AboutResponse>(json);

        Assert.That(about, Is.Not.Null);
        Assert.That(about!.Name, Is.EqualTo("PilotApiDotNet"));
        Assert.That(about.ApiVersion, Is.EqualTo("1.0.0"));
        Assert.That(about.BuildVersion, Is.EqualTo("1.0.0.456"));
        Assert.That(about.DeployDate, Is.EqualTo("2026-01-15"));
    }

    [Test]
    public void AboutResponse_Deserialize_MissingFields_LeavesPropertiesNull_Test()
    {
        const string json = "{}";

        var about = JsonSerializer.Deserialize<AboutResponse>(json);

        Assert.That(about, Is.Not.Null);
        Assert.That(about!.Name, Is.Null);
        Assert.That(about.ApiVersion, Is.Null);
        Assert.That(about.BuildVersion, Is.Null);
        Assert.That(about.DeployDate, Is.Null);
    }
}
