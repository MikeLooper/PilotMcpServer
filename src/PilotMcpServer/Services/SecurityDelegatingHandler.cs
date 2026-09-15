using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.Services;

/// <summary>
/// Applies a bearer token, obtained from <see cref="ISecurityTokenProvider"/>, to the Authorization header of
/// outgoing requests. Skips <c>/about</c> and <c>/healthcheck</c>, which remain unauthenticated, and does
/// nothing when security is not active (the token provider returns <see langword="null"/> in that case).
/// </summary>
public sealed class SecurityDelegatingHandler(ISecurityTokenProvider tokenProvider) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var path = request.RequestUri?.AbsolutePath ?? string.Empty;
		if (!path.EndsWith("/about", StringComparison.OrdinalIgnoreCase) &&
			!path.EndsWith("/healthcheck", StringComparison.OrdinalIgnoreCase))
		{
			var token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(token))
			{
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
			}
		}

		return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
	}
}
