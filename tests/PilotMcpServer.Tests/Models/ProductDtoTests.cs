using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class ProductDtoTests
{
    [Test]
    public void ProductDto_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "productID": 1,
              "productName": "Chai",
              "supplierID": 1,
              "categoryID": 1,
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
    public void ProductDto_Serialize_UsesApiPropertyNamesNotCamelCase_Test()
    {
        var product = new ProductDto { ProductId = 1, ProductName = "Chai", Discontinued = true };

        var json = JsonSerializer.Serialize(product);

        Assert.That(json, Does.Contain("\"productID\":1"));
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
        Assert.That(product.UnitsInStock, Is.EqualTo((short)0));
        Assert.That(product.UnitsOnOrder, Is.EqualTo((short)0));
        Assert.That(product.ReorderLevel, Is.EqualTo((short)0));
        Assert.That(product.Discontinued, Is.False);
    }
}
