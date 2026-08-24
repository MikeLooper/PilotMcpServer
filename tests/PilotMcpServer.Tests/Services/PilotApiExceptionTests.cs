using System.Net;
using PilotMcpServer.Models;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class PilotApiExceptionTests
{
    [Test]
    public void PilotApiException_Constructor_WithProblemDetail_UsesDetailInMessage_Test()
    {
        var problem = new ProblemDetailsResponse { Title = "Invalid", Detail = "categoryId must be positive" };

        var exception = new PilotApiException(HttpStatusCode.BadRequest, problem);

        Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(exception.Problem, Is.SameAs(problem));
        Assert.That(exception.Message, Does.Contain("400"));
        Assert.That(exception.Message, Does.Contain("categoryId must be positive"));
    }

    [Test]
    public void PilotApiException_Constructor_WithProblemTitleOnly_UsesTitleInMessage_Test()
    {
        var problem = new ProblemDetailsResponse { Title = "Invalid request" };

        var exception = new PilotApiException(HttpStatusCode.BadRequest, problem);

        Assert.That(exception.Message, Does.Contain("Invalid request"));
    }

    [Test]
    public void PilotApiException_Constructor_WithNullProblem_UsesGenericMessage_Test()
    {
        var exception = new PilotApiException(HttpStatusCode.InternalServerError, null);

        Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(exception.Problem, Is.Null);
        Assert.That(exception.Message, Does.Contain("500"));
        Assert.That(exception.Message, Does.Contain("InternalServerError"));
    }
}
