using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Data;

namespace Reporter
{
    public static class Extensions
    {
        /// <summary>
        /// string.Split('|')
        /// i[0] = IUser id;
        /// i[1] = TSPlr name;
        /// i[2] = Type;
        /// i[3] = Time;
        /// i[4] = Blocksbroken;
        /// i[5] = Punishment;
        /// i[6] = Note;
        /// </summary>
        public static List<string> PendingDBEntries = new();

        public static EmbedBuilder Construct(this EmbedBuilder builder, IUser user = null)
        {
            builder.WithColor(Color.Blue);
            builder.WithAuthor((user != null) ? user : Program.Client.CurrentUser);
            builder.WithFooter($"Reporter | {DateTime.UtcNow}", Program.Client.CurrentUser.GetAvatarUrl());
            return builder;
        }

        public static TimeSpan? SetReportTime(string input)
        {
            if (TimeSpan.TryParse(input, out TimeSpan result))
                return result;
            else return null;
        }

        public static int RoundUp(this int input)
        {
            int i = input % 10;
            if (i != 0)
                input += (10 - i);
            return input;
        }

        public static async Task WriteInteractionsAsync(IGuild guild)
        {
            await Program.Client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
            {
                Name = "report",
                Description = "Reports a user for harming the world/map (Griefing, tunneling & relevant).",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "player",
                        Required = true,
                        Description = "The username of a player.",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "type",
                        Required = true,
                        Description = "The type of offense (Format: grief, tunnel, hack, chat, other).",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "time",
                        Required = true,
                        Description = "History timespan since offense (Format: dd.hh:mm:ss)",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "blocksbroken",
                        Required = true,
                        Description = "Blocks broken if applicable. (Set '0' if none.)",
                        Type = ApplicationCommandOptionType.Integer,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "punishment",
                        Required = true,
                        Description = "The punishment given to a player for their offense",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "note",
                        Required = false,
                        Description = "Additional notes/summary of report. ignore if there is nothing to add.",
                        Type = ApplicationCommandOptionType.String,
                    },
                },
            }, guild.Id) ;
            await Program.Client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
            {
                Name = "playerinfo",
                Description = "Gets all reports, filtered by a specific player.",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "player",
                        Required = true,
                        Description = "The username of a player.",
                        Type = ApplicationCommandOptionType.String,
                    }
                },
            }, guild.Id);
            await Program.Client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
            {
                Name = "reportinfo",
                Description = "Views a report for the specified ID.",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "id",
                        Required = true,
                        Description = "The ID of a report.",
                        Type = ApplicationCommandOptionType.Integer,
                    },
                },
            }, guild.Id);
            await Program.Client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
            {
                Name = "editreport",
                Description = "Edit a report for the specified report id",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "id",
                        Required = true,
                        Description = "The id of the report.",
                        Type = ApplicationCommandOptionType.Integer,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "type",
                        Required = false,
                        Description = "Edit the type of a report.",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "time",
                        Required = false,
                        Description = "Edit the time of a report.",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "punishment",
                        Required = false,
                        Description = "Edit the punishment of a report",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "note",
                        Required = false,
                        Description = "Edit the note of a report",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "username",
                        Required = false,
                        Description = "Edit the username of a report",
                        Type = ApplicationCommandOptionType.String,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "blocksbroken",
                        Required = false,
                        Description = "Edit the total blocks broken of a report",
                        Type = ApplicationCommandOptionType.Integer,
                    }
                },
            }, guild.Id);
            await Program.Client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
            {
                Name = "reports",
                Description = "Views all reports and their ID",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "page",
                        Required = false,
                        Description = "The page of the report list",
                        Type = ApplicationCommandOptionType.Integer,
                    },
                },
            }, guild.Id);
            await Program.Client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
            {
                Name = "reporterinfo",
                Description = "Gets all data on a reporter, specified by their Discord username.",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "user",
                        Required = false,
                        Description = "The page of the report list",
                        Type = ApplicationCommandOptionType.User,
                    },
                },
            }, guild.Id);
        }

        public static async Task RemoveAppCommands(IGuild guild)
        {
            var guildcommands = await Program.Client.Rest.GetGuildApplicationCommands(guild.Id);
            foreach (var x in guildcommands)
            {
                await x.DeleteAsync();
            }
        }
    }
}
