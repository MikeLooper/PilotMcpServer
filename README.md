# Pilot MCP Server

An [MCP](https://modelcontextprotocol.io/) (Model Context Protocol) server, written in C# / .NET 10, that gives an LLM agent tools to interact with the **Pilot API** — a Northwind-based reference REST API that is deployed in six equivalent flavors (different language/runtime and database combinations). The server talks to whichever deployment you select and exposes strongly-typed tools for every resource in the API, plus a couple of housekeeping tools for discovering what's available.

It communicates with its MCP host over **stdio**, so it's meant to be launched as a subprocess by an MCP-aware client (Claude Desktop, the Claude Code CLI, VS Code's MCP support, etc.) rather than run standalone.

## Directory structure

```
PilotMcpServer/
├── Directory.Build.props        # shared MSBuild settings (net10.0, nullable, etc.)
├── PilotMcpServer.slnx          # solution file
├── shared/
│   └── PilotSharedSource/       # git submodule — OpenAPI contract shared by all Pilot API deployments
├── src/
│   └── PilotMcpServer/          # the MCP server (console app)
│       ├── Program.cs           # host/DI wiring, stdio transport, logging isolation
│       ├── Configuration/       # the API catalog and endpoint summary catalog
│       ├── Models/              # DTOs mirroring the Pilot API's OpenAPI schemas, plus RequestSettings
│       ├── OpenTelemetry/       # OTEL attribute names and span/log record processors
│       ├── Services/            # HTTP client, API-selection state, request settings
│       └── Tools/               # MCP tool classes, one per resource
└── tests/
    ├── PilotMcpServer.IntegrationTests/  # NUnit integration tests (OpenAPI compliance)
    ├── PilotMcpServer.Tests/    # NUnit + Moq unit tests, mirrors src/ layout
    ├── Bruno                    # Bruno collection that calls every tool against a running Pilot API
    └── vscode                   # VS Code manual validation and testing
```

## Shared source submodule

The Pilot API's OpenAPI contract (and any other shared source) lives in a separate repository, [MikeLooper/PilotSharedSource](https://github.com/MikeLooper/PilotSharedSource), and is included here as a git submodule at `shared/PilotSharedSource`.

**First clone of this repo** — pull the submodule in the same step:

```bash
git clone --recurse-submodules https://github.com/MikeLooper/PilotMcpServer.git
```

**Already cloned without submodules?** Fetch it now:

```bash
git submodule update --init --recursive
```

**Pulling in upstream changes to the submodule later** — this repo only tracks a specific commit of `PilotSharedSource`, so bump it explicitly when the shared repo changes:

```bash
git submodule update --remote --merge shared/PilotSharedSource
git add shared/PilotSharedSource
git commit -m "Update PilotSharedSource submodule"
```

(Equivalently: `cd shared/PilotSharedSource && git pull origin main`, then `cd -` and commit the updated gitlink from the parent repo.)

## Building, testing, and running

```bash
dotnet build                 # builds the server and both test projects
dotnet test                  # runs the unit tests and the OpenAPI compliance integration tests
dotnet run --project src/PilotMcpServer   # runs the server (stdio transport — expects an MCP client on the other end)
```

## Using it from an MCP host

Point your MCP client at the built executable and let it launch the server over stdio. For example, in a Claude Desktop / Claude Code style `mcpServers` config:

```json
{
  "mcpServers": {
    "pilot": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/Working/Storage/Dev/GitHub/PilotMcpServer/src/PilotMcpServer"]
    }
  }
}
```

(Or point `command` at the published/built `PilotMcpServer.exe` directly for a faster startup than `dotnet run`.)
```json
{
  "servers": {
    "pilot": {
      "type": "stdio",
      "command": "C:/Working/Storage/Dev/GitHub/PilotMcpServer/src/PilotMcpServer/bin/Debug/net10.0/PilotMcpServer.exe",
      "args": []
    }
  }
}
```

