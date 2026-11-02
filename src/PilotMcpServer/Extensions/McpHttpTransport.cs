namespace PilotMcpServer.Extensions;

/// <summary>
/// Reads the opt-in <c>--http-url</c> command line argument that serves MCP over Streamable HTTP rather than stdio, for
/// HTTP-only test clients such as Bruno. The HTTP endpoint has no authentication of its own, yet its tools can change
/// data using the server's Pilot API credentials, so only loopback addresses are accepted.
/// </summary>
public static class McpHttpTransport
{
	public const string UrlArgument = "--http-url";
	public const string AllowedHostsKey = "AllowedHosts";

	/// <summary>Host names accepted in the <c>Host</c> header, which blocks DNS rebinding from a browser.</summary>
	public const string AllowedHosts = "localhost;127.0.0.1;[::1]";

	/// <summary>
	/// Returns the URL given by <c>--http-url &lt;url&gt;</c> or <c>--http-url=&lt;url&gt;</c>, or null when the argument
	/// is absent and the server should use stdio.
	/// </summary>
	/// <exception cref="ArgumentException">The value is missing, isn't an http URL, or isn't a loopback address.</exception>
	public static Uri? GetLoopbackUrl(string[] args)
	{
		for (var i = 0; i < args.Length; i++)
		{
			if (args[i] == UrlArgument)
			{
				return ParseLoopbackUrl(i + 1 < args.Length ? args[i + 1] : null);
			}

			if (args[i].StartsWith($"{UrlArgument}=", StringComparison.Ordinal))
			{
				return ParseLoopbackUrl(args[i][(UrlArgument.Length + 1)..]);
			}
		}

		return null;
	}

	private static Uri ParseLoopbackUrl(string? value)
	{
		if (Uri.TryCreate(value, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttp && url.IsLoopback)
		{
			return url;
		}

		throw new ArgumentException($"{UrlArgument} must be an http URL on a loopback address, e.g. http://localhost:11901. Received: \"{value}\".");
	}
}
