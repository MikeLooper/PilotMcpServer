using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models.Response;

[TestFixture]
public class AddResponseIntTests
{
    [Test]
    public void AddResponseInt_Id_SetAndRead_ReturnsSameValue_Test()
    {
        var response = new AddResponseInt { Id = 42 };

        Assert.That(response.Id, Is.EqualTo(42));
    }

    [Test]
    public void AddResponseInt_Deserialize_MapsIdFromApiJson_Test()
    {
        const string json = """{"id":42}""";

        var response = JsonSerializer.Deserialize<AddResponseInt>(json);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(42));
    }
}
