namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// Which way data flows through a schema. Comparisons are consumer-compatible: a request schema may be stricter than
/// the specification (the client only has to send valid data), while a response schema must accept everything the
/// specification allows the server to return.
/// </summary>
public enum SchemaDirection
{
    /// <summary>Data serialized by the client and sent to the Pilot API.</summary>
    Request,

    /// <summary>Data returned by the Pilot API and deserialized by the client.</summary>
    Response,
}
