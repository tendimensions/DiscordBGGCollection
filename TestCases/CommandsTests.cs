using System.Net.Http;
using System.Threading.Tasks;
using DiscordBGGCollection;
using Xunit;

public class CommandsTests
{
    [Theory]
    [InlineData("tendimensions")]
    [InlineData("sjkellyfetti")]
    [InlineData("ariaka5")]
    public async Task FetchGamesFromBGG_ReturnsGamesList(string username)
    {
        // Arrange
        var httpClient = new HttpClient();
        var commands = new BGGCommands(httpClient);

        // Act
        var result = await commands.FetchGamesFromBGG(username);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("tendimensions")]
    [InlineData("sjkellyfetti")]
    [InlineData("ariaka5")]
    public async Task FetchWantToPlayGamesFromBGG_ReturnsGamesList(string username)
    {
        // Arrange
        var httpClient = new HttpClient();
        var commands = new Commands(httpClient);

        // Act
        var result = await commands.FetchWantToPlayGamesFromBGG(username);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}