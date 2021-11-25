using Reporter.Models;

namespace Reporter.Data
{

    public class ReportManager
    {
        private readonly List<Report> _reports = new();

        private readonly string _path;

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

        public void SaveReports()
            => Writer.SaveReports(_reports, _path);

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

        public IEnumerable<Report> GetReports(string username)
        {
            var result = from x in _reports
                         where x.Username.ToLower() == username.ToLower()
                         select x;
            return result.Any()
                ? result.ToList()
                : Enumerable.Empty<Report>();
        }

        public IEnumerable<Report> GetReportByAgent(ulong id)
        {
            var result = from x in _reports
                         where x.Moderator == id
                         select x;
            return result.Any() 
                ? result.ToList() 
                : Enumerable.Empty<Report>();
        }

        public bool TryGetReport(int id, out Report report)
        {
            var result = from x in _reports
                         where x.ID == id
                         select x;
            report = result.Any()
                ? result.FirstOrDefault()
                : new();
            return report.ID != -1;
        }

        public IEnumerable<Report> GetAllReports()
            => _reports.Any()
                ? _reports
                : Enumerable.Empty<Report>();
    }
}
