namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>Runs the specification-versus-implementation comparison once and shares the result across tests.</summary>
public static class OpenApiCompliance
{
    private static readonly Lazy<OpenApiComplianceResult> LazyResult = new(Run);

    public static OpenApiComplianceResult Result => LazyResult.Value;

    private static OpenApiComplianceResult Run()
        => new OpenApiContractComparer(OpenApiSpecificationLoader.Load(), ImplementedContractBuilder.Build()).Compare();
}
