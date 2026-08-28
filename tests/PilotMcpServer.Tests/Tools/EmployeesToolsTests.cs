using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class EmployeesToolsTests
{
    [Test]
    public async Task EmployeesTools_GetAllEmployeesAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new EmployeesTools(client.Object);
        var expected = new List<EmployeeDto> { new() { EmployeeId = 1, FirstName = "Nancy", LastName = "Davolio" } };
        client.Setup(c => c.GetJsonListAsync<EmployeeDto>("/v1/employees/get-all", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllEmployeesAsync(null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task EmployeesTools_GetEmployeeAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new EmployeesTools(client.Object);
        var expected = new EmployeeDto { EmployeeId = 2, FirstName = "Andrew", LastName = "Fuller" };
        client.Setup(c => c.GetJsonAsync<EmployeeDto>("/v1/employees/get/2", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetEmployeeAsync(2, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task EmployeesTools_AddEmployeeAsync_PostsEmployeeAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new EmployeesTools(client.Object);
        var employee = new EmployeeDto { EmployeeId = 0, FirstName = "New", LastName = "Hire" };
        var expected = new AddResponse { Id = 9 };
        client.Setup(c => c.PostJsonAsync<EmployeeDto, AddResponse>("/v1/employees/add", employee, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddEmployeeAsync(employee, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task EmployeesTools_UpdateEmployeeAsync_PutsEmployee_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new EmployeesTools(client.Object);
        var employee = new EmployeeDto { EmployeeId = 1, FirstName = "Nancy", LastName = "Updated" };
        client.Setup(c => c.PutJsonAsync("/v1/employees/update", employee, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateEmployeeAsync(employee, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task EmployeesTools_DeleteEmployeeAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new EmployeesTools(client.Object);
        client.Setup(c => c.DeleteAsync("/v1/employees/delete/4", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteEmployeeAsync(4, null, CancellationToken.None);

        client.VerifyAll();
    }
}
