using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PilotMcpServer.Configuration;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class SecurityTokenProviderTests
{
	[Test]
	public async Task SecurityTokenProvider_GetTokenAsync_SecurityNotActive_ReturnsNull_Test()
	{
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"access_token\":\"abc123\"}");
		var httpClient = new HttpClient(handler);
		var options = Options.Create(new SecurityOptions { Active = false });
		var provider = new SecurityTokenProvider(httpClient, options, NullLogger<SecurityTokenProvider>.Instance);

		var token = await provider.GetTokenAsync(CancellationToken.None);

		Assert.That(token, Is.Null);
	}

	[Test]
	public async Task SecurityTokenProvider_GetTokenAsync_SecurityActive_ParsesSnakeCaseAccessToken_Test()
	{
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"access_token\":\"abc123\",\"expires_in\":300,\"token_type\":\"Bearer\"}");
		var httpClient = new HttpClient(handler);
		var options = Options.Create(new SecurityOptions
		{
			Active = true,
			IdpHostBaseUrl = "http://localhost:55001",
			IdpHostRealm = "local-realm",
			IdpHostClientId = "local-client",
			IdpHostUsername = "working_admin_user",
			IdpHostPassword = "Chf-894RZWmv",
		});
		var provider = new SecurityTokenProvider(httpClient, options, NullLogger<SecurityTokenProvider>.Instance);

		var token = await provider.GetTokenAsync(CancellationToken.None);

		Assert.That(token, Is.EqualTo("abc123"));
	}

	[Test]
	public async Task SecurityTokenProvider_GetTokenAsync_SecurityActive_CachesTokenAcrossCalls_Test()
	{
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"access_token\":\"abc123\",\"expires_in\":300}");
		var httpClient = new HttpClient(handler);
		var options = Options.Create(new SecurityOptions
		{
			Active = true,
			IdpHostBaseUrl = "http://localhost:55001",
			IdpHostRealm = "local-realm",
			IdpHostClientId = "local-client",
			IdpHostUsername = "working_admin_user",
			IdpHostPassword = "Chf-894RZWmv",
		});
		var provider = new SecurityTokenProvider(httpClient, options, NullLogger<SecurityTokenProvider>.Instance);

		var first = await provider.GetTokenAsync(CancellationToken.None);
		var firstRequest = handler.LastRequest;
		var second = await provider.GetTokenAsync(CancellationToken.None);

		Assert.That(second, Is.EqualTo(first));
		Assert.That(handler.LastRequest, Is.SameAs(firstRequest));
	}

	[Test]
	public async Task SecurityTokenProvider_GetTokenAsync_SecurityActive_EmptyAccessToken_ReturnsNull_Test()
	{
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"access_token\":\"\"}");
		var httpClient = new HttpClient(handler);
		var options = Options.Create(new SecurityOptions
		{
			Active = true,
			IdpHostBaseUrl = "http://localhost:55001",
			IdpHostRealm = "local-realm",
			IdpHostClientId = "local-client",
			IdpHostUsername = "working_admin_user",
			IdpHostPassword = "Chf-894RZWmv",
		});
		var provider = new SecurityTokenProvider(httpClient, options, NullLogger<SecurityTokenProvider>.Instance);

		var token = await provider.GetTokenAsync(CancellationToken.None);

		Assert.That(token, Is.Null);
	}

	[Test]
	public async Task SecurityTokenProvider_GetTokenAsync_SecurityActive_SendsClientIdAndCredentialsInBody_Test()
	{
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"access_token\":\"abc123\",\"expires_in\":300}");
		var httpClient = new HttpClient(handler);
		var options = Options.Create(new SecurityOptions
		{
			Active = true,
			IdpHostBaseUrl = "http://localhost:55001",
			IdpHostRealm = "local-realm",
			IdpHostClientId = "local-client",
			IdpHostUsername = "working_admin_user",
			IdpHostPassword = "Chf-894RZWmv",
		});
		var provider = new SecurityTokenProvider(httpClient, options, NullLogger<SecurityTokenProvider>.Instance);

		await provider.GetTokenAsync(CancellationToken.None);

		Assert.That(handler.LastRequestBody, Does.Contain("client_id=local-client"));
		Assert.That(handler.LastRequestBody, Does.Contain("username=working_admin_user"));
		Assert.That(handler.LastRequest?.RequestUri?.ToString(), Is.EqualTo("http://localhost:55001/realms/local-realm/protocol/openid-connect/token"));
	}
}
