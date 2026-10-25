using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models.Response;

[TestFixture]
public class AddResponseStringTests
{
    [Test]
    public void AddResponseString_Id_SetAndRead_ReturnsSameValue_Test()
    {
        var response = new AddResponseString { Id = "NEWCO" };

        Assert.That(response.Id, Is.EqualTo("NEWCO"));
    }

    [Test]
    public void AddResponseString_Deserialize_MapsStringIdFromApiJson_Test()
    {
        const string json = """{"id":"NEWCO"}""";

        var response = JsonSerializer.Deserialize<AddResponseString>(json);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo("NEWCO"));
    }
}
