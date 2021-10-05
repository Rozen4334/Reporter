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
    class Program
    {
        public static Config Settings = Config.Read();

        public static DiscordSocketClient Client;
        private static readonly DiscordSocketConfig _ClientConfig = new()
        {
            MessageCacheSize = 100,
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.All,
        };

        private bool _RetryConnection = true;
        private const int _RetryInterval = 1000;
        private bool _Running = true;
        private const int _RunningInterval = 1000;

        static void Main(string[] args) 
            => new Program().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            // checking for invalid run state.
            if (Client != null)
            {
                if (Client.ConnectionState == ConnectionState.Connecting ||
                    Client.ConnectionState == ConnectionState.Connected)
                    return;
            }

            // set the empty variables, override the preset variables.
            Client = new DiscordSocketClient(_ClientConfig);

            _RetryConnection = true;
            _Running = false;

            // while for client connection.
            while (true)
            {
                try
                {

                    await Client.LoginAsync(TokenType.Bot, Settings.BotToken);

                    await Client.StartAsync();

                    await Initialize();

                    _Running = true;

                    break;
                }
                catch
                {
                    await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Failed to connect."));
                    if (_RetryConnection == false)
                    {
                        return;
                    }
                    // making sure the client retries to connect and doesnt escape the while
                    await Task.Delay(_RetryInterval);
                }
            }

            // while for breaking the connection in case it disconnects
            while (_Running) { await Task.Delay(_RunningInterval); }

            if (Client.ConnectionState == ConnectionState.Connecting ||
                Client.ConnectionState == ConnectionState.Connected)
            {
                try { Client.StopAsync().Wait(); }
                catch { }
            }
        }

        private async Task Initialize()
        {
            if (Client.LoginState != LoginState.LoggedIn)
                return;

            // Message recieved & command handler.
            Client.MessageReceived += MessageReceived;
            Client.InteractionCreated += InteractionRecieved;
            Client.UserUpdated += UserUpdated;

            Client.Ready += Ready;
            Client.Log += Log;

            await Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage args)
        {
            // pass the message on as usermessage
            var message = args as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            if (message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                if (args.Author is SocketGuildUser user)
                {
                    if (!user.HasRole("Staff"))
                        return;
                    if (message.Content.Contains("writeappcommands"))
                    {
                        if (message.Channel is ITextChannel channel)
                        {
                            await Extensions.WriteAppCommands(channel.Guild);
                            await channel.SendMessageAsync("Succesfully created application commands. Ready for use!");
                        }
                    }
                    if (message.Content.Contains("deleteappcommands"))
                    {
                        if (message.Channel is ITextChannel channel)
                        {
                            await Extensions.RemoveAppCommands(channel.Guild);
                            await channel.SendMessageAsync("Succesfully removed application commands. Use ` {mention} writeappcommands ` to write all app commands");
                        }
                    }
                    if (message.Content.Contains("addimage"))
                    {
                        if (message.Channel is ITextChannel channel)
                        {
                            string[] input = message.Content.Trim().Split(' ');

                            if (input.Length > 4)
                            {
                                await channel.SendMessageAsync("Invalid syntax, only add one image per execution.");
                                return;
                            }

                            if (int.TryParse(input[2], out int id))
                            {
                                var report = Reports.GetReportByID(id);

                                if (message.Attachments.Count != 0)
                                {
                                    foreach (var x in message.Attachments)
                                    {
                                        report.ProofURLs.Add(x.Url);
                                    }
                                    Reports.SaveUsers();
                                    await channel.SendMessageAsync("Succesfully added image(s) to report.");
                                    return;
                                }
                                report.ProofURLs.Add(input[3]);
                                await channel.SendMessageAsync("Succesfully added image to report.");
                                Reports.SaveUsers();
                            }
                        }
                    }
                }
            }
        }

        private async Task InteractionRecieved(SocketInteraction args)
        {
            switch(args.Type)
            {
                case InteractionType.ApplicationCommand:
                        await Commands.CommandHandler(args);
                    break;
                case InteractionType.MessageComponent:
                        await Interactions.InteractionHandler(args);
                    break;
            }
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
                            await channel.SendMessageAsync(":warning: **Terraria One:**, Your nickname violates one of these naming rules and has been replaced with your normal username:\n` No color codes `\n` No item codes `");
                        }
                        catch { }
                    }
                }
        }

        public async Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            await Task.CompletedTask;
        }

        private async Task Ready()
        {
            await Client.SetGameAsync($" over T1", null, ActivityType.Watching);
        }
    }
}
