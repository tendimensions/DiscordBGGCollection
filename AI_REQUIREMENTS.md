# Technical Requirements Document: DiscordBGGCollection

## Overview
This document defines the technical requirements for the DiscordBGGCollection solution. It is intended for AI parsing and to maintain the integrity and scope of the solution.

## Solution Structure
- The solution consists of two main projects:
  - `DiscordBGGCollection`: The main application, a Discord bot that interacts with BoardGameGeek (BGG) APIs.
  - `TestCases`: Contains unit tests for validating command logic and data retrieval.

## Functional Requirements
1. The bot must connect to Discord using a bot token specified in `appsettings.json` which is not part of source control.
2. The bot must respond to Discord commands related to BGG data, including:
  - Fetching a user's board game collection
  - Fetching a user's want-to-play list
  - Displaying game information from BGG
3. All BGG data must be retrieved via the official BGG API or compatible endpoints.
4. The bot must format and send responses back to Discord channels.
5. Error handling must be implemented for API failures, empty results, and malformed commands.

## Non-Functional Requirements
1. The solution must target .NET 8.0 or higher.
2. All configuration must be managed via `appsettings.json` which is not part of source control.
3. The bot must respond to commands within 5 seconds under normal conditions.
4. The solution must be maintainable and extensible for future features (e.g., recommendations, statistics).

## Data Requirements
1. Only publicly available BGG data should be accessed unless user credentials are provided.
2. Data caching may be implemented to reduce API calls and improve performance.
3. User privacy must be respected; no personal data should be stored without consent.

## Testing Requirements
1. All command logic and data retrieval must be covered by unit tests in the `TestCases` project.
2. Tests must use xUnit and cover both success and failure scenarios.
3. Code coverage tools (e.g., coverlet) should be used to ensure adequate test coverage.

## Dependencies
- Discord.Net for Discord bot functionality
- Newtonsoft.Json for JSON parsing
- xUnit for unit testing
- coverlet.collector for code coverage

## Configuration
- `appsettings.json` must include:
  - Discord bot token
  - Any BGG API configuration
  - Logging settings
- `appsettings.json` contains sensitive data, should not be checked into source control, and included in the .gitignore file

## Deployment
### Docker Container Deployment
To simplify deployment and ensure consistency across environments, the DiscordBGGCollection solution can be packaged and deployed as a Docker container. Sensitive configuration, such as `appsettings.json`, must be supplied from outside the Docker image and mounted into the container at runtime. This ensures secrets are not baked into the image and can be managed securely. The container does not need to expose any ports, but it must be able to make outbound HTTPS requests to Discord and BoardGameGeek (BGG) APIs (port 443).

#### Requirements
- Docker must be installed on the target host system.
- The solution must include a `Dockerfile` in the root directory that defines the build and runtime environment for the .NET application.
- Sensitive configuration (such as `appsettings.json` with the Discord bot token) must be provided at runtime from outside the image, using a mounted volume. Do not copy or bake `appsettings.json` into the Docker image during build.

#### Build and Run Steps
1. Build the Docker image:
  ```powershell
  docker build -t discordbggcollection .
  ```
2. Run the container:
  ```powershell
  docker run -d --name discordbggcollection \
    -v /path/to/appsettings.json:/app/appsettings.json \
    -e DOTNET_ENVIRONMENT=Production \
    discordbggcollection
  ```
  - Replace `/path/to/appsettings.json` with the actual path to your configuration file on the host system.
  - The file will be mounted inside the container at `/app/appsettings.json` and used by the application at runtime.
  - You may also use environment variables for additional configuration as needed.

#### Best Practices
- Do not include or copy sensitive configuration files (such as `appsettings.json`) into the Docker image. Always supply them from outside the image and mount them as volumes at runtime.
- Use a `.dockerignore` file to explicitly exclude sensitive files (e.g., `appsettings.json`) and other unnecessary files from being copied into the Docker build context. This helps prevent accidental inclusion of secrets in the image.
- Use environment variables or Docker secrets for sensitive data.
- Keep the Docker image up to date with security patches and .NET runtime updates.

#### Terminology
- **Dockerfile**: Script that defines how the application is built and run inside a container.
- **Image**: Packaged application and its dependencies, built from the Dockerfile.
- **Container**: Running instance of the Docker image.
- **Volume**: Mounted directory or file from the host into the container, used for configuration or persistent data.
- **Environment Variable**: Key-value pair passed to the container at runtime for configuration.

## Extensibility
1. The solution should allow for easy addition of new Discord commands.
2. Future AI features (e.g., natural language processing, recommendations) should be supported by modular design.

## Build & Run Instructions
1. Restore dependencies:
  ```powershell
  dotnet restore
  ```
2. Build the solution:
  ```powershell
  dotnet build
  ```
3. Run unit tests:
  ```powershell
  dotnet test
  ```
4. Run the bot:
  ```powershell
  dotnet run --project DiscordBGGCollection/DiscordBGGCollection.csproj
  ```

---
This document should be kept up to date as the solution evolves. Add or edit requirements as needed to reflect changes in scope or functionality.