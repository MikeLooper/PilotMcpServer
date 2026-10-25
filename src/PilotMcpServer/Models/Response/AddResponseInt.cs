using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>
/// Response returned by a Pilot API "add" endpoint whose record has a numeric identifier.
/// </summary>
public sealed class AddResponseInt
{
	/// <summary>
	/// Gets the identifier assigned to the newly created record.
	/// </summary>
	[JsonPropertyName("id")]
	[Description("The identifier assigned to the newly created record.")]
	public long Id { get; init; }
}
