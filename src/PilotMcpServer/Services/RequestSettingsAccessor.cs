using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;

namespace PilotMcpServer.Services;

/// <summary>
/// <see cref="IRequestSettingsAccessor"/> backed by an <see cref="AsyncLocal{T}"/>. The storage is static, so every
/// instance sees the same value for a given execution context (the same approach as ASP.NET Core's
/// <c>HttpContextAccessor</c>).
/// </summary>
public sealed class RequestSettingsAccessor : IRequestSettingsAccessor
{
	private static readonly AsyncLocal<RequestSettings?> CurrentSettings = new();

	public RequestSettings? Current
	{
		get => CurrentSettings.Value;
		set => CurrentSettings.Value = value;
	}
}
