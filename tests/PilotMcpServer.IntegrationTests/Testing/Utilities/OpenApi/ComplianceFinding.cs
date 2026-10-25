namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// One difference between the OpenAPI specification and the implementation.
/// </summary>
/// <param name="Severity">How serious the difference is.</param>
/// <param name="Scope">What the finding belongs to: an operation key (e.g. "GET /about"), a component
/// (e.g. "#/components/schemas/CategoriesDto") or <see cref="DocumentScope"/>.</param>
/// <param name="Location">Where within the scope the difference is (e.g. "response 200 > categoryName").</param>
/// <param name="Message">What differs.</param>
public sealed record ComplianceFinding(ComplianceSeverity Severity, string Scope, string Location, string Message)
{
    public const string DocumentScope = "document";

    public override string ToString() => $"[{Severity}] {Scope} :: {Location} :: {Message}";
}
