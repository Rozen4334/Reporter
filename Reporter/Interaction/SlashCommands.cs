using Reporter.Data;
using Reporter.Models;
using System.Text;

namespace Reporter.Interaction;

public class SlashCommands
{
    private readonly Dictionary<string, Func<SocketSlashCommand, ReportManager, Task>> _callback = new();

    private readonly DiscordSocketClient _client;

    private readonly TimeManager _time;

    private readonly Logger _logger;

    public SlashCommands(DiscordSocketClient client, TimeManager time, Logger logger)
    {
        _client = client;
        _time = time;
        _logger = logger;

        _callback["report"] = Report;
        _callback["reportinfo"] = ReportInfo;
        _callback["playerinfo"] = PlayerInfo;
        _callback["editreport"] = EditReport;
        _callback["reports"] = Reports;
        _callback["reporterinfo"] = ReporterInfo;
    }

    public IEnumerable<ApplicationCommandProperties> Configure()
    {
        _client.SlashCommandExecuted += HandleAsync;

        if (Config.Settings.WriteCommands)
        {
            foreach (var b in _builders)
            {
                yield return b.Build();
            }
        }
    }

    #region Builders
    private readonly SlashCommandBuilder[] _builders = new[]
    {
        // Report command
        new SlashCommandBuilder()
        .WithName("report")
        .WithDescription("Reports a user for harming the world/map (Griefing, tunneling & relevant).")
        .AddOption("player", ApplicationCommandOptionType.String, "The player to report.", true)
        .AddOption("punishment", ApplicationCommandOptionType.String, "The punishment given to said player.", true)
        .AddOption("timespan", ApplicationCommandOptionType.String, "The timespan since this offense.", false)
        .AddOption("blocks-broken", ApplicationCommandOptionType.Integer, "The amount of tiles griefed.", false)
        .AddOption("note", ApplicationCommandOptionType.String, "Potential note to add to this report.", false),

        // Player info command
        new SlashCommandBuilder()
        .WithName("playerinfo")
        .WithDescription("Gets all reports, filtered by a specific player.")
        .AddOption("player", ApplicationCommandOptionType.String, "The player to look up.", true),
        
        // Report info command
        new SlashCommandBuilder()
        .WithName("reportinfo")
        .WithDescription("Views a report for the specified ID.")
        .AddOption("id", ApplicationCommandOptionType.Integer, "The ID to look up.", true),

        // Edit report command
        new SlashCommandBuilder()
        .WithName("editreport")
        .WithDescription("Edits a report for the specified report ID.")
        .AddOption("player", ApplicationCommandOptionType.String, "The player's name.", false)
        .AddOption("punishment", ApplicationCommandOptionType.String, "The punishment given to said player.", false)
        .AddOption("timespan", ApplicationCommandOptionType.String, "The timespan since this offense.", false)
        .AddOption("blocks-broken", ApplicationCommandOptionType.Integer, "The amount of tiles griefed.", false)
        .AddOption("note", ApplicationCommandOptionType.String, "Potential note to add to this report.", false)
        .AddOption("type", ApplicationCommandOptionType.Integer, "The report's type: 'grief | tunnel | hack | chat | other'.", false),

        // List reports command
        new SlashCommandBuilder()
        .WithName("reports")
        .WithDescription("Views all reports and their ID")
        .AddOption("page", ApplicationCommandOptionType.Integer, "The page to list.", false),

        // Reporter info command
        new SlashCommandBuilder()
        .WithName("reporterinfo")
        .WithDescription("Gets all data on a reporter, specified by their Discord username.")
        .AddOption("user", ApplicationCommandOptionType.User,"The reporter to look up.", false),
    };
    #endregion

    private async Task HandleAsync(SocketSlashCommand command)
    {
        if (_callback.TryGetValue(command.CommandName, out var result))
        {
            if (command.User is SocketGuildUser user)
                await result(command, new(user.Guild.Id));
            // else return log
        }
        // else return log
    }

    private async Task Report(SocketSlashCommand command, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client);
        var c = new ComponentBuilder();
        var d = command.Data.Options.ToArray();
        var s = new SelectMenuBuilder()
            .WithCustomId($"id_confirm|{command.User.Id}")
            .WithPlaceholder("Select any type to confirm report.")
            .AddOption("Grief", "0", "Griefing/purposefully destroying builds.")
            .AddOption("Tunnel", "1", "Tunnelling, mass terraforming or making hellevators")
            .AddOption("Chat", "2", "An offense in chat, like NSFW or toxicity.")
            .AddOption("Hack", "3", "A hacking player.")
            .AddOption("Other", "4", "A different kind of offense.")
            .WithMinValues(1)
            .WithMaxValues(1);
        c.WithButton("Exit report", $"id_exit|{command.User.Id}", ButtonStyle.Danger, row: 1);
        c.WithSelectMenu(s, 0);

