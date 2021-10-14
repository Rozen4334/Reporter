using Discord;
using Discord.WebSocket;
using Reporter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    public class Components
    {
        private readonly Dictionary<string, Func<SocketMessageComponent, string, Task>> CallbackHandler = new();

        public Components()
        {
            CallbackHandler["id_exit"] = Exit;
            CallbackHandler["id_confirm"] = Confirm;
            CallbackHandler["id_img"] = Images;
            CallbackHandler["id_view"] = ViewReport;
            CallbackHandler["id_page"] = ReportPages;
            CallbackHandler["id_viewall"] = ViewAll;
        }
        public async Task InteractionHandler(SocketMessageComponent component)
        {
            string[] id = component.Data.CustomId.Split('|');

            if (ulong.TryParse(id[1], out ulong userid))
            {
                if (component.User.Id != userid)
                    return;
                else if (CallbackHandler.TryGetValue(id[0], out Func<SocketMessageComponent, string, Task> value))
                    await value(component, (id.Length < 3) ? "" : id[2]);
                else await component.RespondAsync(":warning: Unknown component handler, please resolve this!");
            }
            else await component.DeferAsync();
        }
        
        private async Task Exit(SocketMessageComponent args, string iid)
        {
            var builder = new EmbedBuilder().Construct(args.User).WithColor(Color.Red);
            builder.WithTitle("Canceled report creation");

            Extensions.PendingEntries.RemoveAll(x => x.Agent == args.User.Id);

            var message = args.Message;
            await message.ModifyAsync(x => x.Components = null);
            await message.ModifyAsync(x => x.Embed = builder.Build());
            await args.DeferAsync();
        }

        private async Task Confirm(SocketMessageComponent args, string iid)
        {
            var manager = new ReportManager((args.User as IGuildUser).GuildId);

            var builder = new EmbedBuilder().Construct(args.User).WithColor(Color.Green);
            builder.WithTitle("Succesfully wrote report");

            Report entry = Extensions.PendingEntries.Find(x => x.Agent == args.User.Id);
            if (entry == null)
            {
                await args.RespondAsync("Report not found. This could be caused by an error on the bot, or because it was restarted with this pending report open.");
                return;
            }

            var report = manager.AddUser(entry);
            Extensions.PendingEntries.Remove(entry);

            builder.WithDescription($"Created report for: ` {report.Username} ` with ID: ` {report.ID} `");

            var component = new ComponentBuilder().WithButton("View report", $"id_view|{args.User.Id}|{report.ID}");

            var message = args.Message;
            await message.ModifyAsync(x => x.Components = component.Build());
            await message.ModifyAsync(x => x.Embed = builder.Build());
            await args.DeferAsync();
        }

        private async Task Images(SocketMessageComponent args, string iid)
        {
            var manager = new ReportManager((args.User as IGuildUser).GuildId);

            int id = int.Parse(iid);
            var builder = new EmbedBuilder().Construct(args.User);
            builder.WithTitle($"Displaying all images for report: ` {id} `");

            if (manager.GetReportByID(id, out Report report))
            {
                if (!report.ProofURLs.Any())
                    builder.WithDescription("No images to display. Add images with ` @reporter addimage <reportID> <image> ` (or attach an image in the message youre sending");
                Embed[] embeds = { builder.Build() };
                await args.RespondAsync("", embeds);
                if (report.ProofURLs.Any())
                    foreach (var x in report.ProofURLs)
                        await args.Channel.SendMessageAsync(x);
            }
        }

        private async Task ViewReport(SocketMessageComponent args, string iid)
        {
            var manager = new ReportManager((args.User as IGuildUser).GuildId);

            int id = int.Parse(iid);
            var message = args.Message;
            var builder = new EmbedBuilder().Construct(args.User);

            if (manager.GetReportByID(id, out Report reportbyid))
            {
                var component = new ComponentBuilder()
                    .WithButton("View images", $"id_img|{args.User.Id}|{reportbyid.ID}")
                    .WithButton("View all reports", $"id_img|{args.User.Id}|{reportbyid.Username}", ButtonStyle.Secondary, new Emoji("📃"));

                builder.WithTitle($"Report: ` {reportbyid.ID} `");
                builder.AddField("User:", reportbyid.Username, true);
                builder.AddField("Type:", reportbyid.Type, true);
                builder.AddField("Time:", reportbyid.Time);
                if (reportbyid.BlocksBroken != 0)
                    builder.AddField("Blocks broken:", reportbyid.BlocksBroken, true);
                builder.AddField("Punishment:", reportbyid.Punishment, true);
                if (reportbyid.Note != "")
                    builder.AddField("Note:", reportbyid.Note);
                if (reportbyid.ProofURLs.Count != 0)
                    builder.WithImageUrl(reportbyid.ProofURLs.First());

                await message.ModifyAsync(x => x.Components = component.Build());
                await message.ModifyAsync(x => x.Embed = builder.Build());
                await args.DeferAsync();
            }
        }

        private async Task ViewAll(SocketMessageComponent args, string iid)
        {
            var manager = new ReportManager((args.User as IGuildUser).GuildId);
            var builder = new EmbedBuilder().Construct(args.User);
            var reports = manager.GetReports(iid);
            var message = args.Message;

            if (reports.Any())
            {
                builder.WithTitle($"User information: ` {iid} `");
                builder.WithDescription($"I have found ` {reports.Count} ` report(s) for specified user.");
                int blocks = 0;
                List<string> ids = new();
                foreach (var x in reports)
                {
                    blocks += x.BlocksBroken;
                    ids.Add($"` {x.ID} ` - type: {x.Type}");
                }
                builder.AddField("Total blocks broken:", (blocks != 0) ? blocks : "None");
                builder.AddField("Last punishment given:", reports.Last().Punishment);
                builder.AddField($"Reports [{ids.Count}]:", string.Join("\n", ids));
            }
            else
            {
                builder.WithTitle($"No reports found!");
                builder.WithDescription("This user does not have any known reports.");
            }
            await message.ModifyAsync(x => x.Components = null);
            await message.ModifyAsync(x => x.Embed = builder.Build());
            await args.DeferAsync();
        }

        private async Task ReportPages(SocketMessageComponent args, string ppage)
        {
            var manager = new ReportManager((args.User as IGuildUser).GuildId);

            int page = int.Parse(ppage);
            var message = args.Message;
            var builder = new EmbedBuilder().Construct(args.User);

            builder.WithTitle("Report list");
            builder.WithDescription($"Currently viewing page: ` {page} `");

            var reports = manager.GetAllReports();

            int max = 10 + reports.Count - (page * 10);

            var pages = reports.Count.RoundUp() / 10;

            builder.WithFooter($"Reporter | Page {page} of {pages} | {DateTime.UtcNow}", Program.Client.CurrentUser.GetAvatarUrl());

            var component = new ComponentBuilder()
                .WithButton("Previous page", $"id_page|{args.User.Id}|{page - 1}", ButtonStyle.Danger, null, null, (page - 1 == 0) ? true : false)
                .WithButton("Next page", $"id_page|{args.User.Id}|{page + 1}", ButtonStyle.Success, null, null, (page >= pages) ? true : false);

            var users = new List<Report>();
            users.AddRange(reports.FindAll(x => x.ID >= max - 9 && x.ID <= max));
            users.Reverse();

            StringBuilder sb = new();
            foreach (var x in users)
            {
                if (x != null)
                {
                    sb.AppendLine($"` {x.ID} ` **{x.Username}** - Type: {x.Type}");
                    sb.AppendLine("⤷ Reported by: " + ((x.Agent != 0) ? $"**{Program.Client.GetUser(x.Agent).Username}**" : "Unavailable"));
                }
            }
            builder.AddField($"Reports:", sb.ToString());
            await message.ModifyAsync(x => x.Components = component.Build());
            await message.ModifyAsync(x => x.Embed = builder.Build());
            await args.DeferAsync();
        }
    }
}
