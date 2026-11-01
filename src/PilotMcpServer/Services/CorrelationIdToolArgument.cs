using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Server;

namespace PilotMcpServer.Services;

/// <summary>
/// Declares the optional <c>correlationId</c> argument in a tool's input schema, so agents and skills, which can only
/// pass arguments their tool schema lists, can supply it. The tool methods don't take it as a parameter:
/// <see cref="RequestSettingsMessageFilter"/> reads it from the request, and the SDK ignores it when binding arguments.
/// </summary>
public static class CorrelationIdToolArgument
{
	private const string Description = "Optional. GUID that ties this call to an existing trace or workflow, used as its correlation ID. Pass one only if you were given it; never invent one. Omit it to have one generated.";

	public static void AddToInputSchema(McpServerTool tool)
	{
		var schema = JsonNode.Parse(tool.ProtocolTool.InputSchema.GetRawText())!.AsObject();

		if (schema["properties"] is not JsonObject properties)
		{
			properties = [];
			schema["properties"] = properties;
		}

		properties[RequestSettingsMessageFilter.CorrelationIdKey] = new JsonObject
		{
			["description"] = Description,
			["type"] = "string",
			["format"] = "uuid",
		};

		tool.ProtocolTool.InputSchema = JsonSerializer.SerializeToElement(schema);
	}
}
