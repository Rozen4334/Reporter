using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Data
{
    class Writer
    {
        public static void SaveUsers(IEnumerable<Report> accounts, string filePath)
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public static IEnumerable<Report> LoadUsers(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Report>>(json);
        }
        public static bool SaveExists(string filePath) => File.Exists(filePath);
    }
}
