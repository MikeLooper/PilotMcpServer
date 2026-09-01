using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class SuppliersToolsTests
{
    [Test]
    public async Task SuppliersTools_GetAllSuppliersAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new SuppliersTools(client.Object);
        var expected = new List<SupplierDto> { new() { SupplierId = 1, CompanyName = "Exotic Liquids" } };
        client.Setup(c => c.GetJsonListAsync<SupplierDto>("/v1/suppliers/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllSuppliersAsync(null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task SuppliersTools_GetSupplierAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new SuppliersTools(client.Object);
        var expected = new SupplierDto { SupplierId = 1, CompanyName = "Exotic Liquids" };
        client.Setup(c => c.GetJsonAsync<SupplierDto>("/v1/suppliers/get/1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetSupplierAsync(1, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task SuppliersTools_AddSupplierAsync_PostsSupplierAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new SuppliersTools(client.Object);
        var supplier = new SupplierDto { SupplierId = 0, CompanyName = "New Supplier" };
        var expected = new AddResponse { Id = 30 };
        client.Setup(c => c.PostJsonAsync<SupplierDto, AddResponse>("/v1/suppliers/add", supplier, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddSupplierAsync(supplier, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task SuppliersTools_UpdateSupplierAsync_PutsSupplier_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new SuppliersTools(client.Object);
        var supplier = new SupplierDto { SupplierId = 1, CompanyName = "Updated" };
        client.Setup(c => c.PutJsonAsync("/v1/suppliers/update", supplier, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateSupplierAsync(supplier, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task SuppliersTools_DeleteSupplierAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new SuppliersTools(client.Object);
        client.Setup(c => c.DeleteAsync("/v1/suppliers/delete/1", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteSupplierAsync(1, null, CancellationToken.None);

        client.VerifyAll();
    }
}
