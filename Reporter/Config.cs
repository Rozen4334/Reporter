using System.Text.Json;

namespace Reporter
{
    public static class Config
    {
        public static Settings Settings { get; } = Settings.Read();

        public static void Save()
            => Settings.Save();
    }
    public class Settings
    {
        public string BotToken { get; set; } = "new";

        public string SavePath { get; set; } = "files";

        public bool WriteCommands { get; set; } = true;

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

        internal void Save()
            => File.WriteAllText("Config.json", JsonSerializer.Serialize(this,
                    new JsonSerializerOptions() { WriteIndented = true }));
    }
}