All server logging goes to **stderr** — never stdout — so it never corrupts the JSON-RPC stream the client is reading from stdout.

## The Pilot APIs

The Pilot API is deployed in six equivalent flavors, all implementing the same contract (`shared/PilotSharedSource/OpenAPI/PilotApi_v1.yaml`). This server ships with the deployment list compiled directly into the assembly (`src/PilotMcpServer/Configuration/PilotApiCatalog.cs`) rather than reading it from an external config file at runtime, so it can't be silently altered post-deployment — a developer edits that file and rebuilds to change the list.

| Description                        | Host      | Container Name               | Port  |
| ---------------------------------- | --------- | ---------------------------- | ----- |
| .NET Core with SQL Server (default) | localhost | pilot-api-dotnet-mssql       | 11001 |
| .NET Core with PostgreSQL          | localhost | pilot-api-dotnet-postgres    | 11101 |
| Java Spring Boot with SQL Server   | localhost | pilot-api-java-mssql         | 11201 |
| Java Spring Boot with PostgreSQL   | localhost | pilot-api-java-postgres      | 11301 |
| Python with SQL Server             | localhost | pilot-api-python-mssql       | 11401 |
| Python with PostgreSQL             | localhost | pilot-api-python-postgres    | 11501 |

### Docker hostname selection (`RUNNING_IN_DOCKER`)

When `PilotApiCatalog` builds `PilotApiEndpoint` entries, it selects the hostname source based on the `RUNNING_IN_DOCKER` environment variable:

- If `RUNNING_IN_DOCKER` is present and set to `true` (case-insensitive), the endpoint uses each API's `ContainerName`.
- Otherwise, the endpoint uses each API's `Host`.

This allows the same build to run correctly:
- outside Docker (use `localhost` hostnames), and
- inside Docker/container networks (use service/container DNS names).

Examples:

```powershell
$env:RUNNING_IN_DOCKER = "true"
dotnet run --project src/PilotMcpServer
```

```bash
RUNNING_IN_DOCKER=true dotnet run --project src/PilotMcpServer
```

Every data tool accepts an optional `apiName` argument (matching the **Description** column above, e.g. `"Python with PostgreSQL"`) to call a specific deployment for that one call. Call `select_api` to change the default used when `apiName` is omitted.

## Tools

**System / discovery**

