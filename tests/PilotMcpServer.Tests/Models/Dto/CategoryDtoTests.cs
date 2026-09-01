using System.Text.Json;
using PilotMcpServer.Models.Dto;

namespace PilotMcpServer.Tests.Models.Dto;

[TestFixture]
public class CategoryDtoTests
{
    [Test]
    public void CategoryDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """{"categoryID":1,"categoryName":"Beverages","description":"Soft drinks, coffees, teas, beers, and ales"}""";

        var category = JsonSerializer.Deserialize<CategoryDto>(json);

        Assert.That(category, Is.Not.Null);
        Assert.That(category!.CategoryId, Is.EqualTo(1));
        Assert.That(category.CategoryName, Is.EqualTo("Beverages"));
        Assert.That(category.Description, Is.EqualTo("Soft drinks, coffees, teas, beers, and ales"));
    }

    [Test]
    public void CategoryDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var category = new CategoryDto { CategoryId = 3, CategoryName = "Produce" };

        var json = JsonSerializer.Serialize(category);

        Assert.That(json, Does.Contain("\"categoryID\":3"));
        Assert.That(json, Does.Contain("\"categoryName\":\"Produce\""));
        Assert.That(json, Does.Not.Contain("\"categoryId\""));
    }

    [Test]
    public void CategoryDto_Picture_RoundTripsAsBase64EncodedBytes_Test()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var category = new CategoryDto { CategoryId = 1, CategoryName = "Beverages", Picture = bytes };

        var json = JsonSerializer.Serialize(category);
        var roundTripped = JsonSerializer.Deserialize<CategoryDto>(json);

        Assert.That(roundTripped!.Picture, Is.EqualTo(bytes));
    }

    [Test]
    public void CategoryDto_OptionalProperties_Unset_AreNull_Test()
    {
        var category = new CategoryDto { CategoryId = 1, CategoryName = "Beverages" };

        Assert.That(category.Description, Is.Null);
        Assert.That(category.Picture, Is.Null);
    }
}
