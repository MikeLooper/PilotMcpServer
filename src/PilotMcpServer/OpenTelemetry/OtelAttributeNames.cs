namespace PilotMcpServer.OpenTelemetry;

/// <summary>Names of the custom tags and attributes this server adds to OpenTelemetry spans and log records.</summary>
public static class OtelAttributeNames
{
	/// <summary>Tag added to <c>Activity.Tags</c> (trace spans) holding the current correlation ID.</summary>
	public const string CorrelationIdTag = "correlationId";

	/// <summary>Attribute added to <c>LogRecord.Attributes</c> holding the current correlation ID.</summary>
	public const string CorrelationIdAttribute = "CorrelationId";

	/// <summary>Log record attribute holding the logged exception's message.</summary>
	public const string ExceptionMessage = "Exception.Message";

	/// <summary>Log record attribute holding the logged exception's stack trace.</summary>
	public const string ExceptionStacktrace = "Exception.Stacktrace";

	/// <summary>Log record attribute holding the logged exception's inner exception, rendered with <c>ToString()</c>.</summary>
	public const string ExceptionInnerException = "Exception.InnerException";
}
