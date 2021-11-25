using System.Text.Json;

namespace Reporter;

public static class Config
{
    /// <summary>
    /// Accesses all configuration fields
    /// </summary>
    public static Settings Settings { get; } = Settings.Read();

    /// <summary>
    /// Saves configuration if overriden inside the client
    /// </summary>
    public static void Save()
        => Settings.Save();
}

public class Settings
{
    /// <summary>
    /// Token of the Discord bot
    /// </summary>
    public string BotToken { get; set; } = "new";

    /// <summary>
    /// Save path
    /// </summary>
    public string SavePath { get; set; } = "files";

    /// <summary>
    /// To write commands over REST or not
    /// </summary>
    public bool WriteCommands { get; set; } = true;

    /// <summary>
    /// All whitelisted user ID's, that get access to command execution.
    /// </summary>
    public ulong[] WhitelistedUsers { get; set; } = new ulong[] { 539535197935239179 };

    // reads the settings, internal for safety 
    internal static Settings Read()
    {
        string path = "Config.json";
        if (!File.Exists(path))
        {
            try
            {
                var str = JsonSerializer.Serialize(new Settings(),
                    new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(path, str);
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path)) ?? new();
    }

    // saves settings, internal for safety. Public available at config class
    internal void Save()
        => File.WriteAllText("Config.json", JsonSerializer.Serialize(this,
                new JsonSerializerOptions() { WriteIndented = true }));
}