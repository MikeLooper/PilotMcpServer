namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>How serious a difference between the OpenAPI specification and the implementation is.</summary>
public enum ComplianceSeverity
{
    /// <summary>Informational only (descriptions, examples, alternate types, component names).</summary>
    Info,

    /// <summary>Worth reviewing but not a contract break (extras, optional items, document metadata).</summary>
    Warn,

    /// <summary>A contract break; fails the compliance test.</summary>
    Error,
}
