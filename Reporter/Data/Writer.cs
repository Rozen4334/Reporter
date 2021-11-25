using Reporter.Models;
using System.Text.Json;

namespace Reporter.Data;

internal class Writer
{
    /// <summary>
    /// Save reports into file
    /// </summary>
    /// <param name="accounts"></param>
    /// <param name="filePath"></param>
    public static void SaveReports(IEnumerable<Report> accounts, string filePath)
        => File.WriteAllText(filePath, JsonSerializer.Serialize(accounts, 
            new JsonSerializerOptions() { WriteIndented = true }));

    /// <summary>
    /// Load reports from file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static IEnumerable<Report> LoadUsers(string filePath)
        => JsonSerializer.Deserialize<List<Report>>(File.ReadAllText(filePath)) ?? new();

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool SaveExists(string filePath) 
        => File.Exists(filePath);
}

