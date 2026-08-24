using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>Response returned by a Pilot API "add" endpoint after creating a record.</summary>
public sealed class AddResponse
{
    [JsonPropertyName("id")]
    [Description("The identifier assigned to the newly created record.")]
    public long Id { get; init; }
}
