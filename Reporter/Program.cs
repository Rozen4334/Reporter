using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Reporter.Data;

namespace Reporter
{
    public class Program
    {
        public static Config Settings = Config.Read();

        public static DiscordSocketClient Client;

        private bool _RetryConnection = true;
        private const int _RetryInterval = 1000;
        private bool _Running = true;
        private const int _RunningInterval = 1000;

        static void Main(string[] args) 
            => new Program().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            if (Client != null)
            {
                if (Client.ConnectionState == ConnectionState.Connecting ||
                    Client.ConnectionState == ConnectionState.Connected)
                    return;
            }

            Client = new(new()
            {
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All,
            });

            _RetryConnection = true;
            _Running = false;

            while (true)
            {
                try
                {

                    await Client.LoginAsync(TokenType.Bot, Settings.BotToken);

                    await Client.StartAsync();

                    await SetEvents();

                    _Running = true;

                    break;
                }
                catch
                {
                    await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Failed to connect."));
                    if (_RetryConnection == false)
                        return;

                    await Task.Delay(_RetryInterval);
                }
            }

            while (_Running) { await Task.Delay(_RunningInterval); }

            if (Client.ConnectionState == ConnectionState.Connecting ||
                Client.ConnectionState == ConnectionState.Connected)
            {
                try { Client.StopAsync().Wait(); }
                catch { }
            }
        }

        private async Task SetEvents()
        {
            if (Client.LoginState != LoginState.LoggedIn)
                return;

            Client.MessageReceived += MessageReceived;
            Client.InteractionCreated += InteractionRecieved;
            Client.UserUpdated += UserUpdated;

            Client.Ready += Ready;
            Client.Log += Log;

            await Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage args)
        {
            if (args is SocketUserMessage message)
            {
                if (message == null)
                    return;
                int argPos = 0;

                if (!message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                    return;

                if (args.Author is SocketGuildUser user)
                {
                    if (!user.HasRole("Staff"))
                        return;

                    if (message.Content.Contains("writeappcommands"))
                    {
                        await Extensions.WriteAppCommands(user.Guild);
                        await message.ReplyAsync("Succesfully created application commands. Ready for use!", 
                            false, null, new AllowedMentions() { MentionRepliedUser = true });
                    }
                    if (message.Content.Contains("deleteappcommands"))
                    {
                        await Extensions.RemoveAppCommands(user.Guild);
                        await message.ReplyAsync("Succesfully removed application commands. Use ` {mention} writeappcommands ` to write all app commands", 
                            false, null, new AllowedMentions() { MentionRepliedUser = true });
                    }
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
                                    manager.SaveUsers();
                                    await message.ReplyAsync("Succesfully added image(s) to report.", 
                                        false, null, new AllowedMentions() { MentionRepliedUser = true });
                                    return;
                                }
                                report.ProofURLs.Add(input[3]);
                                await message.ReplyAsync("Succesfully added image to report.", 
                                    false, null, new AllowedMentions() { MentionRepliedUser = true });
                                manager.SaveUsers();
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
            }
        }

        private async Task InteractionRecieved(SocketInteraction args)
        {
            if (args is SocketSlashCommand command)
                await new Commands().CommandHandler(command);
            else if (args is SocketMessageComponent component)
                await new Components().InteractionHandler(component);
        }

        private async Task UserUpdated(SocketUser entry, SocketUser result)
        {
            if (entry is SocketGuildUser old && result is SocketGuildUser user)
                if (old.Nickname != user.Nickname)
                {
                    var nick = user.Nickname.ToLower();
                    if (nick.Contains("[c/") || nick.Contains("[i"))
                    {
                        await user.ModifyAsync(x => x.Nickname = user.Username);
                        try
                        {
                            var channel = await user.CreateDMChannelAsync();
                            await channel.SendMessageAsync($":warning: **{user.Guild.Name}**, Your nickname violates one of these naming rules and has been replaced with your normal username:\n` No color codes `\n` No item codes `");
                        }
                        catch { throw; }
                    }
                }
        }

        private async Task Log(LogMessage message)
            => Console.WriteLine(message.ToString());

        private async Task Ready()
            => await Client.SetGameAsync($" over T1", null, ActivityType.Watching);
    }
}
