namespace PilotMcpServer.OpenTelemetry;

/// <summary>Names of the custom tags and attributes this server adds to OpenTelemetry spans and log records.</summary>
public static class OtelAttributeNames
{
	/// <summary>Tag added to <c>Activity.Tags</c> (trace spans) holding the current correlation ID.</summary>
	public const string CorrelationIdTag = "correlationId";

	/// <summary>Tag added to <c>Activity.Tags</c> (trace spans) holding the current MCP request's JSON-RPC ID.</summary>
	public const string RequestIdTag = "mcpRequestId";

	/// <summary>Tag added to <c>Activity.Tags</c> (trace spans) holding the name of the MCP client that made the request.</summary>
	public const string CallerNameTag = "callerName";

	/// <summary>Attribute added to <c>LogRecord.Attributes</c> holding the current correlation ID.</summary>
	public const string CorrelationIdAttribute = "CorrelationId";

	/// <summary>
	/// Attribute added to <c>LogRecord.Attributes</c> holding the current MCP request's JSON-RPC ID. Not named
	/// <c>RequestId</c>, which ASP.NET Core's hosting log scope already uses for its HTTP request identifier.
	/// </summary>
	public const string RequestIdAttribute = "McpRequestId";

	/// <summary>Attribute added to <c>LogRecord.Attributes</c> holding the name of the MCP client that made the request.</summary>
	public const string CallerNameAttribute = "CallerName";

	/// <summary>Log record attribute holding the logged exception's message.</summary>
	public const string ExceptionMessage = "Exception.Message";

	/// <summary>Log record attribute holding the logged exception's stack trace.</summary>
	public const string ExceptionStacktrace = "Exception.Stacktrace";

	/// <summary>Log record attribute holding the logged exception's inner exception, rendered with <c>ToString()</c>.</summary>
	public const string ExceptionInnerException = "Exception.InnerException";
}
