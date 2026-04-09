# Future Work

## Correctness / Reliability

1. **Implement `/bgg plays`** — the command exists but returns nothing. Implement it or remove it.
2. **Mock HTTP in tests** — current tests hit the real BGG API and are flaky offline. Add mocked `HttpClient` tests that don't depend on live BGG user data.

## Architecture

3. **Extract `BggApiClient`** — HTTP calls, XML parsing, and retry logic are all in `BGGCommands.cs`. Move them into a dedicated client class.
4. **Extract `ResponseFormatter`** — table formatting and file-upload logic should live separately from command handlers.
5. **Replace `SendFileManuallyAsync` with Discord.Net's `SendFileAsync`** — removes the raw HTTP POST and ~30 lines of manual auth handling.
6. **Use typed options** — replace `_config["BotToken"]` magic strings with an `IOptions<BotSettings>` class.

## Features

7. **Reverse `/bgg compare` direction** — currently only checks user1's want-to-play vs user2's collection. Support the reverse.
8. **Response caching** — cache BGG API responses for 5–10 minutes to reduce latency and API load.
9. **Pagination** — large collections are truncated. Add pagination instead of cutting off results.

## Ops

10. **Structured logging** — replace `Console.WriteLine` with `Microsoft.Extensions.Logging` for better Docker log aggregation.
11. **Wire up `DOTNET_ENVIRONMENT`** — the Docker run command sets it to `Production` but the app doesn't load `appsettings.Production.json`. Implement environment-specific config or remove the env var.
