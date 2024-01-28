using System.Collections.Generic;
using SCPDiscordBot.Properties;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SCPDiscord
{
	public class Config
	{
		public class Bot
		{
			public string token = "";
			public ulong serverId = 0;
			public string logLevel = "Information";
			public string presenceType = "Watching";
			public string presenceText = "for server startup...";
			public bool disableCommands = false;
		}
		public Bot bot;

		public Dictionary<ulong, string[]> permissions = new Dictionary<ulong, string[]>();

		public class Plugin
		{
			public string address = "127.0.0.1";
			public int port = 8888;
		}
		public Plugin plugin;
	}

	public static class ConfigParser
	{
		public static bool loaded = false;

		public static Config config = null;

		public static string configPath = "config.yml";

		public static void LoadConfig()
		{
			if (SCPDiscordBot.commandlineArguments.Length > 0)
			{
				configPath = SCPDiscordBot.commandlineArguments[0];
			}

			Logger.Log("Loading config \"" + Path.GetFullPath(configPath) + "\"", LogID.CONFIG);

			// Writes default config to file if it does not already exist
			if (!File.Exists(configPath))
			{
				File.WriteAllText(configPath, Encoding.UTF8.GetString(Resources.default_config));
			}

			// Reads config contents into FileStream
			FileStream stream = File.OpenRead(configPath);

			// Converts the FileStream into a YAML object
			IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();
			config = deserializer.Deserialize<Config>(new StreamReader(stream));

			loaded = true;
		}

		public static void PrintConfig()
		{
			Logger.Debug("#### Config settings ####", LogID.CONFIG);
			// Token skipped
			Logger.Debug("bot.server-id: " + config.bot.serverId, LogID.CONFIG);
			Logger.Debug("bot.log-level: " + config.bot.logLevel, LogID.CONFIG);
			Logger.Debug("bot.presence-type: " + config.bot.presenceType, LogID.CONFIG);
			Logger.Debug("bot.presence-text: " + config.bot.presenceText, LogID.CONFIG);
			Logger.Debug("bot.disable-commands: " + config.bot.disableCommands, LogID.CONFIG);
			// TODO: Print permissions
			Logger.Debug("plugin.address: " + config.plugin.address, LogID.CONFIG);
			Logger.Debug("plugin.port: " + config.plugin.port, LogID.CONFIG);
		}

		public static bool HasPermission(DiscordMember member, string command)
		{
			foreach (DiscordRole role in member.Roles)
			{
				if (config.permissions.TryGetValue(role.Id, out string[] permissions))
				{
					if (permissions.Any(s => Regex.IsMatch(command, "^" + s)))
					{
						return true;
					}
				}
			}

			if (config.permissions.TryGetValue(0, out string[] everyonePermissions))
			{
				if (everyonePermissions.Any(s => Regex.IsMatch(command, "^" + s)))
				{
					return true;
				}
			}

			return false;
		}
	}
}
