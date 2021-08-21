using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Data
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public string Punishment { get; set; }
        public int BlocksBroken { get; set; }
        public string Note { get; set; }
        public List<string> ProofURLs { get; set; }
    }
    public class Reports : User
    {
        private static List<User> Users;

        private static string FilePath = $"{Path.Combine(Program.Settings.SavePath, "Users.json")}";

        static Reports()
        {
            if (Writer.SaveExists(FilePath))
            {
                var accs = Writer.LoadUsers(FilePath);
                Users = accs.ToList();
            }
            else
            {
                Users = new List<User>();
                SaveUsers();
            }
        }

        public static void SaveUsers() => Writer.SaveUsers(Users, FilePath);

        public static User AddUser(string username, string type, DateTime time, string punishment, int blocksbroken, List<string> urls = null, string note = "")
        {
            var id = 1 + ((Users.Count != 0) ? Users.Count : 0);
            if (Users.Any(x => x.ID == id))
            {
                Console.WriteLine("false ID, code change required");
                return null;
            }
            if (urls == null)
                urls = new List<string>();
            User user = new()
            {
                ID = id,
                Username = username,
                Type = type.ToLower(),
                BlocksBroken = blocksbroken,
                ProofURLs = urls,
                Note = note,
                Punishment = punishment,
                Time = time
            };
            Users.Add(user);
            SaveUsers();
            return user;
        }

        public static List<User> GetReports(string username)
        {
            var result = from x in Users
                       where x.Username.ToLower() == username.ToLower()
                       select x;
            return result.ToList();
        }

        public static List<User> GetReports(string username, string type)
        {
            var result = from x in Users
                         where x.Username.ToLower() == username.ToLower() && x.Type == type.ToLower()
                         select x;
            return result.ToList();
        }

        public static User GetReportByID(int id)
        {
            var result = from x in Users
                         where x.ID == id
                         select x;
            if (result == null)
                return null;
            return result.FirstOrDefault();
        }

        public static List<User> GetAllReports()
        {
            return Users;
        }
    }
}
