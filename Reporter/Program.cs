using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Reporter.Data;
using Reporter.Interaction;

namespace Reporter
{
    public class Program
    {
        private readonly DiscordSocketClient _client;

        private readonly IServiceProvider _services;

        private readonly Logger _logger;

        static void Main(string[] args) => new Program()
            .RunAsync()
            .GetAwaiter()
            .GetResult();

        public Program()
        {
            _services = ConfigureServices;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _logger = _services.GetRequiredService<Logger>();
        }

        public async Task RunAsync()
        {
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, Config.Settings.BotToken);

            _client.MessageReceived += MessageReceived;
            _client.Ready += Ready;

            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private static IServiceProvider ConfigureServices => new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new()
            {
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All
            }))
            .AddSingleton(new Logger())
            .AddSingleton<TimeManager>()
            .AddSingleton<SlashCommands>()
            .AddSingleton<Components>()
            .BuildServiceProvider();

        private async Task MessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message)
                return;
            if (message.Author is not SocketGuildUser user)
                return;

            int argPos = 0;
            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;
            if (!(user.Id == 539535197935239179 || user.HasRole("Staff")))
                return;
            if (message.Content.Contains("addimage"))
            {
                string[] param = message.Content.Trim().Split(' ');

                if (long.TryParse(param[2], out long id))
                {
                    var manager = new ReportManager(user.Guild.Id);

                    if (manager.TryGetReport(id, out var report))
                    {
                        if (message.Attachments.Any())
                        {
                            report.AddImages(message.Attachments);
                            manager.SaveReports();
                            await message.ReplyAsync("Succesfully added image(s) to report.",
                                false, null, new AllowedMentions() { MentionRepliedUser = true });
                            return;
                        }
                        report.AddImages(param[3..]);
                        await message.ReplyAsync("Succesfully added image to report.",
                            false, null, new AllowedMentions() { MentionRepliedUser = true });
                        manager.SaveReports();
                    }
                    else
                    {
                        await message.ReplyAsync("This report ID is invalid, please try again by specifying a valid ID.",
                            false, null, new AllowedMentions() { MentionRepliedUser = true });
                        return;
                    }
                }
            }
        }

        private async Task Log(LogMessage message) 
            => await _logger.LogAsync(message);

        private async Task Ready()
        {
            await _client.SetGameAsync($" over Sandbox games", null, ActivityType.Watching);

            _services.GetRequiredService<Components>().Configure();

            var cmd = _services.GetRequiredService<SlashCommands>().Configure();

            if (cmd.Any())
            {
                await _logger.LogAsync("Succesfully registered slash commands to all available guilds!");

                foreach (var guild in _client.Guilds)
                    await guild.BulkOverwriteApplicationCommandAsync(cmd.ToArray());
            }
        }
    }
}
