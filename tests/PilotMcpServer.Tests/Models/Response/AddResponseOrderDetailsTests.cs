using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models.Response;

[TestFixture]
public class AddResponseOrderDetailsTests
{
    [Test]
    public void AddResponseOrderDetails_Keys_SetAndRead_ReturnSameValues_Test()
    {
        var response = new AddResponseOrderDetails { OrderId = 10248, ProductId = 11 };

        Assert.That(response.OrderId, Is.EqualTo(10248));
        Assert.That(response.ProductId, Is.EqualTo(11));
    }

    [Test]
    public void AddResponseOrderDetails_Deserialize_MapsCompositeKeyFromApiJson_Test()
    {
        const string json = """{"orderId":10248,"productId":11}""";

        var response = JsonSerializer.Deserialize<AddResponseOrderDetails>(json);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.OrderId, Is.EqualTo(10248));
        Assert.That(response.ProductId, Is.EqualTo(11));
    }
}
