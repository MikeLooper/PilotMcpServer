using System.Diagnostics;
using PilotMcpServer.Models;

namespace PilotMcpServer.OpenTelemetry;

/// <summary>Maps <see cref="RequestSettings"/> onto the span tags and log record attributes named in <see cref="OtelAttributeNames"/>.</summary>
public static class RequestSettingsTelemetry
{
	/// <summary>Tags <paramref name="activity"/> with the correlation ID and, when known, the request ID and caller name.</summary>
	public static Activity SetRequestSettingsTags(this Activity activity, RequestSettings requestSettings)
	{
		activity.SetTag(OtelAttributeNames.CorrelationIdTag, requestSettings.CorrelationId);

		if (requestSettings.RequestId is not null)
		{
			activity.SetTag(OtelAttributeNames.RequestIdTag, requestSettings.RequestId);
		}

		if (requestSettings.CallerName is not null)
		{
			activity.SetTag(OtelAttributeNames.CallerNameTag, requestSettings.CallerName);
		}

		return activity;
	}

	/// <summary>Returns the log record attributes for the correlation ID and, when known, the request ID and caller name.</summary>
	public static IEnumerable<KeyValuePair<string, object?>> GetLogAttributes(this RequestSettings requestSettings)
	{
		yield return new(OtelAttributeNames.CorrelationIdAttribute, requestSettings.CorrelationId);

		if (requestSettings.RequestId is not null)
		{
			yield return new(OtelAttributeNames.RequestIdAttribute, requestSettings.RequestId);
		}

		if (requestSettings.CallerName is not null)
		{
			yield return new(OtelAttributeNames.CallerNameAttribute, requestSettings.CallerName);
		}
	}
}
