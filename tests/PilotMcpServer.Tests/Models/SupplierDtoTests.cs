using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class SupplierDtoTests
{
    [Test]
    public void SupplierDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "supplierID": 1,
              "companyName": "Exotic Liquids",
              "contactName": "Charlotte Cooper",
              "contactTitle": "Purchasing Manager",
              "address": "49 Gilbert St.",
              "city": "London",
              "region": "British Isles",
              "postalCode": "EC1 4SD",
              "country": "UK",
              "phone": "(171) 555-2222",
              "fax": "(171) 555-2223",
              "homePage": "http://example.com/exotic-liquids"
            }
            """;

        var supplier = JsonSerializer.Deserialize<SupplierDto>(json);

        Assert.That(supplier, Is.Not.Null);
        Assert.That(supplier!.SupplierId, Is.EqualTo(1));
        Assert.That(supplier.CompanyName, Is.EqualTo("Exotic Liquids"));
        Assert.That(supplier.ContactName, Is.EqualTo("Charlotte Cooper"));
        Assert.That(supplier.ContactTitle, Is.EqualTo("Purchasing Manager"));
        Assert.That(supplier.Address, Is.EqualTo("49 Gilbert St."));
        Assert.That(supplier.City, Is.EqualTo("London"));
        Assert.That(supplier.Region, Is.EqualTo("British Isles"));
        Assert.That(supplier.PostalCode, Is.EqualTo("EC1 4SD"));
        Assert.That(supplier.Country, Is.EqualTo("UK"));
        Assert.That(supplier.Phone, Is.EqualTo("(171) 555-2222"));
        Assert.That(supplier.Fax, Is.EqualTo("(171) 555-2223"));
        Assert.That(supplier.HomePage, Is.EqualTo("http://example.com/exotic-liquids"));
    }

    [Test]
    public void SupplierDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var supplier = new SupplierDto { SupplierId = 1, CompanyName = "Exotic Liquids" };

        var json = JsonSerializer.Serialize(supplier);

        Assert.That(json, Does.Contain("\"supplierID\":1"));
        Assert.That(json, Does.Contain("\"companyName\":\"Exotic Liquids\""));
    }

    [Test]
    public void SupplierDto_OptionalProperties_Unset_AreNull_Test()
    {
        var supplier = new SupplierDto { SupplierId = 1, CompanyName = "Exotic Liquids" };

        Assert.That(supplier.ContactName, Is.Null);
        Assert.That(supplier.ContactTitle, Is.Null);
        Assert.That(supplier.Address, Is.Null);
        Assert.That(supplier.City, Is.Null);
        Assert.That(supplier.Region, Is.Null);
        Assert.That(supplier.PostalCode, Is.Null);
        Assert.That(supplier.Country, Is.Null);
        Assert.That(supplier.Phone, Is.Null);
        Assert.That(supplier.Fax, Is.Null);
        Assert.That(supplier.HomePage, Is.Null);
    }
}
