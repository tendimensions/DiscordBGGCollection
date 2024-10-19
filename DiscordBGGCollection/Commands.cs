namespace DiscordBGGCollection
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Discord.Commands;

    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _httpClient;

        public Commands(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [Command("games")]
        public async Task GetGamesAsync(string username)
        {
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

        private List<string> SplitMessage(string message, int maxLength)
        {
            var messages = new List<string>();
            for (int i = 0; i < message.Length; i += maxLength)
            {
                messages.Add(message.Substring(i, Math.Min(maxLength, message.Length - i)));
            }
            return messages;
        }
    }
}