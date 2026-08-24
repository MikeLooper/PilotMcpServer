using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class EmployeeDtoTests
{
    [Test]
    public void EmployeeDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "employeeID": 1,
              "firstName": "Nancy",
              "lastName": "Davolio",
              "title": "Sales Representative",
              "titleOfCourtesy": "Ms.",
              "birthDate": "1968-12-08T00:00:00",
              "hireDate": "1992-05-01T00:00:00",
              "address": "507 - 20th Ave. E. Apt. 2A",
              "city": "Seattle",
              "region": "North America",
              "postalCode": "98122",
              "country": "USA",
              "homePhone": "(206) 555-9857",
              "extension": "5467",
              "photoPath": "http://example.com/photos/1.bmp",
              "notes": "Education includes a BA.",
              "reportsTo": 2
            }
            """;

        var employee = JsonSerializer.Deserialize<EmployeeDto>(json);

        Assert.That(employee, Is.Not.Null);
        Assert.That(employee!.EmployeeId, Is.EqualTo(1));
        Assert.That(employee.FirstName, Is.EqualTo("Nancy"));
        Assert.That(employee.LastName, Is.EqualTo("Davolio"));
        Assert.That(employee.Title, Is.EqualTo("Sales Representative"));
        Assert.That(employee.TitleOfCourtesy, Is.EqualTo("Ms."));
        Assert.That(employee.BirthDate, Is.EqualTo(new DateTime(1968, 12, 8)));
        Assert.That(employee.HireDate, Is.EqualTo(new DateTime(1992, 5, 1)));
        Assert.That(employee.Address, Is.EqualTo("507 - 20th Ave. E. Apt. 2A"));
        Assert.That(employee.City, Is.EqualTo("Seattle"));
        Assert.That(employee.Region, Is.EqualTo("North America"));
        Assert.That(employee.PostalCode, Is.EqualTo("98122"));
        Assert.That(employee.Country, Is.EqualTo("USA"));
        Assert.That(employee.HomePhone, Is.EqualTo("(206) 555-9857"));
        Assert.That(employee.Extension, Is.EqualTo("5467"));
        Assert.That(employee.PhotoPath, Is.EqualTo("http://example.com/photos/1.bmp"));
        Assert.That(employee.Notes, Is.EqualTo("Education includes a BA."));
        Assert.That(employee.ReportsTo, Is.EqualTo(2));
    }

    [Test]
    public void EmployeeDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var employee = new EmployeeDto { EmployeeId = 1, FirstName = "Nancy", LastName = "Davolio" };

        var json = JsonSerializer.Serialize(employee);

        Assert.That(json, Does.Contain("\"employeeID\":1"));
        Assert.That(json, Does.Contain("\"firstName\":\"Nancy\""));
        Assert.That(json, Does.Contain("\"lastName\":\"Davolio\""));
    }

    [Test]
    public void EmployeeDto_Photo_RoundTripsAsBase64EncodedBytes_Test()
    {
        var bytes = new byte[] { 5, 6, 7, 8 };
        var employee = new EmployeeDto { EmployeeId = 1, FirstName = "Nancy", LastName = "Davolio", Photo = bytes };

        var json = JsonSerializer.Serialize(employee);
        var roundTripped = JsonSerializer.Deserialize<EmployeeDto>(json);

        Assert.That(roundTripped!.Photo, Is.EqualTo(bytes));
    }

    [Test]
    public void EmployeeDto_OptionalProperties_Unset_AreNull_Test()
    {
        var employee = new EmployeeDto { EmployeeId = 1, FirstName = "Nancy", LastName = "Davolio" };

        Assert.That(employee.Title, Is.Null);
        Assert.That(employee.TitleOfCourtesy, Is.Null);
        Assert.That(employee.BirthDate, Is.Null);
        Assert.That(employee.HireDate, Is.Null);
        Assert.That(employee.Address, Is.Null);
        Assert.That(employee.City, Is.Null);
        Assert.That(employee.Region, Is.Null);
        Assert.That(employee.PostalCode, Is.Null);
        Assert.That(employee.Country, Is.Null);
        Assert.That(employee.HomePhone, Is.Null);
        Assert.That(employee.Extension, Is.Null);
        Assert.That(employee.Photo, Is.Null);
        Assert.That(employee.PhotoPath, Is.Null);
        Assert.That(employee.Notes, Is.Null);
        Assert.That(employee.ReportsTo, Is.Null);
    }
}
