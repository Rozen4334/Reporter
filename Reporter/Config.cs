using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reporter
{
    public class Config
    {
        public string BotToken;

		public string SavePath;

		public static Config Read()
		{
			string configPath = "Config.json";
			if (!File.Exists(configPath))
			{
				File.WriteAllText(configPath, JsonConvert.SerializeObject(Default(), Formatting.Indented));
				return Default();
			}
			try
			{
				return JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
			}
			catch
			{
				Console.WriteLine("The config directory or file is invalid, please reflect & retry");
				return Default();
			}
		}

		public static Config Default()
        {
			return new Config()
			{
				BotToken = "",
				SavePath = ""
			};
        }
	}
}
