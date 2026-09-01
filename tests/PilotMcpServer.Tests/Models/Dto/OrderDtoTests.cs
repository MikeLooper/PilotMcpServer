using System.Text.Json;
using PilotMcpServer.Models.Dto;

namespace PilotMcpServer.Tests.Models.Dto;

[TestFixture]
public class OrderDtoTests
{
    [Test]
    public void OrderDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "orderID": 10248,
              "customerID": "VINET",
              "employeeID": 5,
              "orderDate": "1996-07-04T00:00:00",
              "requiredDate": "1996-08-01T00:00:00",
              "shippedDate": "1996-07-16T00:00:00",
              "shipVia": 3,
              "freight": 32.38,
              "shipName": "Vins et alcools Chevalier",
              "shipAddress": "59 rue de l'Abbaye",
              "shipCity": "Reims",
              "shipRegion": "Western Europe",
              "shipPostalCode": "51100",
              "shipCountry": "France"
            }
            """;

        var order = JsonSerializer.Deserialize<OrderDto>(json);

        Assert.That(order, Is.Not.Null);
        Assert.That(order!.OrderId, Is.EqualTo(10248));
        Assert.That(order.CustomerId, Is.EqualTo("VINET"));
        Assert.That(order.EmployeeId, Is.EqualTo(5));
        Assert.That(order.OrderDate, Is.EqualTo(new DateTime(1996, 7, 4)));
        Assert.That(order.RequiredDate, Is.EqualTo(new DateTime(1996, 8, 1)));
        Assert.That(order.ShippedDate, Is.EqualTo(new DateTime(1996, 7, 16)));
        Assert.That(order.ShipVia, Is.EqualTo(3));
        Assert.That(order.Freight, Is.EqualTo(32.38));
        Assert.That(order.ShipName, Is.EqualTo("Vins et alcools Chevalier"));
        Assert.That(order.ShipAddress, Is.EqualTo("59 rue de l'Abbaye"));
        Assert.That(order.ShipCity, Is.EqualTo("Reims"));
        Assert.That(order.ShipRegion, Is.EqualTo("Western Europe"));
        Assert.That(order.ShipPostalCode, Is.EqualTo("51100"));
        Assert.That(order.ShipCountry, Is.EqualTo("France"));
    }

    [Test]
    public void OrderDto_Deserialize_NullableFieldsAsNull_LeavesPropertiesNull_Test()
    {
        const string json = """{"orderID":10249,"shippedDate":null}""";

        var order = JsonSerializer.Deserialize<OrderDto>(json);

        Assert.That(order, Is.Not.Null);
        Assert.That(order!.OrderId, Is.EqualTo(10249));
        Assert.That(order.ShippedDate, Is.Null);
        Assert.That(order.CustomerId, Is.Null);
        Assert.That(order.EmployeeId, Is.Null);
    }

    [Test]
    public void OrderDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var order = new OrderDto { OrderId = 10248, ShipCity = "Reims" };

        var json = JsonSerializer.Serialize(order);

        Assert.That(json, Does.Contain("\"orderID\":10248"));
        Assert.That(json, Does.Contain("\"shipCity\":\"Reims\""));
    }
}
