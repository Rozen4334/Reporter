using Reporter.Data;
using Reporter.Models;

namespace Reporter
{
    public static class Extensions
    {
        public static List<Report> Pending = new();

        public static bool HasRole(this SocketGuildUser user, string roleName)
        {
            var roles = from a in user.Roles
                        where a.Name == roleName
                        select a;
            return roles.Any();
        }
        public static EmbedBuilder Construct(this EmbedBuilder builder, DiscordSocketClient client, IUser? user = null)
        {
            builder.WithColor(Color.Blue);
            builder.WithAuthor(user ?? client.CurrentUser);
            builder.WithFooter($"Reporter | {DateTime.UtcNow}", client.CurrentUser.GetAvatarUrl());
            return builder;
        }

        public static int RoundUp(this int input)
        {
            int i = input % 10;
            if (i != 0)
                input += (10 - i);
            return input;
        }
    }
}
