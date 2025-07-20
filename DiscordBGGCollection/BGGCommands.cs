/*
 * This file is part of the DiscordBGGCollection project.
 *
 * (c) Jason Poggioli <jason.poggioli@gmail.com>
 *
 * Register a Discord Bot at https://discord.com/developers/applications
 */

namespace DiscordBGGCollection
{
    using Discord;
    using Discord.Commands;
    using Microsoft.Extensions.Configuration;
    using Polly;
    using Polly.Retry;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    [Group("bgg")]
    public class BGGCommands : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly IConfiguration _configuration;

        public BGGCommands(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var helpMessage = "Available commands (Note: usernames are BGG usernames):\n" +
                              "/bgg help - Lists all available commands.\n" +
                              "/bgg games <username> - Fetches games for the specified BGG username.\n" +
                              "/bgg plays <username> - Fetches play statistics for the specified BGG username.\n" +
                              "/bgg wanttoplay <username> - Fetches the user's 'Want to Play' collection\n" +
                              "/bgg compare <username1> <username2> - Provides the overlap of games between the 'Want to Play' of the first user with the collection of the second user\n";

            await ReplyAsync(helpMessage);
        }

        [Command("games")]
        [Description("List all games for a provided BGG username")]
        public async Task GetGamesAsync(string username = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                await ReplyAsync("Please provide a username. Usage: /bgg games <username>");
                return;
            }

            var games = await FetchGamesFromBGG(username);
            if (games == null || !games.Any())
            {
                await ReplyAsync("Could not fetch games for the provided username.");
                return;
            }

            //var gameNames = games.Select(g => g.Name);
            var lines = new List<string> { $"Games for {username}:" };
            lines.Add(string.Format("{0,-4} {1,-40} {2,-6} {3,-5}", "#", "Game Title", "Year", "Plays"));

            int i = 1;
            foreach (var game in games)
            {
                var year = game.YearPublished == 0 ? "—" : game.YearPublished.ToString();
                var name = game.Name?.Length > 40 ? game.Name.Substring(0, 37) + "..." : game.Name;

                lines.Add(string.Format("{0,-4} {1,-40} {2,-6} {3,-5}",
                    i++, name, year, game.NumPlays));
            }

            var message = string.Join("\n", lines);

            const int maxMessageLength = 2000;
            if (message.Length > maxMessageLength)
            {
                await SendFileManuallyAsync(message, "games.txt");
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        [Command("wanttoplay")]
        [Description("Retrieve list of games <BGG username> wants to play")]
        public async Task GetWantToPlayGamesAsync(string username)
        {
            var games = await FetchWantToPlayGamesFromBGG(username);
            if (games == null || !games.Any())
            {
                await ReplyAsync("Could not fetch games for the provided username.");
                return;
            }

            var gameNames = games.Select(g => g.Name);
            var message = $"Games {username} wants to play (NOT a wishlist): {string.Join(", ", gameNames)}";

            const int maxMessageLength = 2000;
            if (message.Length > maxMessageLength)
            {
                await SendFileManuallyAsync(message, "wanttoplay.txt");
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        [Command("compare")]
        [Description("Compare <BGG username1> list of want to plays with <BGG username2>")]
        public async Task GetComparisonGamesAsync(string usernameToPlay, string usernameCollection)
        {
            var gamesToPlay = await FetchWantToPlayGamesFromBGG(usernameToPlay);
            if (gamesToPlay == null || !gamesToPlay.Any())
            {
                await ReplyAsync($"Could not fetch 'want to play' games for the provided username {usernameToPlay}.");
                return;
            }

            var gamesInCollection = await FetchGamesFromBGG(usernameCollection);
            if (gamesInCollection == null || !gamesInCollection.Any())
            {
                await ReplyAsync($"Could not fetch collection games for the provided username {usernameCollection}.");
                return;
            }

            var commonGames = gamesToPlay
                .Select(g => g.Name)
                .Intersect(gamesInCollection.Select(g => g.Name))
                .ToList();

            if (!commonGames.Any())
            {
                await ReplyAsync($"No games found where {usernameCollection} has something {usernameToPlay} wants to play.");
                return;
            }

            var message = $"Games that {usernameToPlay} wants to play that {usernameCollection} has: {string.Join(", ", commonGames)}";

            const int maxMessageLength = 2000;
            if (message.Length > maxMessageLength)
            {
                await SendFileManuallyAsync(message, "comparison.txt");
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        public async Task SendFileManuallyAsync(string message, string filename)
        {
            var httpClient = new HttpClient();
            var botToken = _configuration["BotToken"];
            var channelId = Context.Channel.Id;
            var url = $"https://discord.com/api/v10/channels/{channelId}/messages";

            using var content = new MultipartFormDataContent();

            if (message.Length <= 2000)
            {
                content.Add(new StringContent(message), "content");
            }
            else
            {
                content.Add(new StringContent("Here is your game list."), "content");
            }

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(message));
            var streamContent = new StreamContent(memoryStream);
            content.Add(streamContent, "files[0]", filename);

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", botToken);
            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[HTTP ERROR] {response.StatusCode}: {error}");
                await ReplyAsync("Discord rejected the file upload. Error logged.");
            }
        }

        public async Task<List<BoardGame>> FetchGamesFromBGG(string username)
        {
            const int maxRetries = 5;
            const int delayBase = 2000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                var response = await _httpClient.GetAsync($"https://boardgamegeek.com/xmlapi2/collection?own=1&username={username}");

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    await Task.Delay(delayBase * (attempt + 1));
                    continue;
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var xdoc = XDocument.Parse(responseBody);

                    var games = xdoc.Descendants("item")
                        .Select(item => new BoardGame
                        {
                            Name = item.Element("name")?.Value,
                            YearPublished = int.Parse(item.Element("yearpublished")?.Value ?? "0"),
                            ImageUrl = item.Element("image")?.Value,
                            ThumbnailUrl = item.Element("thumbnail")?.Value,
                            NumPlays = int.Parse(item.Element("numplays")?.Value ?? "0")
                        })
                        .ToList();

                    return games;
                }
                break;
            }
            return null;
        }

        public async Task<List<BoardGame>> FetchWantToPlayGamesFromBGG(string username)
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync($"https://boardgamegeek.com/xmlapi2/collection?wanttoplay=1&username={username}"));
            var xdoc = XDocument.Parse(response);

            var games = xdoc.Descendants("item")
                .Select(item => new BoardGame
                {
                    Name = item.Element("name")?.Value,
                    YearPublished = int.Parse(item.Element("yearpublished")?.Value ?? "0"),
                    ImageUrl = item.Element("image")?.Value,
                    ThumbnailUrl = item.Element("thumbnail")?.Value,
                    NumPlays = int.Parse(item.Element("numplays")?.Value ?? "0")
                })
                .ToList();

            return games;
        }

        [Command("plays")]
        public async Task GetPlaysAsync(string username = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                await ReplyAsync("[NOT FINISHED] Please provide a username. Usage: /bgg plays <username>");
                return;
            }
        }
    }
}