using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Data;
using Discord.WebSocket;

namespace Reporter
{
    public static class Extensions
    {
        public static List<Report> PendingEntries = new();

        public static bool HasRole(this SocketGuildUser user, string roleName)
        {
            var roles = from a in user.Roles
                        where a.Name == roleName
                        select a;
            return roles.Any();
        }
        public static EmbedBuilder Construct(this EmbedBuilder builder, IUser user = null)
        {
            builder.WithColor(Color.Blue);
            builder.WithAuthor((user != null) ? user : Program.Client.CurrentUser);
            builder.WithFooter($"Reporter | {DateTime.UtcNow}", Program.Client.CurrentUser.GetAvatarUrl());
            return builder;
        }

        public static int RoundUp(this int input)
        {
            int i = input % 10;
            if (i != 0)
                input += (10 - i);
            return input;
        }

        public readonly static SlashCommandBuilder[] Builders =
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

        public static async Task WriteAppCommands(IGuild guild)
        {
            List<ApplicationCommandProperties> commands = new();
            foreach (var builder in Builders)
                commands.Add(builder.Build());
            await Program.Client.Rest.BulkOverwriteGuildCommands(commands.ToArray(), guild.Id);
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
