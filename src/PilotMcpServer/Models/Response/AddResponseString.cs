using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>
/// Response returned by a Pilot API "add" endpoint whose record has a string identifier (e.g. Customers).
/// </summary>
public sealed class AddResponseString
{
	/// <summary>
	/// Gets the identifier assigned to the newly created record.
	/// </summary>
	[JsonPropertyName("id")]
	[Description("The identifier assigned to the newly created record.")]
	public string? Id { get; init; }
}