        string punishment = (string)d.First(x => x.Name == "punishment").Value;
        string player = (string)d.First(x => x.Name == "player").Value;

        string note = "";
        if (d.Any(x => x.Name == "note"))
            note = (string)d.First(x => x.Name == "note").Value;

        long blocks = 0;
        if (d.Any(x => x.Name == "blocks-broken"))
            blocks = (long)d.First(x => x.Name == "blocks-broken").Value;

        DateTime span = DateTime.UtcNow;
        if (d.Any(x => x.Name == "timespan"))
            if (!_time.TryGetFromString((string)d.First(x => x.Name == "timespan").Value, out span))
            {
                await command.RespondAsync(":x: **Invalid time!** The time you entered is not a valid span, and cannot be parsed.");
                return;
            }

        b.WithTitle("Reporting user");
        b.WithDescription($"Starting on report for player: ` {d[0].Value} `");

        b.AddField("Time:", span);
        if (blocks != 0)
            b.AddField("Blocks broken:", blocks);
        b.AddField("Punishment:", punishment);
        if (!string.IsNullOrEmpty(note))
            b.AddField("Note:", note);

        Extensions.Pending.Add(new Report(player, command.User.Id, span, punishment, blocks, note));

        await command.RespondAsync(embed: b.Build(), component: c.Build());
    }

    private async Task ReportInfo(SocketSlashCommand command, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client, command.User);
        var c = new ComponentBuilder();
        var d = command.Data.Options.ToArray();
        long id = (long)d.First().Value;

        if (!manager.TryGetReport(id, out var report))
        {
            await command.RespondAsync(":x: **Report ID invalid!** Please try again specifying an existing ID.");
            return;
        }
        c.WithButton("View images", $"id_img|{command.User.Id}|{report.ID}");
        c.WithButton("View all reports", $"id_img|{command.User.Id}|{report.Username}", ButtonStyle.Secondary, new Emoji("📃")); 

        b.WithTitle($"Report: ` {report.ID} `");
        b.AddField("Reported by:", (report.Moderator != 0) ? report.Moderator : "` Unavailable. `");
        b.AddField("User:", report.Username, true);
        b.AddField("Type:", report.Type.ToString(), true);
        b.AddField("Time:", report.Time);
        if (report.BlocksBroken != 0)
            b.AddField("Blocks broken:", report.BlocksBroken, true);
        b.AddField("Punishment:", report.Punishment, true);
        if (report.Note != "")
            b.AddField("Note:", report.Note);
        if (report.ProofURLs.Any())
            b.WithImageUrl(report.ProofURLs.First());

        await command.RespondAsync(embed: b.Build(), component: c.Build());
    }

    private async Task PlayerInfo(SocketSlashCommand command, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client, command.User);
        var d = command.Data.Options.ToArray();
        var player = (string)d.First().Value;
        var reports = manager.GetReports(player);

        if (!reports.Any())
        {
            await command.RespondAsync($":x: **No matches found!** Is the name: {player} correctly spelled? If so, I do not have any reports on them.");
            return;
        }

        b.WithTitle($"User information: ` {player} `");
        b.WithDescription($"I have found ` {reports.Count()} ` report(s) for specified user.");
        long blocks = 0;
        List<string> ids = new();
        foreach (var x in reports)
        {
            blocks += x.BlocksBroken;
            ids.Add($"` {x.ID} ` - type: {x.Type}");
        }

        b.AddField("Total blocks broken:", (blocks != 0) ? blocks : "None");
        b.AddField("Last punishment given:", reports.Last().Punishment);
        b.AddField($"Reports [{ids.Count}]:", string.Join("\n", ids));

        await command.RespondAsync(embed: b.Build());
    }

    private async Task EditReport(SocketSlashCommand command, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client, command.User);
        var d = command.Data.Options.ToArray();
        var id = (long)d.First().Value;

        if (!manager.TryGetReport(id, out Report report))
        {
            await command.RespondAsync(":x: **Report ID invalid!** Please try again specifying an existing ID.");
            return;
        }

        b.WithTitle($"Editing report: ` {id} `");

        for (int i = 0; i < d.Length; i++)
        {
            switch (d[i].Name)
            {
                case "type":
                    if (Enum.TryParse(typeof(ReportType), (string)d[i].Value, out var result))
                    {
                        b.AddField("Edited type:", $"**🢒 Old:** ` {report.Type} `\n**🢒 New:** ` {d[i].Value} `");
                        if (result is ReportType type)
                            report.Type = type;
                    }
                    break;
                case "note":
                    b.AddField("Edited note:", $"**🢒 Old:** ` {report.Note} `\n**🢒 New:** ` {d[i].Value} `");
                    report.Note = (string)d[i].Value;
                    break;
                case "punishment":
                    b.AddField("Edited punishment:", $"**🢒 Old:** ` {report.Punishment} `\n**🢒 New:** ` {d[i].Value} `");
                    report.Punishment = (string)d[i].Value;
                    break;
                case "time":
                    if (TimeSpan.TryParse(d[i].Value.ToString(), out TimeSpan span))
                    {
                        b.AddField("Edited time:", $"**🢒 Old:** ` {report.Time} `\n**🢒 New:** ` {d[i].Value} `");
                        report.Time = DateTime.Now.Subtract(span);
                    }
                    break;
                case "username":
                    b.AddField("Edited username:", $"**🢒 Old:** ` {report.Username} `\n**🢒 New:** ` {d[i].Value} `");
                    report.Username = (string)d[i].Value;
                    break;
                case "blocksbroken":
                    b.AddField("Edited total blocks broken:", $"**🢒 Old:** ` {report.Username} `\n**🢒 New:** ` {d[i].Value} `");
                    report.BlocksBroken = (long)d[i].Value;
                    break;
            }
        }
        manager.SaveReports();
        await command.RespondAsync(embed: b.Build());
    }
    private async Task Reports(SocketSlashCommand command, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client, command.User);
        var c = new ComponentBuilder();

        int page = 1;
        if (command.Data.Options.Any())
        {
            var data = command.Data.Options.ToArray();
            page = Convert.ToInt32((long)data.First().Value);
        }

        b.WithTitle("Report list");
        b.WithDescription($"Currently viewing page: ` {page} `");

        var reports = manager.GetAllReports().ToList();

        if (page < 1)
        {
            await command.RespondAsync(":x: **Invalid page!** The page value cannot be less than 1.");
            return;
        }
        var pages = reports.Count.RoundUp() / 10;

        int max = 10 + reports.Count - (page * 10);
        if (page > pages)
        {
            await command.RespondAsync(":x: **Invalid page!** The page value cannot be more than the amount of available pages.");
            return;
        }

        b.WithFooter($"Reporter | Page {page} of {pages} | {DateTime.UtcNow}", _client.CurrentUser.GetAvatarUrl());

        c.WithButton("Previous page", $"id_page|{command.User.Id}|{page - 1}", ButtonStyle.Danger, null, null, page - 1 == 0);
        c.WithButton("Next page", $"id_page|{command.User.Id}|{page + 1}", ButtonStyle.Success, null, null, page >= pages);

        var users = new List<Report>();
        users.AddRange(reports.FindAll(x => x.ID >= max - 9 && x.ID <= max));
        users.Reverse();
        StringBuilder sb = new();
        if (users.Any())
            foreach (var x in users)
            {
                sb.AppendLine($"` {x.ID} ` **{x.Username}** - Type: {x.Type}");
                sb.AppendLine("⤷ Reported by: " + ((x.Moderator != 0) ? $"**{_client.GetUser(x.Moderator).Username}**" : "Unavailable"));
            }

        b.AddField($"Reports:", sb.ToString());

        await command.RespondAsync(embed: b.Build(), component: c.Build());
    }

    private async Task ReporterInfo(SocketSlashCommand command, ReportManager manager)
    {
        if (command.User is not SocketGuildUser user)
            return;
        var b = new EmbedBuilder().Construct(_client, command.User);
        var d = command.Data.Options.ToArray();

        if (d.Any())
            user = (SocketGuildUser)d.First().Value;

        b.WithTitle($"Info about ` {user.Username} `");

        List<string> roles = new();
        foreach (var r in user.Roles)
            if (!r.IsEveryone)
                roles.Add($"<@&{r.Id}>");

        b.AddField("Roles:", (roles.Any()) ? string.Join(", ", roles) : "None.");

        var reports = manager.GetReportByAgent(user.Id).Reverse();

        List<string> strlist = new();
        foreach (var x in reports)
        {
            if (strlist.Count > 15)
            {
                strlist.Add("Unable to display additional reports. Displaying latest ` 15 `.");
                break;
            }
            strlist.Add($"` {x.ID} ` - type: {x.Type}");
        }

        StringBuilder sb = new();
        if (strlist.Count != 0)
            foreach (var str in strlist)
                sb.AppendLine(str);

        b.AddField($"Total reports [{reports.Count()}]:", sb.ToString());
        Embed[] em = { b.Build() };
        await command.RespondAsync("", em);
    }
}

