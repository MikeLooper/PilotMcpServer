using Moq;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Tools;

[TestFixture]
public class CategoriesToolsTests
{
    [Test]
    public async Task CategoriesTools_GetAllCategoriesAsync_DelegatesToClientAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CategoriesTools(client.Object);
        var expected = new List<CategoryDto> { new() { CategoryId = 1, CategoryName = "Beverages" } };
        client.Setup(c => c.GetJsonListAsync<CategoryDto>("/v1/categories/get-all", "custom-api", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetAllCategoriesAsync("custom-api", CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task CategoriesTools_GetCategoryAsync_BuildsIdInPathAndReturnsResult_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CategoriesTools(client.Object);
        var expected = new CategoryDto { CategoryId = 5, CategoryName = "Produce" };
        client.Setup(c => c.GetJsonAsync<CategoryDto>("/v1/categories/get/5", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.GetCategoryAsync(5, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task CategoriesTools_GetCategoryAsync_ClientReturnsNull_ReturnsNull_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CategoriesTools(client.Object);
        client.Setup(c => c.GetJsonAsync<CategoryDto>("/v1/categories/get/999", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDto?)null);

        var result = await tools.GetCategoryAsync(999, null, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CategoriesTools_AddCategoryAsync_PostsCategoryAndReturnsAddResponse_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CategoriesTools(client.Object);
        var category = new CategoryDto { CategoryId = 0, CategoryName = "Snacks" };
        var expected = new AddResponse { Id = 10 };
        client.Setup(c => c.PostJsonAsync<CategoryDto, AddResponse>("/v1/categories/add", category, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await tools.AddCategoryAsync(category, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        client.VerifyAll();
    }

    [Test]
    public async Task CategoriesTools_UpdateCategoryAsync_PutsCategory_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CategoriesTools(client.Object);
        var category = new CategoryDto { CategoryId = 3, CategoryName = "Updated" };
        client.Setup(c => c.PutJsonAsync("/v1/categories/update", category, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.UpdateCategoryAsync(category, null, CancellationToken.None);

        client.VerifyAll();
    }

    [Test]
    public async Task CategoriesTools_DeleteCategoryAsync_DeletesById_Test()
    {
        var client = new Mock<IPilotHttpClient>(MockBehavior.Strict);
        var tools = new CategoriesTools(client.Object);
        client.Setup(c => c.DeleteAsync("/v1/categories/delete/8", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await tools.DeleteCategoryAsync(8, null, CancellationToken.None);

        client.VerifyAll();
    }
}