| Tool | Description |
| --- | --- |
| `select_api` | Selects which Pilot API deployment subsequent calls use by default. |
| `list_apis` | Lists every configured deployment with live availability, version, and deploy date (from each API's `/about` endpoint), and flags the current selection. |
| `list_endpoints` | Summarizes the logical endpoints of the Pilot API contract, once — not repeated per deployment, since all six share the same contract. |

**Data tools** — each resource below gets `get_all_*`, `get_*`, `add_*`, `update_*`, and `delete_*` tools (e.g. `get_all_categories`, `get_category`, `add_category`, `update_category`, `delete_category`):

- Categories
- Customers
- Employees
- Orders
- Order Details (`get_order_detail` / `delete_order_detail` take both `productId` and `orderId`, since order lines use a composite key)
- Products
- Shippers
- Suppliers

Every tool is `async`, takes a `CancellationToken`, and carries a `[Description]` written for the calling LLM. Request/response DTOs mirror the OpenAPI schemas field-for-field (including which fields are required vs. optional), so payload shape is validated the same way the underlying API validates it.

## OpenTelemetry (OTEL)

The server exports traces, metrics and logs over OTLP to the OpenTelemetry collector in the `otel-collector` Docker container. The collector sends them on to the local Grafana LGTM stack: traces to **Tempo**, metrics to **Mimir** and logs to **Loki**. To start the stack, run `C:\Working\Storage\Dev\GitHub\Docker\Otel\Docker-Otel-builder.bat`, then open Grafana at http://localhost:3000.

Logs still go to stderr as before, and OTEL is added alongside them. No OTEL console exporter is registered, because it writes to stdout, which the stdio transport keeps for JSON-RPC.

### Configuration

The settings are in the `OpenTelemetry` section of the embedded `src/PilotMcpServer/appsettings.json`. Like the rest of that file, they are compiled in, so you rebuild to change them.

| Setting | Default | Meaning |
| --- | --- | --- |
| `Active` | `true` | `false` turns off all OTEL providers and exporters. |
| `Host` | `localhost` | Collector host when running outside Docker. |
| `ContainerName` | `otel-collector` | Collector host when `RUNNING_IN_DOCKER=true` (see [Docker hostname selection](#docker-hostname-selection-running_in_docker)). |
| `Port` | `4317` | `4317` for OTLP gRPC, `4318` for OTLP HTTP. |
| `Protocol` | `Grpc` | `Grpc` or `HttpProtobuf`. Change it together with `Port`. |
| `ServiceName` | `PilotMcpServer` | The `service.name` resource attribute. In Loki this is the `service_name` label, and in Mimir it is the `job` label. |

If the collector isn't running, the server works normally and the telemetry is dropped.

### What is recorded

- **Traces:** one span per MCP request from the ModelContextProtocol SDK (e.g. `tools/call get_category`), with a child span for each outgoing HTTP call: the Pilot API call and the Keycloak token request. The W3C `traceparent` header goes out with each Pilot API call. When the Pilot API is instrumented as well, as `PilotApiDotNet` is, its spans join the same trace.
- **Metrics:** MCP operation and session durations (`mcp_server_operation_duration_seconds`, `mcp_server_session_duration_seconds`) and the .NET HttpClient metrics (`http_client_request_duration_seconds`, `http_client_active_requests`, etc.).
- **Logs:** every `ILogger` entry, with the formatted message and scopes. This includes the start and end entries described in [Request and Pilot API call logging](#request-and-pilot-api-call-logging), the SDK's "tool completed" and "tool threw an unhandled exception" entries, and the HttpClient request/response entries.

### Pilot API request headers

Every Pilot API request, including the System endpoints (`/about` and `/healthcheck`), carries the three optional headers that the OpenAPI specification defines on every endpoint:

| Header | Value | Set by |
| --- | --- | --- |
| `X-Correlation-Id` | The current request's correlation ID (see [Correlation ID](#correlation-id)), or a new GUID outside of an MCP request | `CorrelationIdDelegatingHandler` |
| `X-Api-Version` | The `apiVersion` from the embedded `appsettings.json` (currently `1`) | `PilotApiHeadersDelegatingHandler` |
| `X-Caller-Name` | `PilotMcpServer`, so the Pilot API's logs show which application made the call | `PilotApiHeadersDelegatingHandler` |

A value already on the request is kept (for `X-Correlation-Id`, only if it is a single valid GUID). The handlers run in this order on the `IPilotHttpClient`: correlation ID, API headers, security (bearer token, domain endpoints only), then logging.

### Request settings

Each incoming MCP request gets its own `RequestSettings`, set up by an incoming message filter (`RequestSettingsMessageFilter`) and available for the whole request through `IRequestSettingsAccessor`:

| Setting | Value | Span tag | Log attribute |
| --- | --- | --- | --- |
| `CorrelationId` | The GUID the caller supplied, or a new GUID (see [Correlation ID](#correlation-id)) | `correlationId` | `CorrelationId` |
| `RequestId` | The JSON-RPC `id` of the MCP request | `mcpRequestId` | `McpRequestId` |
| `CallerName` | The MCP client's `clientInfo.name` from its `initialize` request (e.g. `Visual Studio Code`) | `callerName` | `CallerName` |

The tags are added to the MCP request span and every span started during the request; the attributes are added to every log entry written during the request. `RequestId` and `CallerName` are left out when they aren't known. The request ID attribute is named `McpRequestId`, not `RequestId`, because ASP.NET Core's log scope already uses `RequestId` for its HTTP request identifier when the server runs over HTTP.

`CallerName` names who called *this* server, so it isn't forwarded to the Pilot API: `X-Caller-Name` names this server instead. To follow one request across both services, use the correlation ID.

### Request and Pilot API call logging

The start and end of each MCP request and each Pilot API call is logged. This server has no database of its own: the Pilot API calls are its data access, and the Pilot API logs its own database calls. Every entry includes the correlation ID, request ID and caller name, both in the message text (so they show in the stderr console output) and as structured fields.

| Event ID | Level | Logger category | Entry |
| --- | --- | --- | --- |
| 1000 | Information | `PilotMcpServer.Services.RequestSettingsMessageFilter` | `MCP request {McpOperation} started.` |
| 1001 | Information | `PilotMcpServer.Services.RequestSettingsMessageFilter` | `MCP request {McpOperation} completed in {ElapsedMilliseconds} ms.` |
| 1002 | Error | `PilotMcpServer.Services.RequestSettingsMessageFilter` | `MCP request {McpOperation} failed after {ElapsedMilliseconds} ms.` (with the exception) |
| 2000 | Information | `PilotMcpServer.Services.PilotApiLoggingDelegatingHandler` | `Pilot API call {HttpMethod} {PilotApiPath} to {PilotApiName} started.` |
| 2001 | Information | `PilotMcpServer.Services.PilotApiLoggingDelegatingHandler` | `Pilot API call ... completed with status {StatusCode} in {ElapsedMilliseconds} ms.` |
| 2002 | Warning | `PilotMcpServer.Services.PilotApiLoggingDelegatingHandler` | `Pilot API call ... failed after {ElapsedMilliseconds} ms.` (no response, e.g. connection refused or timeout; with the exception) |

`McpOperation` is the method, plus the tool name for a tool call (e.g. `tools/call get_category`), matching the SDK's span name. A tool that returns an error result still logs `completed`, because the SDK turns tool exceptions into error results before the filter sees them; the SDK's "tool threw an unhandled exception" entry records the failure. A Pilot API error status (e.g. 404 or 500) is logged as `completed` with that status code. A failed Pilot API call is a warning, not an error, because some tools expect it: `list_apis` reports an unreachable API as unavailable.

The Pilot API call entries log the correlation ID actually sent in `X-Correlation-Id`. Example console output for one tool call:

```text
MCP request tools/call get_category started. CorrelationId: b49b9d1e-ba7a-407d-b18c-bd58d4a25342, McpRequestId: 2, CallerName: Visual Studio Code
Pilot API call GET /v1/categories/get/1 to .NET Core with SQL Server started. CorrelationId: b49b9d1e-ba7a-407d-b18c-bd58d4a25342, McpRequestId: 2, CallerName: Visual Studio Code
Pilot API call GET /v1/categories/get/1 to .NET Core with SQL Server completed with status 200 in 695.4 ms. CorrelationId: b49b9d1e-ba7a-407d-b18c-bd58d4a25342, McpRequestId: 2, CallerName: Visual Studio Code
MCP request tools/call get_category completed in 1509.3 ms. CorrelationId: b49b9d1e-ba7a-407d-b18c-bd58d4a25342, McpRequestId: 2, CallerName: Visual Studio Code
```

### Correlation ID

Each incoming MCP request (each tool call) gets its own `RequestSettings` with a `CorrelationId` GUID: the value the caller supplied, or a new GUID if it didn't supply one (see [Request settings](#request-settings)). The same value is:

- sent to the Pilot API as the `X-Correlation-Id` header on every request the tool call makes, including the System endpoints.
- added to `Activity.Tags` as **`correlationId`** on the MCP request span and on every span started during the request.
- added to `LogRecord.Attributes` as **`CorrelationId`** on every log entry written during the request.

Because the Pilot API records the incoming `X-Correlation-Id` too, one ID finds a tool call's log entries in both the MCP server and the API.

Each response returns the IDs in `result._meta`, as `correlationId` and `traceId`. To tie several tool calls into one workflow, pass the first call's `correlationId` to the later calls.

**Supplying your own correlation ID**

To tie a request to a correlation ID from your own system, pass a GUID named `correlationId`. It is optional, and there are two places to put it:

| Location | Who uses it | Works on |
| --- | --- | --- |
| `params.arguments.correlationId` | Agents and skills, and any program calling a tool | `tools/call` |
| `params._meta.correlationId` | MCP clients and hosts, and programs | Any MCP request, e.g. `initialize` or `tools/list` |

Every tool lists `correlationId` in its input schema, so an agent or skill passes it like any other tool argument:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "get_category",
    "arguments": { "categoryId": 1, "correlationId": "3f2504e0-4f89-41d3-9a0c-0305e82c3301" }
  }
}
```

A program or host can send it in `_meta` instead, which also covers requests that have no tool arguments:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/list",
  "params": {
    "_meta": { "correlationId": "3f2504e0-4f89-41d3-9a0c-0305e82c3301" }
  }
}
```

- The name is case-sensitive.
- If both locations hold a GUID, `_meta` is used.
- Any format `Guid.TryParse` accepts is allowed. The value is converted to lowercase with hyphens (e.g. `3f2504e0-4f89-41d3-9a0c-0305e82c3301`), so search for that form in Grafana.
- A missing or invalid value doesn't fail the request. The server generates a new GUID instead.
- The value applies to that one request only. To use it on several requests, send it on each one.

The schema is added to every tool when the server starts (`CorrelationIdToolArgument`), so new tools get it without declaring a method parameter.

### Service instance ID

Every span and log entry carries an instance ID that tells which Pilot API this server was targeting: the service name (`OpenTelemetry:ServiceName`) in lowercase kebab case, a dash, then the text after the second-to-last dash of the selected Pilot API's container name.

| Service name | Target API | Suffix | Instance ID |
| --- | --- | --- | --- |
| `PilotMcpServer` | `pilot-api-dotnet-mssql` | `dotnet-mssql` | `pilot-mcp-server-dotnet-mssql` |
| `PilotMcpServer` | `pilot-api-python-postgres` | `python-postgres` | `pilot-mcp-server-python-postgres` |
| `PilotMcpServer` | `pilot-api-java-postgres` | `java-postgres` | `pilot-mcp-server-java-postgres` |

It is added to `Activity.Tags` as **`service.instance.id`** on every span (`EnrichActivityWithServiceInstanceIdProcessor`) and to `LogRecord.Attributes` as **`service_instance_id`** on every log entry (`EnrichLogsWithServiceInstanceIdProcessor`). It follows the currently selected API (`select_api`), so it is a span and log attribute, not the fixed `service.instance.id` resource attribute. A tool call that names a different API in its `apiName` argument still carries the selected API's instance ID.

### Exception attributes

When a log entry carries an exception, `LogExceptionAttributesProcessor` adds the following attributes:

| Attribute | Value |
| --- | --- |
| `Exception.Message` | `exception.Message` |
| `Exception.Stacktrace` | `exception.StackTrace` |
| `Exception.InnerException` | `exception.InnerException.ToString()` |

Each attribute is added only if its value exists (an exception that was never thrown has no stack trace) and the record doesn't already have an attribute with that name. The name check ignores case. If none of the three is added, the record isn't changed.

The OTLP exporter also adds the standard `exception.type`, `exception.message` and `exception.stacktrace` attributes itself, when it sends the record. Its `exception.stacktrace` is the full `exception.ToString()`, which includes inner exceptions.

### Finding events in Grafana

In Grafana, go to **Explore** (or **Drilldown → Logs / Traces / Metrics**) and pick a data source. Loki turns the dots in attribute names into underscores, so `Exception.Message` appears as `Exception_Message` and `service.name` as `service_name`.

**Loki (logs)**

| To find | LogQL |
| --- | --- |
| All server logs | `{service_name="PilotMcpServer"}` |
| Errors only | `{service_name="PilotMcpServer"} \| detected_level="error"` |
| One tool call, across the MCP server and Pilot API | `{service_name=~".+"} \| CorrelationId="<guid>"` |
| One tool call, MCP server only | `{service_name="PilotMcpServer"} \| CorrelationId="<guid>"` |
| Entries for a tool | `{service_name="PilotMcpServer"} \| ToolName="get_category"` |
| One MCP request by JSON-RPC ID | `{service_name="PilotMcpServer"} \| McpRequestId="<id>"` |
| Requests from one MCP client | `{service_name="PilotMcpServer"} \| CallerName="Visual Studio Code"` |
| Entries for one instance | `{service_name="PilotMcpServer"} \| service_instance_id="pilot-mcp-server-dotnet-mssql"` |
| MCP request start and end entries | `{service_name="PilotMcpServer"} \|= "MCP request"` |
| Pilot API call start and end entries | `{service_name="PilotMcpServer"} \|= "Pilot API call"` |
| Slow Pilot API calls | `{service_name="PilotMcpServer"} \|= "Pilot API call" \| ElapsedMilliseconds > 1000` |
| Pilot API calls that got no response | `{service_name="PilotMcpServer"} \|= "Pilot API call" \|= "failed after"` |
| Tool failures | `{service_name="PilotMcpServer"} \|= "threw an unhandled exception"` |
| Entries with an exception | `{service_name="PilotMcpServer"} \| Exception_Message!=""` |
| Exceptions by text | `{service_name="PilotMcpServer"} \| Exception_Message=~"(?i).*unknown pilot api.*"` |
| Outgoing Pilot API calls | `{service_name="PilotMcpServer"} \|= "Sending HTTP request"` |

Other useful fields on each entry: `TraceId` / `SpanId` (link to Tempo), `scope_name` (logger category, e.g. `ModelContextProtocol.Server.McpServer`), `severity_text`, `exception_type`, `service_version`.

**Tempo (traces)**

| To find | TraceQL |
| --- | --- |
| All server traces | `{ resource.service.name = "PilotMcpServer" }` |
| A tool call by correlation ID | `{ span.correlationId = "<guid>" }` |
| Requests from one MCP client | `{ span.callerName = "Visual Studio Code" }` |
| Spans for one instance | `{ span.service.instance.id = "pilot-mcp-server-dotnet-mssql" }` |
| A specific tool | `{ name = "tools/call get_category" }` |
| Failed tool calls | `{ resource.service.name = "PilotMcpServer" && status = error }` |
| Slow tool calls | `{ resource.service.name = "PilotMcpServer" && name =~ "tools/call.*" && duration > 1s }` |

**Mimir (metrics)**

| To find | PromQL |
| --- | --- |
| Tool calls per tool | `sum by (gen_ai_tool_name) (rate(mcp_server_operation_duration_seconds_count{job="PilotMcpServer", mcp_method_name="tools/call"}[5m]))` |
| Tool errors | `sum by (gen_ai_tool_name) (rate(mcp_server_operation_duration_seconds_count{job="PilotMcpServer", error_type!=""}[5m]))` |
| p95 tool duration | `histogram_quantile(0.95, sum by (le, gen_ai_tool_name) (rate(mcp_server_operation_duration_seconds_bucket{job="PilotMcpServer"}[5m])))` |
| Pilot API call latency | `histogram_quantile(0.95, sum by (le, server_address) (rate(http_client_request_duration_seconds_bucket{job="PilotMcpServer"}[5m])))` |

## Validation and Integration Testing

### OpenAPI compliance tests

`tests/PilotMcpServer.IntegrationTests` checks that the Pilot API contract this server implements matches the shared OpenAPI specification, `shared/PilotSharedSource/OpenAPI/PilotApi_v1.yaml`. It catches drift in either direction: a spec change that the client hasn't picked up, or a client change that no longer matches the spec. The tests need no running Pilot API, network access or Docker.

**Running them**

```bash
git submodule update --init --recursive    # the spec comes from the submodule
dotnet test tests/PilotMcpServer.IntegrationTests
```

They also run as part of a plain `dotnet test` and in the GitHub Actions build, which checks out submodules. If the spec file is missing, every test fails with a message telling you to initialize the submodule.

**How the comparison works**

This server is a *client* of the Pilot API and doesn't publish its own OpenAPI document, so the tests derive the implemented contract from the code:

| Part of the contract | Where it comes from |
| --- | --- |
| Operations (method, path, tag, summary) | `PilotEndpointCatalog` |
| Path and query parameters, request body, response type | The signature of the matching `[McpServerTool]` method (e.g. Categories `GetAll` → `CategoriesTools.GetAll*`) |
| Request and response schemas | The DTO types, read by reflection with the client's own `PilotHttpClient` JSON serializer options |
| Headers (`X-Correlation-Id`, `X-Api-Version`, `X-Caller-Name`) and bearer authentication | A request for each path passed through the real correlation ID, API header and security delegating handlers, in memory, into a recording stub |

The derived contract is then compared with the spec, which is read with Microsoft.OpenApi. Comparisons are **consumer-compatible**, so whether a difference matters depends on which way the data flows:

- **Requests** (data the client sends) may be stricter than the spec but never looser. For example, sending a non-null value where the spec allows null is fine; being able to send null where the spec forbids it is an error.
- **Responses** (data the client reads) must accept everything the spec allows the server to return. A response property the client simply ignores is a warning, not an error.

**Severity levels**

| Level | Effect | What it covers |
| --- | --- | --- |
| Error | Fails the test | A spec operation that isn't implemented; a required parameter or request body that is missing or can be omitted; a missing 2xx response; a request schema or property that is missing; a base type mismatch; any security mismatch; incompatible media type, nullability, required list, format, pattern, default, enum or oneOf/anyOf/allOf |
| Warn | Logged only | Extra operations, parameters, responses or properties; missing optional parameters; response properties, schemas or bodies the client doesn't read; document metadata (info, servers, tags, operationIds) |
| Info | Logged only | Descriptions and examples; alternate types such as `[integer, string]` vs `integer`; component names (e.g. `CategoriesDto` vs `CategoryDto`); differences that are compatible in the direction the data flows |

**Reading the results**

- There is one test per spec operation (e.g. `GET /v1/categories/get-all`), one per spec component (e.g. `#/components/schemas/CategoriesDto`) and one for document-level items. A failing test lists its errors in the assertion message.
- Each test writes its Warn and Info findings to the test output.
- The full report, covering every finding at every severity, is written to `OpenApiComplianceReport.md` in the test output directory (e.g. `tests/PilotMcpServer.IntegrationTests/bin/Debug/net10.0/`). It is also attached to the test run.

**When a test fails**

Decide which side is correct. If the spec changed, update the DTOs, tools or `PilotEndpointCatalog` to match it. If the client is right, change the spec in the `PilotSharedSource` repository and then bump the submodule (see [Shared source submodule](#shared-source-submodule)).

### Bruno tests

`tests/Bruno` holds a [Bruno](https://www.usebruno.com/) collection that calls every tool against a running Pilot API. Bruno sends HTTP requests, so the server has an opt-in HTTP mode for it, which accepts loopback addresses only:

```bash
dotnet run --project src/PilotMcpServer -- --http-url http://localhost:11901
```

This endpoint has no authentication, and its tools can change data, so use it for local testing only. See the [Bruno tests README](tests/Bruno/README.md) for how to run the collection.

### Manual validation in VS Code

To check that the MCP server works correctly against the running APIs, open VS Code in the `tests/vscode` directory and follow the directions in the README found there.
