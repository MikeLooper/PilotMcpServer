using PilotMcpServer.Contracts.Services;

namespace PilotMcpServer.IntegrationTests.Testing.Doubles;

/// <summary>Token provider that always returns the same token, simulating an active security configuration.</summary>
public sealed class StaticSecurityTokenProvider(string token) : ISecurityTokenProvider
{
    public Task<string?> GetTokenAsync(CancellationToken cancellationToken) => Task.FromResult<string?>(token);
}
