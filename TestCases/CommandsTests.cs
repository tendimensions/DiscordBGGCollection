using System.Net.Http;
using System.Threading.Tasks;
using DiscordBGGCollection;
using Xunit;

public class CommandsTests
{
    [Fact]
    public async Task FetchGamesFromBGG_ReturnsGamesList()
    {
        // Arrange
        var username = "tendimensions";
        var httpClient = new HttpClient();
        var commands = new Commands(httpClient);

        // Act
        var result = await commands.FetchGamesFromBGG(username);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}