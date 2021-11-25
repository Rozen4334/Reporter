﻿using Reporter.Data;
using Reporter.Models;
using System.Text;

namespace Reporter.Interaction;

public class Components
{
    private readonly Dictionary<string, Func<SocketMessageComponent, string[], ReportManager, Task>> _callback = new();

    private readonly DiscordSocketClient _client;

    public Components(DiscordSocketClient client)
    {
        _client = client;

        _callback["id_exit"] = Exit;
        _callback["id_confirm"] = Confirm;
        _callback["id_img"] = Images;
        _callback["id_view"] = ViewReport;
        _callback["id_page"] = ReportPages;
        _callback["id_viewall"] = ViewAll;
    }

    public void Configure()
    {
        _client.ButtonExecuted += HandleAsync;
        _client.SelectMenuExecuted += HandleAsync;
    }

    private async Task HandleAsync(SocketMessageComponent component)
    {
        string[] arg = component.Data.CustomId.Split('|');
        if (_callback.TryGetValue(arg[0], out var result))
        {
            if (component.User is SocketGuildUser user)
                await result(component, arg.Length > 1 ? arg[1..] : Array.Empty<string>(), new(user.Guild.Id));
            // else return log
        }
        // else return log
    }

    private async Task Exit(SocketMessageComponent component, string[] arg, ReportManager manager)
    {
        var builder = new EmbedBuilder().Construct(_client, component.User).WithColor(Color.Red);
        builder.WithTitle("Canceled report creation");

        Extensions.PendingEntries.RemoveAll(x => x.Moderator == component.User.Id);

        var message = component.Message;
        await message.ModifyAsync(x => x.Components = null);
        await message.ModifyAsync(x => x.Embed = builder.Build());
        await component.DeferAsync();
    }

    private async Task Confirm(SocketMessageComponent component, string[] arg, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client, component.User).WithColor(Color.Green);
        b.WithTitle("Succesfully wrote report");

        Report? entry = Extensions.PendingEntries.Find(x => x.Moderator == component.User.Id);
        if (!entry.HasValue)
        {
            await component.RespondAsync("Report not found. This could be caused by an error on the bot, or because it was restarted with this pending report open.");
            return;
        }

        var report = manager.AddReport(entry.Value);
        Extensions.PendingEntries.Remove(entry.Value);

        b.WithDescription($"Created report for: ` {report.Username} ` with ID: ` {report.ID} `");

        var c = new ComponentBuilder().WithButton("View report", $"id_view|{component.User.Id}|{report.ID}");

        var message = component.Message;
        await message.ModifyAsync(x => x.Components = c.Build());
        await message.ModifyAsync(x => x.Embed = b.Build());
        await component.DeferAsync();
    }

    private async Task Images(SocketMessageComponent component, string[] arg, ReportManager manager)
    {
        int id = int.Parse(arg[0]);
        var builder = new EmbedBuilder().Construct(_client, component.User);
        builder.WithTitle($"Displaying all images for report: ` {id} `");

        if (manager.TryGetReport(id, out var report))
        {
            if (!report.ProofURLs.Any())
                builder.WithDescription("No images to display. Add images with ` @reporter addimage <reportID> <image> ` (or attach an image in the message youre sending");
            Embed[] embeds = { builder.Build() };
            await component.RespondAsync("", embeds);
            if (report.ProofURLs.Any())
                foreach (var x in report.ProofURLs)
                    await component.Channel.SendMessageAsync(x);
        }
    }

    private async Task ViewReport(SocketMessageComponent component, string[] arg, ReportManager manager)
    {
        int id = int.Parse(arg[0]);
        var message = component.Message;
        var b = new EmbedBuilder().Construct(_client, component.User);

        if (manager.TryGetReport(id, out var report))
        {
            var c = new ComponentBuilder()
                .WithButton("View images", $"id_img|{component.User.Id}|{report.ID}")
                .WithButton("View all reports", $"id_img|{component.User.Id}|{report.Username}", ButtonStyle.Secondary, new Emoji("📃"));

            b.WithTitle($"Report: ` {report.ID} `");
            b.AddField("User:", report.Username, true);
            b.AddField("Type:", report.Type, true);
            b.AddField("Time:", report.Time);
            if (report.BlocksBroken != 0)
                b.AddField("Blocks broken:", report.BlocksBroken, true);
            b.AddField("Punishment:", report.Punishment, true);
            if (report.Note != "")
                b.AddField("Note:", report.Note);
            if (report.ProofURLs.Count != 0)
                b.WithImageUrl(report.ProofURLs.First());

            await message.ModifyAsync(x => x.Components = c.Build());
            await message.ModifyAsync(x => x.Embed = b.Build());
            await component.DeferAsync();
        }
    }

    private async Task ViewAll(SocketMessageComponent component, string[] arg, ReportManager manager)
    {
        var b = new EmbedBuilder().Construct(_client, component.User);
        var reports = manager.GetReports(arg[0]);
        var message = component.Message;

        if (reports.Any())
        {
            b.WithTitle($"User information: ` {arg[0]} `");
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
        }
        else
        {
            b.WithTitle($"No reports found!");
            b.WithDescription("This user does not have any known reports.");
        }
        await message.ModifyAsync(x => x.Components = null);
        await message.ModifyAsync(x => x.Embed = b.Build());
        await component.DeferAsync();
    }

    private async Task ReportPages(SocketMessageComponent component, string[] arg, ReportManager manager)
    {
        int page = int.Parse(arg[0]);
        var message = component.Message;
        var b = new EmbedBuilder().Construct(_client, component.User);

        b.WithTitle("Report list");
        b.WithDescription($"Currently viewing page: ` {page} `");

        var reports = manager.GetAllReports().ToList();

        int max = 10 + reports.Count - (page * 10);

        var pages = reports.Count.RoundUp() / 10;

        b.WithFooter($"Reporter | Page {page} of {pages} | {DateTime.UtcNow}", _client.CurrentUser.GetAvatarUrl());

        var c = new ComponentBuilder()
            .WithButton("Previous page", $"id_page|{component.User.Id}|{page - 1}", ButtonStyle.Danger, null, null, (page - 1 == 0) ? true : false)
            .WithButton("Next page", $"id_page|{component.User.Id}|{page + 1}", ButtonStyle.Success, null, null, (page >= pages) ? true : false);

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
        await message.ModifyAsync(x => x.Components = c.Build());
        await message.ModifyAsync(x => x.Embed = b.Build());
        await component.DeferAsync();
    }
}

