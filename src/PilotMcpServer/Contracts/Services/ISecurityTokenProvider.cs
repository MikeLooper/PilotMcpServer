namespace PilotMcpServer.Contracts.Services;

/// <summary>
/// Retrieves and caches a bearer token from the configured identity provider for use against domain Pilot API
/// endpoints. When security is not active (see <c>SecurityOptions.Active</c>), <see cref="GetTokenAsync"/>
/// returns <see langword="null"/> and no token is applied to outgoing requests.
/// </summary>
public interface ISecurityTokenProvider
{
	/// <summary>Returns a valid bearer token, fetching or refreshing it as needed, or <see langword="null"/> when security is inactive.</summary>
	Task<string?> GetTokenAsync(CancellationToken cancellationToken);
}
