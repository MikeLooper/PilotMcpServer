using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class OrderDetailDtoTests
{
    [Test]
    public void OrderDetailDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """{"orderID":10248,"productID":11,"unitPrice":14,"quantity":12,"discount":0.15}""";

        var orderDetail = JsonSerializer.Deserialize<OrderDetailDto>(json);

        Assert.That(orderDetail, Is.Not.Null);
        Assert.That(orderDetail!.OrderId, Is.EqualTo(10248));
        Assert.That(orderDetail.ProductId, Is.EqualTo(11));
        Assert.That(orderDetail.UnitPrice, Is.EqualTo(14));
        Assert.That(orderDetail.Quantity, Is.EqualTo((short)12));
        Assert.That(orderDetail.Discount, Is.EqualTo(0.15f));
    }

    [Test]
    public void OrderDetailDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var orderDetail = new OrderDetailDto { OrderId = 10248, ProductId = 11, UnitPrice = 14, Quantity = 12, Discount = 0.15f };

        var json = JsonSerializer.Serialize(orderDetail);

        Assert.That(json, Does.Contain("\"orderID\":10248"));
        Assert.That(json, Does.Contain("\"productID\":11"));
        Assert.That(json, Does.Contain("\"unitPrice\":14"));
        Assert.That(json, Does.Contain("\"quantity\":12"));
        Assert.That(json, Does.Contain("\"discount\":0.15"));
    }
}
