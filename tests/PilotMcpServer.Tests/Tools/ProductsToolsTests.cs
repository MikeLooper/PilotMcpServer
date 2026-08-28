using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class ProductsToolsTests
{
    [Test]
    public async Task ProductsTools_GetAllProductsAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ProductsTools(client.Object);
        var expected = new List<ProductDto> { new() { ProductId = 1, ProductName = "Chai" } };
        client.Setup(c => c.GetJsonListAsync<ProductDto>("/v1/products/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllProductsAsync(null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task ProductsTools_GetProductAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ProductsTools(client.Object);
        var expected = new ProductDto { ProductId = 1, ProductName = "Chai" };
        client.Setup(c => c.GetJsonAsync<ProductDto>("/v1/products/get/1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetProductAsync(1, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task ProductsTools_AddProductAsync_PostsProductAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ProductsTools(client.Object);
        var product = new ProductDto { ProductId = 0, ProductName = "New Product" };
        var expected = new AddResponse { Id = 78 };
        client.Setup(c => c.PostJsonAsync<ProductDto, AddResponse>("/v1/products/add", product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddProductAsync(product, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task ProductsTools_UpdateProductAsync_PutsProduct_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ProductsTools(client.Object);
        var product = new ProductDto { ProductId = 1, ProductName = "Updated" };
        client.Setup(c => c.PutJsonAsync("/v1/products/update", product, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateProductAsync(product, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task ProductsTools_DeleteProductAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ProductsTools(client.Object);
        client.Setup(c => c.DeleteAsync("/v1/products/delete/1", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteProductAsync(1, null, CancellationToken.None);

        client.VerifyAll();
    }
}
