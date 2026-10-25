using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class CustomersToolsTests
{
    [Test]
    public async Task CustomersTools_GetAllCustomersAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CustomersTools(client.Object);
        var expected = new List<CustomerDto> { new() { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" } };
        client.Setup(c => c.GetJsonListAsync<CustomerDto>("/v1/customers/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllCustomersAsync(apiName: null, cancellationToken: CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task CustomersTools_GetCustomerAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CustomersTools(client.Object);
        var expected = new CustomerDto { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };
        client.Setup(c => c.GetJsonAsync<CustomerDto>("/v1/customers/get/ALFKI", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetCustomerAsync("ALFKI", null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task CustomersTools_AddCustomerAsync_PostsCustomerAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CustomersTools(client.Object);
        var customer = new CustomerDto { CustomerId = "NEWCO", CompanyName = "New Co" };
        var expected = new AddResponseString { Id = "NEWCO" };
        client.Setup(c => c.PostJsonAsync<CustomerDto, AddResponseString>("/v1/customers/add", customer, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddCustomerAsync(customer, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task CustomersTools_UpdateCustomerAsync_PutsCustomer_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CustomersTools(client.Object);
        var customer = new CustomerDto { CustomerId = "ALFKI", CompanyName = "Updated" };
        client.Setup(c => c.PutJsonAsync("/v1/customers/update", customer, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateCustomerAsync(customer, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task CustomersTools_DeleteCustomerAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CustomersTools(client.Object);
        client.Setup(c => c.DeleteAsync("/v1/customers/delete/ALFKI", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteCustomerAsync("ALFKI", null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task CustomersTools_GetAllCustomersAsync_WithPaging_AppendsPageAndPageSizeQuery_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CustomersTools(client.Object);
        var expected = new List<CustomerDto>();
        client.Setup(c => c.GetJsonListAsync<CustomerDto>("/v1/customers/get-all?page=2&pageSize=50", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllCustomersAsync(2, 50, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }
}
