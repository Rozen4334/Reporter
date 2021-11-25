using Reporter.Data;
using Reporter.Models;
using System.Text;

namespace Reporter.Interaction;

public class SlashCommands
{
    private readonly Dictionary<string, Func<SocketSlashCommand, ReportManager, Task>> _callback = new();

    private readonly DiscordSocketClient _client;

    private readonly TimeManager _time;

    public SlashCommands(DiscordSocketClient client, TimeManager time)
    {
        _client = client;
        _time = time;

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

    private readonly SlashCommandBuilder[] _builders = new[]
    {
        new SlashCommandBuilder()
            .WithName("report")
            .WithDescription("Reports a user for harming the world/map (Griefing, tunneling & relevant).")
            .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("player")
                .WithDescription("The username of a player.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("The type of offense (Format: grief, tunnel, hack, chat, other).")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("time")
                .WithDescription("History timespan since offense")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("blocksbroken")
                .WithDescription("Blocks broken if applicable. (Set '0' if none.)")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("punishment")
                .WithDescription("The punishment given to the offender.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("note")
                .WithDescription("Additional information")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false)),
        new SlashCommandBuilder()
            .WithName("playerinfo")
            .WithDescription("Gets all reports, filtered by a specific player.")
            .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("player")
                .WithDescription("The username of a player")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)),
        new SlashCommandBuilder()
            .WithName("reportinfo")
            .WithDescription("Views a report for the specified ID.")
            .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("id")
                .WithDescription("The ID of a report.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)),
        new SlashCommandBuilder()
            .WithName("editreport")
            .WithDescription("Edits a report for the specified report ID.")
            .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("id")
                .WithDescription("The ID of the report.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Edit the type of a report.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("time")
                .WithDescription("Edit the time of a report.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("player")
                .WithDescription("Edit the target of a report.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("blocksbroken")
                .WithDescription("Edit the total broken blocks of a report.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("punishment")
                .WithDescription("Edit the punishment of a report.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("note")
                .WithDescription("Edit the note of a report.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false)),
        new SlashCommandBuilder()
            .WithName("reports")
            .WithDescription("Views all reports and their ID")
            .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("page")
                .WithDescription("The page of the list.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(false)),
        new SlashCommandBuilder()
            .WithName("reporterinfo")
            .WithDescription("Gets all data on a reporter, specified by their Discord username.")
            .AddOption(
            new SlashCommandOptionBuilder()
                .WithName("reporter")
                .WithDescription("The reporters Discord username.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
    };

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

    // grief or tunnel
    private async Task ReportWorld(SocketSlashCommand command, ReportManager manager)
    {

    }

    // chat
    private async Task ReportChat(SocketSlashCommand command, ReportManager manager)
    {

    }

    // hacking
    private async Task ReportHack(SocketSlashCommand command, ReportManager manager)
    {

    }

    // other
    private async Task ReportOther(SocketSlashCommand args, ReportManager manager)
    {

    }

    private async Task Report(SocketSlashCommand command, ReportManager manager)
    {
        var data = command.Data.Options.ToArray();
        var builder = new EmbedBuilder().Construct(_client);

        if (!new TimeManager().GetFromString((string)data[2].Value, out DateTime reporttime))
        {
            builder.WithTitle("Invalid syntax");
            builder.WithDescription("Invalid time string.");
            Embed[] embeds = { builder.Build() };
            await command.RespondAsync("", embeds);
            return;
        }

        var component = new ComponentBuilder()
            .WithButton("Exit report", $"id_exit|{command.User.Id}", ButtonStyle.Danger)
            .WithButton("Confirm report", $"id_confirm|{command.User.Id}", ButtonStyle.Success);

        builder.WithTitle("Reporting user");
        builder.WithDescription($"Starting on report for player: ` {data[0].Value} `");

        builder.AddField("Type:", type);
        builder.AddField("Time:", reporttime);
        if (!data[3].Value.ToString().Equals("0"))
            builder.AddField("Blocks broken:", data[3].Value);
        builder.AddField("Punishment:", data[4].Value);
        if (data.Length > 5)
            builder.AddField("Note:", data[5].Value);

        Extensions.PendingEntries.Add(new Report(0, command.User.Id, data[0].Value.ToString(), type, reporttime, data[4].Value.ToString(), Convert.ToInt32((long)data[3].Value), (data.Length > 5) ? data[5].Value.ToString() : ""));

        Embed[] em = { builder.Build() };
        await command.RespondAsync("", em, false, false, null, null, component.Build());
    }

    private async Task ReportInfo(SocketSlashCommand command, ReportManager manager)
    {
        var data = command.Data.Options.ToArray();
        int id = int.Parse(data.First().Value.ToString());

        var builder = new EmbedBuilder().Construct(_client, command.User);

        if (!manager.TryGetReport(id, out var report))
        {
            builder.WithTitle("Invalid syntax");
            builder.WithDescription("This report ID is invalid, please try again by specifying a valid ID.");
            Embed[] embeds = { builder.Build() };
            await command.RespondAsync("", embeds);
            return;
        }

        var component = new ComponentBuilder()
            .WithButton("View images", $"id_img|{command.User.Id}|{report.ID}")
            .WithButton("View all reports", $"id_img|{command.User.Id}|{report.Username}", ButtonStyle.Secondary, new Emoji("📃")); ;

        builder.WithTitle($"Report: ` {report.ID} `");
        builder.AddField("Reported by:", (report.Moderator != 0) ? report.Moderator : "` Unavailable. `");
        builder.AddField("User:", report.Username, true);
        builder.AddField("Type:", report.Type, true);
        builder.AddField("Time:", report.Time);
        if (report.BlocksBroken != 0)
            builder.AddField("Blocks broken:", report.BlocksBroken, true);
        builder.AddField("Punishment:", report.Punishment, true);
        if (report.Note != "")
            builder.AddField("Note:", report.Note);
        if (report.ProofURLs.Any())
            builder.WithImageUrl(report.ProofURLs.First());

        Embed[] em = { builder.Build() };
        await command.RespondAsync("", em, false, false, null, null, component.Build());
    }

    private async Task PlayerInfo(SocketSlashCommand command, ReportManager manager)
    {
        var data = command.Data.Options.ToArray();
        string plr = data.First().Value.ToString() ?? "";
        var reports = manager.GetReports(plr);

        var b = new EmbedBuilder().Construct(_client, command.User);

        if (!reports.Any())
        {
            b.WithTitle("Invalid syntax");
            b.WithDescription($"I found no matches for player: ` {plr} `. Are you sure the name correctly spelled?");
            Embed[] embeds = { b.Build() };
            await command.RespondAsync("", embeds);
            return;
        }

        b.WithTitle($"User information: ` {plr} `");
        b.WithDescription($"I have found ` {reports.Count()} ` report(s) for specified user.");
        int blocks = 0;
        List<string> ids = new();
        foreach (var x in reports)
        {
            blocks += x.BlocksBroken;
            ids.Add($"` {x.ID} ` - type: {x.Type}");
        }
        b.AddField("Total blocks broken:", (blocks != 0) ? blocks : "None");
        b.AddField("Last punishment given:", reports.Last().Punishment);
        b.AddField($"Reports [{ids.Count}]:", string.Join("\n", ids));
        Embed[] em = { b.Build() };
        await command.RespondAsync("", em);
    }

    private async Task EditReport(SocketSlashCommand command, ReportManager manager)
    {
        var data = command.Data.Options.ToArray();
        var id = (long)data.First().Value;
        var b = new EmbedBuilder().Construct(_client, command.User);

        if (!manager.TryGetReport((int)id, out Report reportbyid))
        {
            b.WithTitle("Invalid syntax");
            b.WithDescription("This report ID is invalid, please try again by specifying a valid ID.");
            Embed[] embeds = { b.Build() };
            await command.RespondAsync("", embeds);
            return;
        }

        b.WithTitle($"Editing report: ` {id} `");

        for (int i = 0; i < data.Length; i++)
        {
            switch (data[i].Name)
            {
                case "type":
                    {
                        if (RTypes.Any(x => x.ToLower().Equals(data[i].Value.ToString().ToLower())))
                        {
                            b.AddField("Edited type:", $"**🢒 Old:** ` {reportbyid.Type} `\n**🢒 New:** ` {data[i].Value} `");
                            reportbyid.Type = data[i].Value.ToString();
                        }
                    }
                    break;
                case "note":
                    {
                        b.AddField("Edited note:", $"**🢒 Old:** ` {reportbyid.Note} `\n**🢒 New:** ` {data[i].Value} `");
                        reportbyid.Note = data[i].Value.ToString();
                    }
                    break;
                case "punishment":
                    {
                        b.AddField("Edited punishment:", $"**🢒 Old:** ` {reportbyid.Punishment} `\n**🢒 New:** ` {data[i].Value} `");
                        reportbyid.Punishment = data[i].Value.ToString();
                    }
                    break;
                case "time":
                    {
                        if (TimeSpan.TryParse(data[i].Value.ToString(), out TimeSpan span))
                        {
                            b.AddField("Edited time:", $"**🢒 Old:** ` {reportbyid.Time} `\n**🢒 New:** ` {data[i].Value} `");
                            reportbyid.Time = DateTime.Now.Subtract(span);
                        }
                    }
                    break;
                case "username":
                    {
                        b.AddField("Edited username:", $"**🢒 Old:** ` {reportbyid.Username} `\n**🢒 New:** ` {data[i].Value} `");
                        reportbyid.Username = data[i].Value.ToString();
                    }
                    break;
                case "blocksbroken":
                    {
                        b.AddField("Edited total blocks broken:", $"**🢒 Old:** ` {reportbyid.Username} `\n**🢒 New:** ` {data[i].Value} `");
                        reportbyid.BlocksBroken = int.Parse(data[i].Value.ToString());
                    }
                    break;
            }
        }
        manager.SaveReports();
        Embed[] em = { b.Build() };
        await command.RespondAsync("", em);
    }
    private async Task Reports(SocketSlashCommand command, ReportManager manager)
    {
        int page = 1;
        if (command.Data.Options.Any())
        {
            var data = command.Data.Options.ToArray();
            page = int.Parse(data.First().Value.ToString());
        }
        var builder = new EmbedBuilder().Construct(command.User);

        builder.WithTitle("Report list");
        builder.WithDescription($"Currently viewing page: ` {page} `");

        var reports = manager.GetAllReports();

        if (page < 1)
        {
            builder.WithTitle("Invalid syntax");
            builder.WithDescription("The page value can't be less than 1.");
            Embed[] embeds = { builder.Build() };
            await command.RespondAsync("", embeds);
            return;
        }
        var pages = reports.Count.RoundUp() / 10;

        int max = 10 + reports.Count - (page * 10);
        if (page > pages)
        {
            builder.WithTitle("Invalid syntax");
            builder.WithDescription("The page value can't be greater than the amount of existing reports.");
            Embed[] embeds = { builder.Build() };
            await command.RespondAsync("", embeds);
            return;
        }

        builder.WithFooter($"Reporter | Page {page} of {pages} | {DateTime.UtcNow}", _client.CurrentUser.GetAvatarUrl());

        var component = new ComponentBuilder()
            .WithButton("Previous page", $"id_page|{command.User.Id}|{page - 1}", ButtonStyle.Danger, null, null, (page - 1 == 0) ? true : false)
            .WithButton("Next page", $"id_page|{command.User.Id}|{page + 1}", ButtonStyle.Success, null, null, (page >= pages) ? true : false);

        var users = new List<Report>();
        users.AddRange(reports.FindAll(x => x.ID >= max - 9 && x.ID <= max));
        users.Reverse();
        StringBuilder sb = new();
        foreach (var x in users)
        {
            if (x != null)
            {
                sb.AppendLine($"` {x.ID} ` **{x.Username}** - Type: {x.Type}");
                sb.AppendLine("⤷ Reported by: " + ((x.Agent != 0) ? $"**{_client.GetUser(x.Agent).Username}**" : "Unavailable"));
            }
        }
        builder.AddField($"Reports:", sb.ToString());

        Embed[] em = { builder.Build() };
        await command.RespondAsync("", em, false, false, null, null, component.Build());
    }

    private async Task ReporterInfo(SocketSlashCommand command, ReportManager manager)
    {
        if (command.Data.Options.Any())
            user = (SocketGuildUser)command.Data.Options.ToArray().FirstOrDefault().Value;
        var builder = new EmbedBuilder().Construct(command.User);
        builder.WithTitle($"Info about ` {user.Username} `");

        List<string> roles = new();
        foreach (var r in user.Roles)
        {
            if (!r.IsEveryone)
                roles.Add($"<@&{r.Id}>");
        }
        builder.AddField("Roles:", (roles.Any()) ? string.Join(", ", roles) : "None.");

        var reports = manager.GetReportByAgent(user.Id);

        reports.Reverse();

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
        {
            foreach (var str in strlist)
                sb.AppendLine(str);
        }

        builder.AddField($"Total reports [{reports.Count}]:", sb.ToString());
        Embed[] em = { builder.Build() };
        await command.RespondAsync("", em);
    }
}

