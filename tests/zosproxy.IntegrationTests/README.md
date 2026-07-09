# zosproxy integration tests

These tests validate the HTTP contract exposed by `zosproxy` and exercise the current integration path:

```text
xUnit -> HTTP -> zosproxy -> COBOL executable -> response JSON
```

## Before running

Start `zosproxy` first:

```bash
dotnet run --project zosproxy/zosproxy.csproj
```

Make sure the COBOL executable exists at the path expected by `zosproxy`.

From `LegacyCobol`:

```bash
cobc -x -free -o alfa ALFA.cbl
```

## Run tests

From the repository root:

```bash
dotnet test tests/zosproxy.IntegrationTests/zosproxy.IntegrationTests.csproj
```

By default the tests call:

```text
http://localhost:5005
```

To use a different URL:

```bash
ZOSPROXY_BASE_URL=http://localhost:5005 dotnet test tests/zosproxy.IntegrationTests/zosproxy.IntegrationTests.csproj
```

## Covered scenarios

- Consult existing client: `GET /api/clientes/000001`
- Consult missing client: `GET /api/clientes/999999`
- Update contact and verify persistence: `PUT /api/clientes/000001/contato`
