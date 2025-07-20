using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordBGGCollection;
using Microsoft.Extensions.Configuration;
using Xunit;

public class CommandsTests
{
    private static BGGCommands CreateTestCommandInstance()
    {
        var httpClient = new HttpClient();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "BotToken", "TEST_TOKEN" } // Dummy token just to satisfy constructor
            })
            .Build();

        return new BGGCommands(httpClient, config);
    }

    [Theory]
    [InlineData("tendimensions")]
    [InlineData("sjkellyfetti")]
    [InlineData("ariaka5")]
    public async Task FetchGamesFromBGG_ReturnsGamesList(string username)
    {
        // Arrange
        var commands = CreateTestCommandInstance();

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
        var commands = CreateTestCommandInstance();

        // Act
        var result = await commands.FetchWantToPlayGamesFromBGG(username);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}