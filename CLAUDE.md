# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore, build, test, run
dotnet restore
dotnet build
dotnet test
dotnet run --project DiscordBGGCollection/DiscordBGGCollection.csproj

# Run a single test by name
dotnet test --filter "FullyQualifiedName~FetchGamesFromBGG"

# Docker
docker build -t discordbggcollection .
docker run -d --name discordbggcollection \
  -v /path/to/appsettings.json:/app/appsettings.json \
  -e DOTNET_ENVIRONMENT=Production \
  discordbggcollection
```

## Configuration

`appsettings.json` is gitignored and must be created locally. See `appsettings.example.json` for the required shape:
```json
{
  "BotToken": "<discord-bot-token>",
  "BggApiKey": "<bgg-application-token>"
}
```

`BggApiKey` is sent as `Authorization: Bearer <key>` on every BGG XML API v2 request. Register an application at https://boardgamegeek.com/applications to obtain a token.

It is also excluded from the Docker image via `.dockerignore` and must be mounted as a volume at runtime — never baked into the image.

## Architecture

**Two projects:**
- `DiscordBGGCollection/` — the bot application
- `TestCases/` — xUnit tests (currently hit the real BGG API, not mocked)

**Flow:**
1. `Program.cs` initializes `DiscordSocketClient`, registers `CommandService`, wires up DI (`HttpClient`, `BGGCommands`, `IConfiguration`), and sets the `MessageReceived` handler.
2. `MessageReceived` strips the `/` prefix, maps bare `/bgg` and unknown subcommands to `help`, then executes via `CommandService`.
3. `BGGCommands.cs` (`ModuleBase<SocketCommandContext>`, group `"bgg"`) contains all command logic. It calls the BGG XML API v2 (`https://boardgamegeek.com/xmlapi2/collection`), parses XML with `XDocument`/LINQ-to-XML, and formats tabular responses.
4. Responses over 2000 characters are sent as attached text files via a direct HTTP POST to the Discord API (`SendFileManuallyAsync`).

**Resilience:** BGG frequently returns HTTP 202 (queue/processing). BGGCommands handles this with a manual retry loop in addition to a Polly `AsyncRetryPolicy` with exponential backoff for `HttpRequestException` and `TaskCanceledException`.

**Key dependencies:** Discord.Net 3.4.0, Polly 8.5.0, xUnit, coverlet.

## Incomplete / Stub Features

- `/bgg plays <username>` — command exists but is not implemented.
