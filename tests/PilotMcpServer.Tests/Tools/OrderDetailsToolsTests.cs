using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class OrderDetailsToolsTests
{
    [Test]
    public async Task OrderDetailsTools_GetAllOrderDetailsAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrderDetailsTools(client.Object);
        var expected = new List<OrderDetailDto> { new() { OrderId = 10248, ProductId = 11, UnitPrice = 14, Quantity = 12, Discount = 0 } };
        client.Setup(c => c.GetJsonListAsync<OrderDetailDto>("/order-details/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllOrderDetailsAsync(null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task OrderDetailsTools_GetOrderDetailAsync_BuildsCompositeKeyInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrderDetailsTools(client.Object);
        var expected = new OrderDetailDto { OrderId = 10248, ProductId = 11, UnitPrice = 14, Quantity = 12, Discount = 0 };
        client.Setup(c => c.GetJsonAsync<OrderDetailDto>("/order-details/get/product/11/order/10248", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetOrderDetailAsync(11, 10248, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task OrderDetailsTools_AddOrderDetailAsync_PostsOrderDetailAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrderDetailsTools(client.Object);
        var orderDetail = new OrderDetailDto { OrderId = 10248, ProductId = 11, UnitPrice = 14, Quantity = 12, Discount = 0 };
        var expected = new AddResponse { Id = 1 };
        client.Setup(c => c.PostJsonAsync<OrderDetailDto, AddResponse>("/order-details/add", orderDetail, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddOrderDetailAsync(orderDetail, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task OrderDetailsTools_UpdateOrderDetailAsync_PutsOrderDetail_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrderDetailsTools(client.Object);
        var orderDetail = new OrderDetailDto { OrderId = 10248, ProductId = 11, UnitPrice = 15, Quantity = 12, Discount = 0.1f };
        client.Setup(c => c.PutJsonAsync("/order-details/update", orderDetail, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateOrderDetailAsync(orderDetail, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task OrderDetailsTools_DeleteOrderDetailAsync_DeletesByCompositeKey_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new OrderDetailsTools(client.Object);
        client.Setup(c => c.DeleteAsync("/order-details/delete/product/11/order/10248", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteOrderDetailAsync(11, 10248, null, CancellationToken.None);

        client.VerifyAll();
    }
}
