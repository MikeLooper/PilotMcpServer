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
│       ├── Models/              # DTOs mirroring the Pilot API's OpenAPI schemas
│       ├── Services/            # HTTP client + API-selection state
│       └── Tools/               # MCP tool classes, one per resource
└── tests/
    ├── PilotMcpServer.IntegrationTests/  # NUnit integration tests (OpenAPI compliance)
    ├── PilotMcpServer.Tests/    # NUnit + Moq unit tests, mirrors src/ layout
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
| Headers (`X-Correlation-Id`) and bearer authentication | A request for each path passed through the real correlation ID and security delegating handlers, in memory, into a recording stub |

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

### Manual validation in VS Code

To check that the MCP server works correctly against the running APIs, open VS Code in the `tests/vscode` directory and follow the directions in the README found there.
