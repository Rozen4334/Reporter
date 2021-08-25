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
    class Interactions
    {
        public static async Task InteractionHandler(SocketInteraction args)
        {
            var component = args as SocketMessageComponent;

            string[] id = component.Data.CustomId.Split('|');

            if (ulong.TryParse(id[1], out ulong userid))
                if (component.User.Id != userid)
                    return;

            switch(id[0])
            {
                case "id_exit":
                    await Exit(component);
                    break;
                case "id_confirm":
                    await Confirm(component);
                    break;
                case "id_img":
                    await Images(component, id[2]);
                    break;
                case "id_view":
                    await ViewReport(component, id[2]);
                    break;
                case "id_page":
                    await ReportPages(component, id[2]);
                    break;
            }
        }
        
        private static async Task Exit(SocketMessageComponent args)
        {
            var builder = new EmbedBuilder().Construct(args.User).WithColor(Color.Red);
            builder.WithTitle("Canceled report creation");

            Extensions.PendingDBEntries.RemoveAll(x => x.Split('|').First() == args.User.Id.ToString());

            var message = args.Message;
            await message.ModifyAsync(x => x.Components = null);
            await message.ModifyAsync(x => x.Embed = builder.Build());
            await args.AcknowledgeAsync();
        }

        private static async Task Confirm(SocketMessageComponent args)
        {
            var builder = new EmbedBuilder().Construct(args.User).WithColor(Color.Green);
            builder.WithTitle("Succesfully wrote report");

            string entry = Extensions.PendingDBEntries.Find(x => x.Split('|').First() == args.User.Id.ToString());
            string[] i = entry.Split('|');
            var report = Reports.AddUser(i[0], i[1], i[2], DateTime.Parse(i[3]), i[5], int.Parse(i[4]), null, (i.Length > 6) ? i[6] : "");
            Extensions.PendingDBEntries.Remove(entry);

            builder.WithDescription($"Created report for: ` {report.Username} ` with ID: ` {report.ID} `");

            var component = new ComponentBuilder().WithButton("View report", $"id_view|{args.User.Id}|{report.ID}");

            var message = args.Message;
            await message.ModifyAsync(x => x.Components = component.Build());
            await message.ModifyAsync(x => x.Embed = builder.Build());
            await args.AcknowledgeAsync();
        }

        private static async Task Images(SocketMessageComponent args, string iid)
        {
            int id = int.Parse(iid);
            var builder = new EmbedBuilder().Construct(args.User);
            builder.WithTitle($"Displaying all images for report: ` {id} `");

            var report = Reports.GetReportByID(id);

            if (report.ProofURLs.Count == 0)
                builder.WithDescription("No images to display. Add images with ` {mention} addimage reportID image (or attach an image in the message youre sending");
            Embed[] embeds = { builder.Build() };
            await args.RespondAsync(embeds);
            if (report.ProofURLs.Count != 0)
            {
                foreach (var x in report.ProofURLs)
                {
                    await args.Channel.SendMessageAsync(x);
                }
            }
        }

        private static async Task ViewReport(SocketMessageComponent args, string iid)
        {
            int id = int.Parse(iid);
            var message = args.Message;
            var builder = new EmbedBuilder().Construct(args.User);

            var reportbyid = Reports.GetReportByID(id);

            var component = new ComponentBuilder().WithButton("View images", $"id_img|{args.User.Id}|{reportbyid.ID}");

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
            await args.AcknowledgeAsync();
        }

        private static async Task ReportPages(SocketMessageComponent args, string ppage)
        {
            int page = int.Parse(ppage);
            var message = args.Message;
            var builder = new EmbedBuilder().Construct(args.User);

            builder.WithTitle("Report list");
            builder.WithDescription($"Currently viewing page: ` {page} `");

            var reports = Reports.GetAllReports();

            int max = 10 + reports.Count - (page * 10);

            var pages = reports.Count.RoundUp() / 10;

            builder.WithFooter($"Reporter | Page {page} of {pages} | {DateTime.UtcNow}", Program.Client.CurrentUser.GetAvatarUrl());

            var component = new ComponentBuilder()
                .WithButton("Previous page", $"id_page|{args.User.Id}|{page - 1}", ButtonStyle.Danger, null, null, (page - 1 == 0) ? true : false)
                .WithButton("Next page", $"id_page|{args.User.Id}|{page + 1}", ButtonStyle.Success, null, null, (page >= pages) ? true : false);

            var users = new List<User>();
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
            await args.AcknowledgeAsync();
        }
    }
}
