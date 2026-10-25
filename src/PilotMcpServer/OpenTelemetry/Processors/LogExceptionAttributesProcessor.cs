using OpenTelemetry;
using OpenTelemetry.Logs;

namespace PilotMcpServer.OpenTelemetry.Processors;

/// <summary>
/// Adds the logged exception's message, stack trace and inner exception to the log record's attributes. Each value
/// is added only when it is available (not null or empty) and the record does not already carry that attribute
/// (compared case-insensitively, so an existing <c>exception.message</c> also counts). When nothing is added, the
/// record's attributes are left untouched.
/// </summary>
public sealed class LogExceptionAttributesProcessor : BaseProcessor<LogRecord>
{
	public override void OnEnd(LogRecord data)
	{
		var exception = data.Exception;
		if (exception is null)
		{
			return;
		}

		var attributes = data.Attributes?.ToList() ?? [];

		// Non-short-circuiting | so every attribute is considered.
		var added = TryAdd(attributes, OtelAttributeNames.ExceptionMessage, exception.Message)
			| TryAdd(attributes, OtelAttributeNames.ExceptionStacktrace, exception.StackTrace)
			| TryAdd(attributes, OtelAttributeNames.ExceptionInnerException, exception.InnerException?.ToString());

		if (added)
		{
			data.Attributes = attributes;
		}
	}

	private static bool TryAdd(List<KeyValuePair<string, object?>> attributes, string key, string? value)
	{
		if (string.IsNullOrEmpty(value) ||
			attributes.Exists(attribute => string.Equals(attribute.Key, key, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}

		attributes.Add(new KeyValuePair<string, object?>(key, value));
		return true;
	}
}
