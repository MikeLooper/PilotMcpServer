using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class ShipperDtoTests
{
    [Test]
    public void ShipperDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """{"shipperID":1,"companyName":"Speedy Express","phone":"(503) 555-9831"}""";

        var shipper = JsonSerializer.Deserialize<ShipperDto>(json);

        Assert.That(shipper, Is.Not.Null);
        Assert.That(shipper!.ShipperId, Is.EqualTo(1));
        Assert.That(shipper.CompanyName, Is.EqualTo("Speedy Express"));
        Assert.That(shipper.Phone, Is.EqualTo("(503) 555-9831"));
    }

    [Test]
    public void ShipperDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var shipper = new ShipperDto { ShipperId = 1, CompanyName = "Speedy Express" };

        var json = JsonSerializer.Serialize(shipper);

        Assert.That(json, Does.Contain("\"shipperID\":1"));
        Assert.That(json, Does.Contain("\"companyName\":\"Speedy Express\""));
    }

    [Test]
    public void ShipperDto_OptionalPhone_Unset_IsNull_Test()
    {
        var shipper = new ShipperDto { ShipperId = 1, CompanyName = "Speedy Express" };

        Assert.That(shipper.Phone, Is.Null);
    }
}
