using System.Text.Json;
using PilotMcpServer.Models.Dto;

namespace PilotMcpServer.Tests.Models.Dto;

[TestFixture]
public class CustomerDtoTests
{
    [Test]
    public void CustomerDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "customerID": "ALFKI",
              "companyName": "Alfreds Futterkiste",
              "contactName": "Maria Anders",
              "contactTitle": "Sales Representative",
              "address": "Obere Str. 57",
              "city": "Berlin",
              "region": "Western Europe",
              "postalCode": "12209",
              "country": "Germany",
              "phone": "030-0074321",
              "fax": "030-0076545"
            }
            """;

        var customer = JsonSerializer.Deserialize<CustomerDto>(json);

        Assert.That(customer, Is.Not.Null);
        Assert.That(customer!.CustomerId, Is.EqualTo("ALFKI"));
        Assert.That(customer.CompanyName, Is.EqualTo("Alfreds Futterkiste"));
        Assert.That(customer.ContactName, Is.EqualTo("Maria Anders"));
        Assert.That(customer.ContactTitle, Is.EqualTo("Sales Representative"));
        Assert.That(customer.Address, Is.EqualTo("Obere Str. 57"));
        Assert.That(customer.City, Is.EqualTo("Berlin"));
        Assert.That(customer.Region, Is.EqualTo("Western Europe"));
        Assert.That(customer.PostalCode, Is.EqualTo("12209"));
        Assert.That(customer.Country, Is.EqualTo("Germany"));
        Assert.That(customer.Phone, Is.EqualTo("030-0074321"));
        Assert.That(customer.Fax, Is.EqualTo("030-0076545"));
    }

    [Test]
    public void CustomerDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var customer = new CustomerDto { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };

        var json = JsonSerializer.Serialize(customer);

        Assert.That(json, Does.Contain("\"customerID\":\"ALFKI\""));
        Assert.That(json, Does.Contain("\"companyName\":\"Alfreds Futterkiste\""));
    }

    [Test]
    public void CustomerDto_OptionalProperties_Unset_AreNull_Test()
    {
        var customer = new CustomerDto { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };

        Assert.That(customer.ContactName, Is.Null);
        Assert.That(customer.ContactTitle, Is.Null);
        Assert.That(customer.Address, Is.Null);
        Assert.That(customer.City, Is.Null);
        Assert.That(customer.Region, Is.Null);
        Assert.That(customer.PostalCode, Is.Null);
        Assert.That(customer.Country, Is.Null);
        Assert.That(customer.Phone, Is.Null);
        Assert.That(customer.Fax, Is.Null);
    }
}
