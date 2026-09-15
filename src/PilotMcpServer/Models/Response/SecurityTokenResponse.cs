using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Response;

/// <summary>Response payload from the identity provider's token endpoint.</summary>
public sealed class SecurityTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = string.Empty;

	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; set; }

	[JsonPropertyName("token_type")]
	public string TokenType { get; set; } = string.Empty;
}

