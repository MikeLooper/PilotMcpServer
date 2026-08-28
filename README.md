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
    └── PilotMcpServer.Tests/    # NUnit + Moq unit tests, mirrors src/ layout
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
dotnet build                 # builds the server and the test project
dotnet test                  # runs the unit test suite (NUnit + Moq)
dotnet run --project src/PilotMcpServer   # runs the server (stdio transport — expects an MCP client on the other end)
```

## Using it from an MCP host

Point your MCP client at the built executable and let it launch the server over stdio. For example, in a Claude Desktop / Claude Code style `mcpServers` config:

```json
{
  "mcpServers": {
    "pilot": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/path/to/PilotMcpServer/src/PilotMcpServer"]
    }
  }
}
```

(Or point `command` at the published/built `PilotMcpServer.exe` directly for a faster startup than `dotnet run`.)

All server logging goes to **stderr** — never stdout — so it never corrupts the JSON-RPC stream the client is reading from stdout.

## The Pilot APIs

The Pilot API is deployed in six equivalent flavors, all implementing the same contract (`shared/PilotSharedSource/OpenAPI/PilotApi_v1.yaml`). This server ships with the deployment list compiled directly into the assembly (`src/PilotMcpServer/Configuration/PilotApiCatalog.cs`) rather than reading it from an external config file at runtime, so it can't be silently altered post-deployment — a developer edits that file and rebuilds to change the list.

| Description                        | Host      | Container Name               | Port  |
| ---------------------------------- | --------- | ---------------------------- | ----- |
| .NET Core with SQL Server (default) | localhost | pilot-api-dotnet-mssql       | 55101 |
| .NET Core with PostgreSQL          | localhost | pilot-api-dotnet-postgres    | 55201 |
| Java Spring Boot with SQL Server   | localhost | pilot-api-java-mssql         | 55301 |
| Java Spring Boot with PostgreSQL   | localhost | pilot-api-java-postgres      | 55401 |
| Python with SQL Server             | localhost | pilot-api-python-mssql       | 55701 |
| Python with PostgreSQL             | localhost | pilot-api-python-postgres    | 55801 |

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
