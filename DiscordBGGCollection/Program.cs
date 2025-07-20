using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBGGCollection
{
    internal class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IConfiguration _configuration;

        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug, // Verbose logging
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false
            });

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(new HttpClient())
                .AddSingleton<BGGCommands>()
                .AddSingleton<IConfiguration>(_configuration)
                .BuildServiceProvider();

            string botToken = _configuration["BotToken"];

            _client.Log += Log;
            _commands.Log += Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            var logPrefix = $"[{msg.Severity}] [{msg.Source}]";
            if (msg.Exception is not null)
            {
                Console.WriteLine($"{logPrefix} {msg.Message}\nException: {msg.Exception}");
            }
            else
            {
                Console.WriteLine($"{logPrefix} {msg.Message}");
            }
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);
            int argPos = 0;

            if (message.Content.Trim().Equals("/bgg", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _commands.ExecuteAsync(context, "bgg help", _services);
                if (!result.IsSuccess)
                {
                    Console.WriteLine($"[CommandError] {result.ErrorReason}");
                }
            }
            else if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    Console.WriteLine($"[CommandError] {result.ErrorReason}");

                    if (result.Error == CommandError.UnknownCommand)
                    {
                        var result2 = await _commands.ExecuteAsync(context, "bgg help", _services);
                        if (!result2.IsSuccess)
                        {
                            Console.WriteLine($"[FallbackHelpError] {result2.ErrorReason}");
                        }
                    }
                }
            }
        }
    }
}