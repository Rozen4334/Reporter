using Reporter.Models;
using System.Text.Json;

namespace Reporter.Data;

internal class Writer
{
    public static void SaveReports(IEnumerable<Report> accounts, string filePath)
        => File.WriteAllText(filePath, JsonSerializer.Serialize(accounts, 
            new JsonSerializerOptions() { WriteIndented = true }));

    public static IEnumerable<Report> LoadUsers(string filePath)
        => JsonSerializer.Deserialize<List<Report>>(File.ReadAllText(filePath)) ?? new();

    public static bool SaveExists(string filePath) 
        => File.Exists(filePath);
}

