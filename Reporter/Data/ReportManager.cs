using Reporter.Models;

namespace Reporter.Data;

public class ReportManager
{
    // List of reports
    private readonly List<Report> _reports = new();

    // File path
    private readonly string _path;

    /// <summary>
    /// Constructs a manager based on the guild it needs to target
    /// </summary>
    /// <param name="guildId"></param>
    public ReportManager(ulong guildId)
    {
        _path = $"{Path.Combine(Config.Settings.SavePath, $"{guildId}.json")}";

        if (Writer.SaveExists(_path))
            _reports = Writer.LoadUsers(_path).ToList();
        else
        {
            _reports = new List<Report>();
            SaveReports();
        }
    }

    /// <summary>
    /// Save the current report list
    /// </summary>
    public void SaveReports()
        => Writer.SaveReports(_reports, _path);

    /// <summary>
    /// Add a report from its struct
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public Report AddReport(Report report)
    {
        var id = 1 + (_reports.Any() ? _reports.Count : 0);
        if (_reports.Any(x => x.ID == id))
            Console.WriteLine("false ID, code change required");
        else
        {
            report.ID = id;
            _reports.Add(report);
            SaveReports();
        }
        return report;
    }

    /// <summary>
    /// Get a set of reports by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public IEnumerable<Report> GetReports(string username)
    {
        var result = from x in _reports
                        where x.Username.ToLower() == username.ToLower()
                        select x;
        return result.Any()
            ? result.ToList()
            : Enumerable.Empty<Report>();
    }

    /// <summary>
    /// Get a set of reports by moderator
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerable<Report> GetReportByModerator(ulong id)
    {
        var result = from x in _reports
                        where x.Agent == id
                        select x;
        return result.Any() 
            ? result.ToList() 
            : Enumerable.Empty<Report>();
    }

    /// <summary>
    /// Try to get reports by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    public bool TryGetReport(long id, out Report report)
    {
        var result = from x in _reports
                        where x.ID == id
                        select x;
        report = result.Any()
            ? result.FirstOrDefault()
            : new();
        return report.ID != -1;
    }

    /// <summary>
    /// Gets all reports
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Report> GetAllReports()
        => _reports.Any()
            ? _reports
            : Enumerable.Empty<Report>();
}