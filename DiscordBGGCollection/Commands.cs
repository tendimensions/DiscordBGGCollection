namespace DiscordBGGCollection
{
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
            if (games == null)
            {
                await ReplyAsync("Could not fetch games for the provided username.");
                return;
            }

            await ReplyAsync($"Games for {username}: {string.Join(", ", games)}");
        }

        public async Task<string[]> FetchGamesFromBGG(string username)
        {
            var response = await _httpClient.GetStringAsync($"https://boardgamegeek.com/xmlapi2/collection?username={username}");
            // Parse the XML response to extract game names
            var games = new List<string>(); // Replace with actual parsing logic
            return games.ToArray();
        }
    }
}