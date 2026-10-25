using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class OrdersToolsTests
{
    [Test]
    public async Task OrdersTools_GetAllOrdersAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrdersTools(client.Object);
        var expected = new List<OrderDto> { new() { OrderId = 1 } };
        client.Setup(c => c.GetJsonListAsync<OrderDto>("/v1/orders/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllOrdersAsync(apiName: null, cancellationToken: CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task OrdersTools_GetOrderAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrdersTools(client.Object);
        var expected = new OrderDto { OrderId = 10248 };
        client.Setup(c => c.GetJsonAsync<OrderDto>("/v1/orders/get/10248", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetOrderAsync(10248, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task OrdersTools_AddOrderAsync_PostsOrderAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrdersTools(client.Object);
        var order = new OrderDto { OrderId = 0 };
        var expected = new AddResponseInt { Id = 11077 };
        client.Setup(c => c.PostJsonAsync<OrderDto, AddResponseInt>("/v1/orders/add", order, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddOrderAsync(order, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task OrdersTools_UpdateOrderAsync_PutsOrder_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrdersTools(client.Object);
        var order = new OrderDto { OrderId = 10248, ShipCity = "Berlin" };
        client.Setup(c => c.PutJsonAsync("/v1/orders/update", order, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateOrderAsync(order, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task OrdersTools_DeleteOrderAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrdersTools(client.Object);
        client.Setup(c => c.DeleteAsync("/v1/orders/delete/10248", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteOrderAsync(10248, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task OrdersTools_GetAllOrdersAsync_WithPaging_AppendsPageAndPageSizeQuery_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrdersTools(client.Object);
        var expected = new List<OrderDto>();
        client.Setup(c => c.GetJsonListAsync<OrderDto>("/v1/orders/get-all?page=2&pageSize=50", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllOrdersAsync(2, 50, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }
}
