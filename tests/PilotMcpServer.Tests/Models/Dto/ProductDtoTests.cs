using System.Text.Json;
using PilotMcpServer.Models.Dto;

namespace PilotMcpServer.Tests.Models.Dto;

[TestFixture]
public class ProductDtoTests
{
    [Test]
    public void ProductDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "productId": 1,
              "productName": "Chai",
              "supplierId": 1,
              "categoryId": 1,
              "quantityPerUnit": "10 boxes x 20 bags",
              "unitPrice": 18,
              "unitsInStock": 39,
              "unitsOnOrder": 0,
              "reorderLevel": 10,
              "discontinued": false
            }
            """;

        var product = JsonSerializer.Deserialize<ProductDto>(json);

        Assert.That(product, Is.Not.Null);
        Assert.That(product!.ProductId, Is.EqualTo(1));
        Assert.That(product.ProductName, Is.EqualTo("Chai"));
        Assert.That(product.SupplierId, Is.EqualTo(1));
        Assert.That(product.CategoryId, Is.EqualTo(1));
        Assert.That(product.QuantityPerUnit, Is.EqualTo("10 boxes x 20 bags"));
        Assert.That(product.UnitPrice, Is.EqualTo(18));
        Assert.That(product.UnitsInStock, Is.EqualTo((short)39));
        Assert.That(product.UnitsOnOrder, Is.EqualTo((short)0));
        Assert.That(product.ReorderLevel, Is.EqualTo((short)10));
        Assert.That(product.Discontinued, Is.False);
    }

    [Test]
    public void ProductDto_Serialize_UsesOpenApiPropertyNames_Test()
    {
        var product = new ProductDto { ProductId = 1, ProductName = "Chai", Discontinued = true };

        var json = JsonSerializer.Serialize(product);

        Assert.That(json, Does.Contain("\"productId\":1"));
        Assert.That(json, Does.Contain("\"productName\":\"Chai\""));
        Assert.That(json, Does.Contain("\"discontinued\":true"));
    }

    [Test]
    public void ProductDto_OptionalProperties_Unset_HaveDefaultValues_Test()
    {
        var product = new ProductDto { ProductId = 1, ProductName = "Chai" };

        Assert.That(product.SupplierId, Is.Null);
        Assert.That(product.CategoryId, Is.Null);
        Assert.That(product.QuantityPerUnit, Is.Null);
        Assert.That(product.UnitPrice, Is.Null);
        Assert.That(product.UnitsInStock, Is.Null);
        Assert.That(product.UnitsOnOrder, Is.Null);
        Assert.That(product.ReorderLevel, Is.Null);
        Assert.That(product.Discontinued, Is.False);
    }

    [Test]
    public void ProductDto_Deserialize_WithoutId_LeavesIdNull_Test()
    {
        const string json = """{"productName":"Chai"}""";

        var dto = JsonSerializer.Deserialize<ProductDto>(json);

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.ProductId, Is.Null);
    }

    [Test]
    public void ProductDto_Serialize_WithoutId_WritesNullId_Test()
    {
        var dto = new ProductDto { ProductName = "Chai" };

        var json = JsonSerializer.Serialize(dto);

        Assert.That(json, Does.Contain("\"productId\":null"));
    }

    [Test]
    public void ProductDto_Serialize_UnsetStockLevels_OmitsDefaultedValues_Test()
    {
        var product = new ProductDto { ProductName = "Chai" };

        var json = JsonSerializer.Serialize(product);

        Assert.That(json, Does.Not.Contain("\"unitsInStock\""));
        Assert.That(json, Does.Not.Contain("\"unitsOnOrder\""));
        Assert.That(json, Does.Not.Contain("\"reorderLevel\""));
    }

    [Test]
    public void ProductDto_Deserialize_NullStockLevels_MapsToNull_Test()
    {
        const string json = """{"productId":1,"productName":"Chai","unitsInStock":null,"unitsOnOrder":null,"reorderLevel":null}""";

        var product = JsonSerializer.Deserialize<ProductDto>(json);

        Assert.That(product, Is.Not.Null);
        Assert.That(product!.UnitsInStock, Is.Null);
        Assert.That(product.UnitsOnOrder, Is.Null);
        Assert.That(product.ReorderLevel, Is.Null);
    }
}
