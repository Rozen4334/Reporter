using Reporter.Models;

namespace Reporter;

public static class Extensions
{
    /// <summary>
    /// Pending reports that need to be confirmed
    /// </summary>
    public static List<Report> Pending = new();

    /// <summary>
    /// Checks if a user has a role or not
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public static bool HasRole(this SocketGuildUser user, string roleName)
    {
        var roles = from a in user.Roles
                    where a.Name == roleName
                    select a;
        return roles.Any();
    }

    /// <summary>
    /// Constructs an embed based on user or client provided
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="client"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static EmbedBuilder Construct(this EmbedBuilder builder, DiscordSocketClient client, IUser? user = null)
    {
        builder.WithColor(Color.Blue);
        builder.WithAuthor(user ?? client.CurrentUser);
        builder.WithFooter($"Reporter | {DateTime.UtcNow}", client.CurrentUser.GetAvatarUrl());
        return builder;
    }

    /// <summary>
    /// Rounds up a number in modulo approach
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int RoundUp(this int input)
    {
        int i = input % 10;
        if (i != 0)
            input += (10 - i);
        return input;
    }
}
