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

        static void Main(string[] args) => new Program()
            .RunAsync()
            .GetAwaiter()
            .GetResult();

        public Program()
        {
            _services = ConfigureServices;
            _client = _services.GetRequiredService<DiscordSocketClient>();
        }

        public async Task RunAsync()
        {
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, Config.Settings.BotToken);

            _client.MessageReceived += MessageReceived;
            _client.Ready += Ready;

            _services.GetRequiredService<SlashCommands>().Configure();

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
                string[] input = message.Content.Trim().Split(' ');

                if (input.Length > 4)
                {
                    await message.ReplyAsync("Invalid syntax, only add one image per execution.",
                        false, null, new AllowedMentions() { MentionRepliedUser = true });
                    return;
                }

                if (int.TryParse(input[2], out int id))
                {
                    var manager = new ReportManager(user.Guild.Id);

                    if (manager.GetReportByID(id, out Report report))
                    {
                        if (message.Attachments.Any())
                        {
                            report.AddImages(message.Attachments.ToList());
                            manager.SaveReports();
                            await message.ReplyAsync("Succesfully added image(s) to report.",
                                false, null, new AllowedMentions() { MentionRepliedUser = true });
                            return;
                        }
                        report.ProofURLs.Add(input[3]);
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

        private async Task Log(LogMessage message) => await Task.Run(() =>
            Console.WriteLine(message.ToString()));

        private async Task Ready()
            => await _client.SetGameAsync($" over Sandbox games", null, ActivityType.Watching);
    }
}
