using PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

namespace PilotMcpServer.IntegrationTests.OpenApi;

/// <summary>
/// Validates that the Pilot API contract implemented by this client matches the shared OpenAPI specification
/// (shared/PilotSharedSource/OpenAPI/PilotApi_v1.yaml). Error findings fail the test for their scope; Warn and Info
/// findings are logged to the test output and to the attached OpenApiComplianceReport.md.
/// </summary>
[TestFixture]
public class OpenApiComplianceTests
{
    [OneTimeSetUp]
    public void WriteComplianceReport()
    {
        var result = OpenApiCompliance.Result;
        var reportPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "OpenApiComplianceReport.md");

        File.WriteAllText(reportPath, result.ToMarkdownReport());

        TestContext.AddTestAttachment(reportPath, "Full OpenAPI compliance report (all severities).");
        TestContext.Progress.WriteLine($"{result.Summary} Report: {reportPath}");
    }

    [TestCaseSource(nameof(OperationScopes))]
    public void OpenApiCompliance_Operation_HasNoErrorFindings_Test(string operationScope)
    {
        var findings = OpenApiCompliance.Result.For(operationScope);

        var errors = LogAndGetErrors(findings);

        Assert.That(errors, Is.Empty, FormatErrors(operationScope, errors));
    }

    [TestCaseSource(nameof(ComponentScopes))]
    public void OpenApiCompliance_Component_HasNoErrorFindings_Test(string componentScope)
    {
        var findings = OpenApiCompliance.Result.For(componentScope);

        var errors = LogAndGetErrors(findings);

        Assert.That(errors, Is.Empty, FormatErrors(componentScope, errors));
    }

    [Test]
    public void OpenApiCompliance_Document_HasNoErrorFindings_Test()
    {
        var findings = OpenApiCompliance.Result.For(ComplianceFinding.DocumentScope);

        var errors = LogAndGetErrors(findings);

        Assert.That(errors, Is.Empty, FormatErrors(ComplianceFinding.DocumentScope, errors));
    }

    private static IEnumerable<string> OperationScopes() => OpenApiCompliance.Result.OperationScopes;

    private static IEnumerable<string> ComponentScopes() => OpenApiCompliance.Result.ComponentScopes;

    private static ComplianceFinding[] LogAndGetErrors(IReadOnlyList<ComplianceFinding> findings)
    {
        foreach (var finding in findings.Where(finding => finding.Severity != ComplianceSeverity.Error))
        {
            TestContext.Out.WriteLine(finding.ToString());
        }

        return findings.Where(finding => finding.Severity == ComplianceSeverity.Error).ToArray();
    }

    private static string FormatErrors(string scope, IEnumerable<ComplianceFinding> errors)
        => $"OpenAPI compliance errors for {scope}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(error => $"  {error.Location}: {error.Message}"));
}
