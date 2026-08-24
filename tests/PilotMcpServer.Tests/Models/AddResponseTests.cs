using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class AddResponseTests
{
    [Test]
    public void AddResponse_Id_SetAndRead_ReturnsSameValue_Test()
    {
        var response = new AddResponse { Id = 42 };

        Assert.That(response.Id, Is.EqualTo(42));
    }

    [Test]
    public void AddResponse_Deserialize_MapsIdFromApiJson_Test()
    {
        const string json = """{"id":42}""";

        var response = JsonSerializer.Deserialize<AddResponse>(json);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(42));
    }
}
