using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class ShippersToolsTests
{
    [Test]
    public async Task ShippersTools_GetAllShippersAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ShippersTools(client.Object);
        var expected = new List<ShipperDto> { new() { ShipperId = 1, CompanyName = "Speedy Express" } };
        client.Setup(c => c.GetJsonListAsync<ShipperDto>("/shippers/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllShippersAsync(null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task ShippersTools_GetShipperAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ShippersTools(client.Object);
        var expected = new ShipperDto { ShipperId = 1, CompanyName = "Speedy Express" };
        client.Setup(c => c.GetJsonAsync<ShipperDto>("/shippers/get/1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetShipperAsync(1, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task ShippersTools_AddShipperAsync_PostsShipperAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ShippersTools(client.Object);
        var shipper = new ShipperDto { ShipperId = 0, CompanyName = "New Shipper" };
        var expected = new AddResponse { Id = 4 };
        client.Setup(c => c.PostJsonAsync<ShipperDto, AddResponse>("/shippers/add", shipper, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddShipperAsync(shipper, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task ShippersTools_UpdateShipperAsync_PutsShipper_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ShippersTools(client.Object);
        var shipper = new ShipperDto { ShipperId = 1, CompanyName = "Updated" };
        client.Setup(c => c.PutJsonAsync("/shippers/update", shipper, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateShipperAsync(shipper, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task ShippersTools_DeleteShipperAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new ShippersTools(client.Object);
        client.Setup(c => c.DeleteAsync("/shippers/delete/1", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteShipperAsync(1, null, CancellationToken.None);

        client.VerifyAll();
    }
}
