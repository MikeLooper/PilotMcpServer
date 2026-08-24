using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class PilotApiStatusTests
{
    [Test]
    public void PilotApiStatus_Properties_AvailableApi_SetAndRead_ReturnSameValues_Test()
    {
        var status = new PilotApiStatus
        {
            Name = ".NET Core with SQL Server",
            Host = "localhost",
            Port = 55101,
            IsSelected = true,
            IsDefault = true,
            IsAvailable = true,
            ApiVersion = "1.0.0",
            BuildVersion = "1.0.0.123",
            DeployDate = "2026-01-01",
        };

        Assert.That(status.Name, Is.EqualTo(".NET Core with SQL Server"));
        Assert.That(status.Host, Is.EqualTo("localhost"));
        Assert.That(status.Port, Is.EqualTo(55101));
        Assert.That(status.IsSelected, Is.True);
        Assert.That(status.IsDefault, Is.True);
        Assert.That(status.IsAvailable, Is.True);
        Assert.That(status.ApiVersion, Is.EqualTo("1.0.0"));
        Assert.That(status.BuildVersion, Is.EqualTo("1.0.0.123"));
        Assert.That(status.DeployDate, Is.EqualTo("2026-01-01"));
        Assert.That(status.Error, Is.Null);
    }

    [Test]
    public void PilotApiStatus_Properties_UnavailableApi_LeavesVersionFieldsNullAndSetsError_Test()
    {
        var status = new PilotApiStatus
        {
            Name = "Java Spring Boot with SQL Server",
            Host = "localhost",
            Port = 55301,
            IsSelected = false,
            IsDefault = false,
            IsAvailable = false,
            Error = "Connection refused",
        };

        Assert.That(status.IsAvailable, Is.False);
        Assert.That(status.ApiVersion, Is.Null);
        Assert.That(status.BuildVersion, Is.Null);
        Assert.That(status.DeployDate, Is.Null);
        Assert.That(status.Error, Is.EqualTo("Connection refused"));
    }
}
