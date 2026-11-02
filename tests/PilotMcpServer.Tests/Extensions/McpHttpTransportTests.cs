using PilotMcpServer.Extensions;

namespace PilotMcpServer.Tests.Extensions;

[TestFixture]
public class McpHttpTransportTests
{
	[Test]
	public void McpHttpTransport_GetLoopbackUrl_ReturnsNullWhenArgumentIsAbsent_Test()
	{
		var result = McpHttpTransport.GetLoopbackUrl(["--other", "value"]);

		Assert.That(result, Is.Null);
	}

	[TestCase("http://localhost:11601")]
	[TestCase("http://127.0.0.1:11601")]
	[TestCase("http://[::1]:11601")]
	public void McpHttpTransport_GetLoopbackUrl_ReturnsUrlFollowingArgument_Test(string url)
	{
		var result = McpHttpTransport.GetLoopbackUrl(["--http-url", url]);

		Assert.That(result, Is.EqualTo(new Uri(url)));
	}

	[Test]
	public void McpHttpTransport_GetLoopbackUrl_ReturnsUrlFromEqualsForm_Test()
	{
		var result = McpHttpTransport.GetLoopbackUrl(["--http-url=http://localhost:11601"]);

		Assert.That(result, Is.EqualTo(new Uri("http://localhost:11601")));
	}

	[TestCase("http://0.0.0.0:11601")]
	[TestCase("http://example.com:11601")]
	[TestCase("https://localhost:11601")]
	[TestCase("not a url")]
	public void McpHttpTransport_GetLoopbackUrl_ThrowsForNonLoopbackHttpUrl_Test(string url)
	{
		Assert.That(() => McpHttpTransport.GetLoopbackUrl(["--http-url", url]), Throws.ArgumentException);
	}

	[Test]
	public void McpHttpTransport_GetLoopbackUrl_ThrowsWhenValueIsMissing_Test()
	{
		Assert.That(() => McpHttpTransport.GetLoopbackUrl(["--http-url"]), Throws.ArgumentException);
	}
}
