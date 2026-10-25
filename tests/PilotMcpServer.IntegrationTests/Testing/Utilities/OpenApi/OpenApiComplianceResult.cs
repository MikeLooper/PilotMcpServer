using System.Text;

namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>Every finding from comparing the OpenAPI specification with the implementation, grouped by scope.</summary>
public sealed class OpenApiComplianceResult
{
    public required IReadOnlyList<ComplianceFinding> Findings { get; init; }

    /// <summary>The operation keys ("GET /about") found in the specification, the implementation, or both.</summary>
    public required IReadOnlyList<string> OperationScopes { get; init; }

    /// <summary>The component scopes ("#/components/schemas/AboutResponse") defined by the specification.</summary>
    public required IReadOnlyList<string> ComponentScopes { get; init; }

    public IReadOnlyList<ComplianceFinding> For(string scope)
        => Findings.Where(finding => finding.Scope == scope).ToArray();

    public int Count(ComplianceSeverity severity) => Findings.Count(finding => finding.Severity == severity);

    public string Summary
        => $"OpenAPI compliance: {Count(ComplianceSeverity.Error)} error(s), {Count(ComplianceSeverity.Warn)} warning(s), " +
            $"{Count(ComplianceSeverity.Info)} info.";

    /// <summary>Renders every finding as a Markdown report, most severe first and grouped by scope.</summary>
    public string ToMarkdownReport()
    {
        var report = new StringBuilder()
            .AppendLine("# Pilot API OpenAPI compliance report")
            .AppendLine()
            .AppendLine($"Specification: `{OpenApiSpecificationLoader.SpecificationPath}`")
            .AppendLine()
            .AppendLine(Summary)
            .AppendLine();

        foreach (var severity in new[] { ComplianceSeverity.Error, ComplianceSeverity.Warn, ComplianceSeverity.Info })
        {
            var findings = Findings.Where(finding => finding.Severity == severity).ToArray();
            report.AppendLine($"## {severity} ({findings.Length})").AppendLine();

            foreach (var group in findings.GroupBy(finding => finding.Scope))
            {
                report.AppendLine($"### {group.Key}").AppendLine();
                foreach (var finding in group)
                {
                    report.AppendLine($"- **{finding.Location}**: {finding.Message}");
                }

                report.AppendLine();
            }
        }

        return report.ToString();
    }
}
