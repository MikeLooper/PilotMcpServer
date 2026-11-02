# Bruno tests for the Pilot MCP Server

This [Bruno](https://www.usebruno.com/) collection calls all 45 MCP tools through the server's real MCP pipeline, against a running Pilot API deployment.

## What you need

- The Pilot API containers and Keycloak running.
- Node.js, to run the Bruno CLI with `npx`. You can also open this folder in the Bruno app.
- Port 11901 free. If it isn't, use another port in the command below and change `mcpUrl` in the environment file.
- Existing records in every resource, because the list tests expect results.

## Running the tests

Bruno sends HTTP requests, so start the server in its HTTP mode and leave it running. In this mode it accepts loopback addresses only, and a normal start still uses stdio.

```bash
cd C:/Working/Storage/Dev/GitHub/PilotMcpServer
dotnet run --project src/PilotMcpServer -- --http-url http://localhost:11901
```

Then, in a second terminal, run the collection against one deployment:

```bash
cd tests/Bruno
npx @usebruno/cli@4.3.0 run --env dotnet-mssql -r
```

To save the results, add `--reporter-html results.html`.

In the Bruno app, choose an environment from the list at the top right before you open the runner. With no environment selected, the collection's defaults apply: `http://localhost:11901` and `.NET Core with SQL Server`.

| Environment | Deployment |
| --- | --- |
| `dotnet-mssql` | .NET Core with SQL Server (the default) |
| `dotnet-postgres` | .NET Core with PostgreSQL |
| `java-mssql` | Java Spring Boot with SQL Server |
| `java-postgres` | Java Spring Boot with PostgreSQL |
| `python-mssql` | Python with SQL Server |
| `python-postgres` | Python with PostgreSQL |

Run the collection with `-r`, or use the Bruno runner, because later requests use IDs saved by earlier ones. A run takes about 25 seconds.

## What is tested

| Folder | Coverage |
| --- | --- |
| `Session` | The MCP handshake, the exact list of tools, and `correlationId` in every tool's schema |
| `System` | Healthcheck, about, endpoints, API selection, and unknown API names. It also checks that a valid or invalid `correlationId` doesn't fail a call |
| Each data folder | Get all, a page, a page past the end, then add, get, update, get, delete, and get again |

The tests can't see the `X-Correlation-Id` header the server sends to the Pilot API. Use Grafana to check that value.

## Test data

Each data folder creates its own records and deletes them when it finishes, so the seed data isn't changed. `Order Details` also creates and removes its own order and product.

A run that stops early can leave records behind. To find them, look for `Bruno Test` or `Bruno Updated` in the name field (`shipName` on orders), or `Bruno` in `firstName` on employees.

`System` changes the server's selected API, then sets it back to `.NET Core with SQL Server`. Don't share a running HTTP server with another client during a run.
