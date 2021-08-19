using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using Reporter.Data;

namespace Reporter
{
    public class Commands
    {
        private static string[] RTypes = { "Grief", "Tunnel", "Hack", "Chat",  "Other" };

        public static async Task CommandHandler(SocketInteraction args)
        {
            var cmd = args as SocketSlashCommand;
            switch(cmd.Data.Name)
            {
                case "viewreport":
                    await ViewReport(cmd);
                    break;
                case "report":
                    await Report(cmd);
                    break;
                case "viewuser":
                    await ViewUser(cmd);
                    break;
                case "editreport":
                    await EditReport(cmd);
                    break;

            }    
        }

        private static async Task Report(SocketSlashCommand args)
        {
            var data = args.Data.Options.ToArray();

            // checking valid command input

            var builder = new EmbedBuilder().Construct();

            var type = data[1].Value.ToString();
            if (!RTypes.Any(x => x.ToLower().Contains(type.ToLower())))
            {
                builder.WithTitle("Invalid syntax");
                builder.WithDescription($"Invalid type. Valid types are: ` {string.Join(", ", RTypes)} `");
                Embed[] embeds = { builder.Build() };
                await args.RespondAsync(embeds);
                return;
            }

            var time = Extensions.SetReportTime(data[2].Value.ToString());
            if (time == null)
            {
                builder.WithTitle("Invalid syntax");
                builder.WithDescription("Invalid time string. The format is: ` dd.hh:mm:ss `");
                Embed[] embeds = { builder.Build() };
                await args.RespondAsync(embeds);
                return;
            }
            DateTime reporttime = DateTime.UtcNow.Subtract((TimeSpan)time);

            // actual command execution
            var component = new ComponentBuilder()
                .WithButton("Exit report", $"id_exit|{args.User.Id}", ButtonStyle.Danger)
                .WithButton("Confirm report", $"id_confirm|{args.User.Id}", ButtonStyle.Success);

            builder.WithTitle("Reporting user");
            builder.WithDescription($"Starting on report for player: ` {data[0].Value} `");

            builder.AddField("Type:", type);
            builder.AddField("Time:", reporttime);
            if (!data[3].Value.ToString().Equals("0"))
                builder.AddField("Blocks broken:", data[3].Value);
            builder.AddField("Punishment:", data[4].Value);
            if (data.Length > 5)
                builder.AddField("Note:", data[5].Value);

            //add a pending item in the pendingdblist
            Extensions.PendingDBEntries.Add($"{args.User.Id}|{data[0].Value}|{type}|{reporttime}|{data[3].Value}|{data[4].Value}" + ((data.Length > 5) ? $"|{data[5].Value}" : ""));

            Embed[] em = { builder.Build() };
            await args.RespondAsync(em, null, false, InteractionResponseType.ChannelMessageWithSource, false, null, null, component.Build());
        }

        private static async Task ViewReport(SocketSlashCommand args)
        { 
            var data = args.Data.Options.ToArray();
            int id = int.Parse(data.First().Value.ToString());
            var reportbyid = Reports.GetReportByID(id);

            var builder = new EmbedBuilder().Construct(args.User);

            if (reportbyid == null)
            {
                builder.WithTitle("Invalid syntax");
                builder.WithDescription("This report ID is invalid, please try again by specifying a valid ID.");
                Embed[] embeds = { builder.Build() };
                await args.RespondAsync(embeds);
                return;
            }

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

            Embed[] em = { builder.Build() };
            await args.RespondAsync(em, null, false, InteractionResponseType.ChannelMessageWithSource, false, null, null, component.Build());
        }

        private static async Task ViewUser(SocketSlashCommand args)
        {
            var data = args.Data.Options.ToArray();
            string plr = data.First().Value.ToString();
            var reports = Reports.GetReports(plr);

            var builder = new EmbedBuilder().Construct(args.User);

            if (reports.Count == 0)
            {
                builder.WithTitle("Invalid syntax");
                builder.WithDescription($"I found no matches for player: ` {plr} `. Are you sure the name correctly spelled?");
                Embed[] embeds = { builder.Build() };
                await args.RespondAsync(embeds);
                return;
            }

            builder.WithTitle($"User information: ` {plr} `");
            builder.WithDescription($"I have found ` {reports.Count} ` report(s) for specified user.");
            int blocks = 0;
            List<string> ids = new();
            foreach (var x in reports)
            {
                blocks += x.BlocksBroken;
                ids.Add($"**🢒 {x.ID}**");
            }
            builder.AddField("Total blocks broken:", (blocks != 0) ? blocks : "None");
            builder.AddField("Last punishment given:", reports.Last().Punishment);
            builder.AddField($"Reports [{ids.Count}]:", string.Join("\n", ids));
            Embed[] em = { builder.Build() };
            await args.RespondAsync(em);
        }

        private static async Task EditReport(SocketSlashCommand args)
        {
            var data = args.Data.Options.ToArray();
            int id = int.Parse(data.First().Value.ToString());
            var reportbyid = Reports.GetReportByID(id);

            var builder = new EmbedBuilder().Construct(args.User);

            if (reportbyid == null)
            {
                builder.WithTitle("Invalid syntax");
                builder.WithDescription("This report ID is invalid, please try again by specifying a valid ID.");
                Embed[] embeds = { builder.Build() };
                await args.RespondAsync(embeds);
                return;
            }

            builder.WithTitle($"Editing report: ` {id} `");

            for (int i = 0; i < data.Length; i++)
            {
                switch (data[i].Name)
                {
                    case "type":
                        {
                            if (RTypes.Any(x => x.ToLower().Equals(data[i].Value.ToString().ToLower())))
                            {
                                builder.AddField("Edited type:", $"**🢒 Old:** ` {reportbyid.Type} `\n**🢒 New:** ` {data[i].Value} `");
                                reportbyid.Type = data[i].Value.ToString();
                            }
                        }
                        break;
                    case "note":
                        {
                            builder.AddField("Edited note:", $"**🢒 Old:** ` {reportbyid.Note} `\n**🢒 New:** ` {data[i].Value} `");
                            reportbyid.Note = data[i].Value.ToString();
                        }
                        break;
                    case "punishment":
                        {
                            builder.AddField("Edited punishment:", $"**🢒 Old:** ` {reportbyid.Punishment} `\n**🢒 New:** ` {data[i].Value} `");
                            reportbyid.Punishment = data[i].Value.ToString();
                        }
                        break;
                    case "time":
                        {
                            if (TimeSpan.TryParse(data[i].Value.ToString(), out TimeSpan span))
                            {
                                builder.AddField("Edited time:", $"**🢒 Old:** ` {reportbyid.Time} `\n**🢒 New:** ` {data[i].Value} `");
                                reportbyid.Time = DateTime.Now.Subtract(span);
                            }
                        }
                        break;
                    case "username":
                        {
                            builder.AddField("Edited username:", $"**🢒 Old:** ` {reportbyid.Username} `\n**🢒 New:** ` {data[i].Value} `");
                            reportbyid.Username = data[i].Value.ToString();
                        }
                        break;
                    case "blocksbroken":
                        {
                            builder.AddField("Edited total blocks broken:", $"**🢒 Old:** ` {reportbyid.Username} `\n**🢒 New:** ` {data[i].Value} `");
                            reportbyid.BlocksBroken = int.Parse(data[i].Value.ToString());
                        }
                        break;
                }
            }
            Reports.SaveUsers();
            Embed[] em = { builder.Build() };
            await args.RespondAsync(em);
        }
    }
}
