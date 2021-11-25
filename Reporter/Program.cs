using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Reporter.Data;
using Reporter.Interaction;

namespace Reporter;

public class Program
{
    // Discord client
    private readonly DiscordSocketClient _client;

    // Service collection
    private readonly IServiceProvider _services;

    // Log instance
    private readonly Logger _logger;

    #region Display

    /// <summary>
    /// Version
    /// </summary>
    public Version Version { get; } 
        = typeof(Program).Assembly.GetName().Version ?? new(1, 0);

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; } 
        = "Reporter";

    /// <summary>
    /// Author
    /// </summary>
    public string Author { get; } 
        = "Rozen4334";

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; } 
        = "A Discord reporter bot for sandbox game servers.";

    #endregion

    /// <summary>
    /// Ctor
    /// </summary>
    public Program()
    {
        _services = ConfigureServices;
        _client = _services.GetRequiredService<DiscordSocketClient>();
        _logger = _services.GetRequiredService<Logger>();
    }

    /// <summary>
    /// Static startup init
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args) => new Program()
        .RunAsync()
        .GetAwaiter()
        .GetResult();

    // Configure all services.
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

    /// <summary>
    /// Runs the client, starts & configures events.
    /// </summary>
    /// <returns></returns>
    public async Task RunAsync()
    {
        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, Config.Settings.BotToken);

        _client.MessageReceived += MessageReceived;
        _client.Ready += Ready;

        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    // Client message received
    private async Task MessageReceived(SocketMessage arg)
    {
        // if message is of type user
        if (arg is not SocketUserMessage message)
            return;

        // if author is in guild or not
        if (message.Author is not SocketGuildUser user)
            return;

        // if message starts with @reporter mention
        int argPos = 0;
        if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            return;

        // if user is of staff or whitelisted in config
        if (!(user.HasRole("Staff") || Config.Settings.WhitelistedUsers.Any(x => x == user.Id)))
            return;

        // if commandname is addimage
        string[] param = message.Content.Trim().Split(' ');
        if (param[1] != "addimage")
            return;

        if (long.TryParse(param[2], out long id))
        {
            var manager = new ReportManager(user.Guild.Id);

            AllowedMentions target = new() { MentionRepliedUser = true };
            if (manager.TryGetReport(id, out var report))
            {
                if (message.Attachments.Any())
                {
                    report.AddImages(message.Attachments);
                    manager.SaveReports();
                    await message.ReplyAsync(":white_check_mark: **Succesfully added image(s) to report!**", allowedMentions: target);
                    return;
                }
                report.AddImages(param[3..]);
                await message.ReplyAsync(":white_check_mark: **Succesfully added image(s) to report!**", allowedMentions: target);
                manager.SaveReports();
            }
            else
            {
                await message.ReplyAsync(":x: **This report ID is invalid!** Please try again by specifying a valid ID.", allowedMentions: target);
                return;
            }
        }
    }

    // Client log
    private async Task Log(LogMessage message) 
        => await _logger.LogAsync(message);

    // Client ready
    private async Task Ready()
    {
        await _client.SetGameAsync($" over Sandbox games", null, ActivityType.Watching);

        await _logger.LogAsync(Name + " Version " + Version);
        await _logger.LogAsync(Description);
        await _logger.LogAsync("Created by: " + Author);

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