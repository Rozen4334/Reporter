using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Data
{
    public class Report
    {
        public int ID { get; set; }

        public ulong Agent { get; set; }

        public string Username { get; set; }

        public string Type { get; set; }

        public DateTime Time { get; set; }

        public string Punishment { get; set; }

        public int BlocksBroken { get; set; }

        public string Note { get; set; }

        public List<string> ProofURLs { get; set; } = new();

        public Report(int id, ulong agent, string username, string type, DateTime time, string punishment, int blocksbroken = 0, string note = null)
        {
            ID = id;
            Agent = agent;
            Username = username;
            Type = type;
            Time = time;
            Punishment = punishment;
            Note = note 
                ?? "";
            BlocksBroken = (blocksbroken != 0) 
                ? blocksbroken 
                : 0;
        }

        public Report AddImages(List<Attachment> args)
        {
            args.ForEach(x => ProofURLs.Add(x.Url));
            return this;
        }
    }

    public class ReportManager
    {
        private static List<Report> Entries;

        private readonly string FilePath;

        public ReportManager(ulong guildId)
        {
            FilePath = $"{Path.Combine(Program.Settings.SavePath, $"{guildId}.json")}";

            if (Writer.SaveExists(FilePath))
            {
                var accs = Writer.LoadUsers(FilePath);
                Entries = accs.ToList();
            }
            else
            {
                Entries = new List<Report>();
                SaveUsers();
            }
        }

        public void SaveUsers() 
            => Writer.SaveUsers(Entries, FilePath);

        public Report AddUser(Report report)
        {
            var id = 1 + ((Entries.Count != 0) ? Entries.Count : 0);
            if (Entries.Any(x => x.ID == id))
            {
                Console.WriteLine("false ID, code change required");
                return null;
            }
            else
            {
                report.ID = id;
                Entries.Add(report);
                SaveUsers();
                return report;
            }
        }

        public List<Report> GetReports(string username)
        {
            var result = from x in Entries
                       where x.Username.ToLower() == username.ToLower()
                       select x;
            return result.ToList();
        }

        public bool GetReportByID(int id, out Report report)
        {
            var result = from x in Entries
                         where x.ID == id
                         select x;
            report = result.Any() ? result.FirstOrDefault() : null;
            return report != null;
        }

        public List<Report> GetReportByAgent(ulong userid)
        {
            var result = from x in Entries
                         where x.Agent == userid
                         select x;
            if (result == null)
                return null;
            return result.ToList();
        }

        public List<Report> GetAllReports()
            => Entries;
    }
}
