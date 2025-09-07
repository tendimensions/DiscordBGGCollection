# DiscordBGGCollection

DiscordBGGCollection is a C#/.NET solution that integrates BoardGameGeek (BGG) data with Discord. It allows users to fetch and display board game collections, want-to-play lists, and other BGG information directly in Discord via bot commands.

## Features
- Fetches board game collections from BGG for a given user
- Displays want-to-play lists and other BGG data in Discord
- Supports custom Discord bot commands for interacting with BGG
- Includes unit tests for command logic and data retrieval

## Projects
- **DiscordBGGCollection**: Main application and Discord bot logic
- **TestCases**: Unit tests for command and data logic

## How It Works
1. The Discord bot listens for specific commands in Discord channels.
2. When a command is received, it queries the BGG API for the requested data (e.g., a user's collection or want-to-play list).
3. The bot formats and sends the results back to Discord.

## Getting Started
1. **Clone the repository**
   ```powershell
   git clone <repo-url>
   ```
2. **Restore dependencies**
   ```powershell
   dotnet restore
   ```
3. **Build the solution**
   ```powershell
   dotnet build
   ```
4. **Run unit tests**
   ```powershell
   dotnet test
   ```
5. **Configure the bot**
   - Edit `appsettings.json` with your Discord bot token and any required settings.
6. **Run the bot**
   ```powershell
   dotnet run --project DiscordBGGCollection/DiscordBGGCollection.csproj
   ```

## Configuration
- `appsettings.json`: Contains Discord bot token and other settings.

## Requirements
- .NET 8 SDK
- Discord bot token

## Contributing
Pull requests and issues are welcome!

## License
MIT License
