using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models.Response;

namespace PilotMcpServer.Services;

/// <summary>
/// Obtains bearer tokens from the configured identity provider using the resource owner password credentials
/// grant, caching the token until shortly before it expires. Returns <see langword="null"/> when
/// <see cref="SecurityOptions.Active"/> is <see langword="false"/>.
/// </summary>
public sealed class SecurityTokenProvider(HttpClient httpClient, IOptions<SecurityOptions> options, ILogger<SecurityTokenProvider> logger) : ISecurityTokenProvider
{
	private readonly SemaphoreSlim _lock = new(1, 1);
	private string? _cachedToken;
	private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

	public async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
	{
		var settings = options.Value;
		if (!settings.Active)
		{
			return null;
		}

		if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt)
		{
			return _cachedToken;
		}

		await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt)
			{
				return _cachedToken;
			}

			var request = new HttpRequestMessage(HttpMethod.Post, settings.TokenUrl)
			{
				Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["grant_type"] = "password",
					["client_id"] = settings.IdpHostClientId,
					["username"] = settings.IdpHostUsername,
					["password"] = settings.IdpHostPassword,
				})
			};

			using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var token = await response.Content.ReadFromJsonAsync<SecurityTokenResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
			if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
			{
				logger.LogWarning("Security token endpoint returned an empty token.");
				return null;
			}

			_cachedToken = token.AccessToken;
			// Refresh a bit early to avoid using a token that expires mid-flight.
			var lifetime = token.ExpiresIn > 0 ? token.ExpiresIn : 60;
			_expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(lifetime - 10, 5));

			return _cachedToken;
		}
		finally
		{
			_lock.Release();
		}
	}
}
