namespace PilotMcpServer.Configuration;

/// <summary>
/// Configuration for obtaining and applying a bearer token to domain Pilot API requests (not <c>/about</c> or
/// <c>/healthcheck</c>). Bound from the "Security" configuration section.
/// </summary>
public sealed class SecurityOptions
{
	public const string SectionName = "Security";

	/// <summary>When <see langword="false"/>, token retrieval and application to outgoing requests is disabled entirely.</summary>
	public bool Active { get; set; }

	/// <summary>Base URL of the identity provider host used to obtain a bearer token.</summary>
	public string IdpHostBaseUrl { get; set; } = string.Empty;

	/// <summary>The identity provider realm.</summary>
	public string IdpHostRealm { get; set; } = string.Empty;

	/// <summary>The identity provider client ID.</summary>
	public string IdpHostClientId { get; set; } = string.Empty;

	/// <summary>The resource owner username used for the password credentials grant.</summary>
	public string IdpHostUsername { get; set; } = string.Empty;

	/// <summary>The resource owner password used for the password credentials grant.</summary>
	public string IdpHostPassword { get; set; } = string.Empty;

	public string TokenUrl => $"{IdpHostBaseUrl}/realms/{IdpHostRealm}/protocol/openid-connect/token";
}
