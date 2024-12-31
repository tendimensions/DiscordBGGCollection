namespace DiscordBGGCollection
{
    using System.Collections.Generic;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Discord.Commands;

    [Group("bgg")]
    public class BGGCommands : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _httpClient;

        public BGGCommands(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var helpMessage = "Available commands:\n" +
                              "/bgg help - Lists all available commands.\n" +
                              "/bgg games <username> - Fetches games for the specified BGG username.\n" +
                              "/bgg plays <username> - Fetches play statistics for the specified BGG username.";

            await ReplyAsync(helpMessage);
        }

        [Command("games")]
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

            var gameNames = games.Select(g => g.Name);
            var message = $"Games for {username}: {string.Join(", ", gameNames)}";

            // Split the message if it exceeds Discord's message length limit
            const int maxMessageLength = 2000;
            if (message.Length > maxMessageLength)
            {
                var messages = SplitMessage(message, maxMessageLength);
                foreach (var msg in messages)
                {
                    await ReplyAsync(msg);
                }
                // Looking to replace with a file download instead
                //using (var memoryStream = new MemoryStream())
                //{
                //    using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
                //    {
                //        await writer.WriteAsync(message);
                //        await writer.FlushAsync();
                //    }

                //    memoryStream.Position = 0;
                //    await Context.Channel.SendFileAsync(memoryStream, "games.txt", $"Games for {username}:");
                //}
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        [Command("wanttoplay")]
        public async Task GetWantToPlayGamesAsync(string username)
        {
            var games = await FetchWantToPlayGamesFromBGG(username);
            if (games == null || !games.Any())
            {
                await ReplyAsync("Could not fetch games for the provided username.");
                return;
            }

            var gameNames = games.Select(g => g.Name);
            var message = $"Games for {username}: {string.Join(", ", gameNames)}";

            // Split the message if it exceeds Discord's message length limit
            const int maxMessageLength = 2000;
            if (message.Length > maxMessageLength)
            {
                var messages = SplitMessage(message, maxMessageLength);
                foreach (var msg in messages)
                {
                    await ReplyAsync(msg);
                }
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        public async Task<List<BoardGame>> FetchGamesFromBGG(string username)
        {
            var response = await _httpClient.GetStringAsync($"https://boardgamegeek.com/xmlapi2/collection?own=1&username={username}");
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

        public async Task<List<BoardGame>> FetchWantToPlayGamesFromBGG(string username)
        {
            var response = await _httpClient.GetStringAsync($"https://boardgamegeek.com/xmlapi2/collection?wanttoplay=1&username={username}");
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

        private List<string> SplitMessage(string message, int maxLength)
        {
            var messages = new List<string>();
            for (int i = 0; i < message.Length; i += maxLength)
            {
                messages.Add(message.Substring(i, Math.Min(maxLength, message.Length - i)));
            }
            return messages;
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