using System.Text.Json;
using ModelContextProtocol.Server;
using Moq;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

/// <summary>
/// Verifies that the OpenAPI value constraints declared on the tool parameters and DTOs (via data annotations)
/// are published in the MCP tool input schemas, so clients see the same limits as the Pilot API contract.
/// </summary>
[TestFixture]
public class ToolInputSchemaTests
{
    [Test]
    public void ToolInputSchema_GetProduct_IdParameterHasMinimumAndMaximum_Test()
    {
        var schema = GetInputSchema<ProductsTools>(nameof(ProductsTools.GetProductAsync));

        var productId = schema.GetProperty("properties").GetProperty("productId");

        Assert.That(productId.GetProperty("minimum").GetInt64(), Is.EqualTo(1));
        Assert.That(productId.GetProperty("maximum").GetInt64(), Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void ToolInputSchema_GetCustomer_CustomerIdParameterHasFixedLength_Test()
    {
        var schema = GetInputSchema<CustomersTools>(nameof(CustomersTools.GetCustomerAsync));

        var customerId = schema.GetProperty("properties").GetProperty("customerId");

        Assert.That(customerId.GetProperty("minLength").GetInt32(), Is.EqualTo(5));
        Assert.That(customerId.GetProperty("maxLength").GetInt32(), Is.EqualTo(5));
    }

    [Test]
    public void ToolInputSchema_AddProduct_DtoPropertiesHaveLengthAndRangeConstraints_Test()
    {
        var schema = GetInputSchema<ProductsTools>(nameof(ProductsTools.AddProductAsync));

        var product = schema.GetProperty("properties").GetProperty("product").GetProperty("properties");

        Assert.That(product.GetProperty("productName").GetProperty("minLength").GetInt32(), Is.EqualTo(1));
        Assert.That(product.GetProperty("productName").GetProperty("maxLength").GetInt32(), Is.EqualTo(40));
        Assert.That(product.GetProperty("unitsInStock").GetProperty("minimum").GetInt32(), Is.EqualTo(0));
        Assert.That(product.GetProperty("unitsInStock").GetProperty("maximum").GetInt32(), Is.EqualTo(short.MaxValue));
    }

    [Test]
    public void ToolInputSchema_AddShipper_CompanyNameIsRequired_Test()
    {
        var schema = GetInputSchema<ShippersTools>(nameof(ShippersTools.AddShipperAsync));

        var required = schema.GetProperty("properties").GetProperty("shipper").GetProperty("required")
            .EnumerateArray().Select(e => e.GetString());

        Assert.That(required, Does.Contain("companyName"));
    }

    private static JsonElement GetInputSchema<TTools>(string methodName)
    {
        var tools = Activator.CreateInstance(typeof(TTools), new Mock<IPilotHttpClient>().Object)!;
        var method = typeof(TTools).GetMethod(methodName)!;

        return McpServerTool.Create(method, tools).ProtocolTool.InputSchema;
    }
}
