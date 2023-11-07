using Newtonsoft.Json;
using SCPDiscord.EventListeners;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Mirror.LiteNetLib4Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using YamlDotNet.Core;
using Log = PluginAPI.Core.Log;

namespace SCPDiscord
{
	public class SCPDiscord
	{
		public readonly Stopwatch serverStartTime = new Stopwatch();

		internal SynchronousExecutor sync;

		internal static SCPDiscord plugin;

		public bool roundStarted = false;

		public RoleSync roleSync;

		public bool shutdown;

		public const string VERSION = "3.1.4";

		[PluginEntryPoint("SCPDiscord", VERSION, "SCP:SL - Discord bridge.", "Karl Essinger")]
		public void Start()
		{
			plugin = this;

			serverStartTime.Start();

			LiteNetLib4MirrorNetworkManager.singleton.gameObject.AddComponent<SynchronousExecutor>();
			sync = LiteNetLib4MirrorNetworkManager.singleton.gameObject.GetComponent<SynchronousExecutor>();

			// Event handlers
            EventManager.RegisterEvents(this, sync);
			EventManager.RegisterEvents(this, new SyncPlayerRole());
			EventManager.RegisterEvents(this, new PlayerEventListener(this));
			EventManager.RegisterEvents(this, new ServerEventListener(this));
			EventManager.RegisterEvents(this, new EnvironmentEventListener(this));

			if (!LoadConfig())
				return;

			/*
			// Add the invisible SCPD marker at the end of the server name if the server has metrics on
			if (Config.GetBool("settings.metrics") && !string.IsNullOrWhiteSpace(ConfigFile.ServerConfig.GetString("server_name", "")))
			{
				ConfigFile.ServerConfig.SetString("server_name", ConfigFile.ServerConfig.GetString("server_name", "") + "<color=#ffffff00><size=1>SCPD:" + VERSION + "</size></color>");
				ServerConsole.ReloadServerName();
			}
			*/

			roleSync = new RoleSync(this);
			if (Server.Port == Config.GetInt("bot.port"))
			{
				Error("ERROR: Server is running on the same port as the plugin, aborting...");
				throw new Exception();
			}
			Language.Reload();

			new Thread(() => new StartNetworkSystem(plugin)).Start();

			Info("SCPDiscord " + VERSION + " enabled.");
		}

		private class SyncPlayerRole
		{
			[PluginEvent(ServerEventType.PlayerJoined)]
			public void OnPlayerJoin(Player player)
			{
				if (player == null) return;

				try
				{
					plugin.roleSync.SendRoleQuery(player);
				}
				catch (Exception e)
				{
					Log.Error("Error occured when checking player for rolesync!\n" + e);
				}
			}
		}

		public bool LoadConfig()
		{
			try
			{
				Config.Reload(plugin);
				Info("Successfully loaded config '" + Config.GetConfigPath() + "'.");
				return true;
			}
			catch (Exception e)
			{
				if (e is DirectoryNotFoundException)
				{
					Error("Config directory not found.");
				}
				else if (e is UnauthorizedAccessException)
				{
					Error("Primary language file access denied.");
				}
				else if (e is FileNotFoundException)
				{
					Error("'" + Config.GetConfigPath() + "' was not found.");
				}
				else if (e is JsonReaderException || e is YamlException)
				{
					Error("'" + Config.GetConfigPath() + "' formatting error.");
				}
				else if (e is Config.ConfigParseException)
				{
					Error("Formatting issue in config file '" + Config.GetConfigPath() + "'. Aborting startup.");
				}
				else
				{
					Error("Error reading config file '" + Config.GetConfigPath() + "'. Aborting startup.\n" + e);
				}
			}
			return false;
		}

		public void OnDisable()
		{
			shutdown = true;
			NetworkSystem.Disconnect();
			Log.Info("SCPDiscord disabled.");
		}

		public void Info(string message)
		{
			Log.Info(message);
		}

		public void Warn(string message)
		{
			Log.Warning(message);
		}

		public void Error(string message)
		{
			Log.Error(message);
		}

		public void Verbose(string message)
		{
			if (Config.GetBool("settings.verbose") || Config.GetBool("settings.debug"))
			{
				Log.Info(message);
			}
		}

		public void VerboseWarn(string message)
		{
			if (Config.GetBool("settings.verbose") || Config.GetBool("settings.debug"))
			{
				Log.Warning(message);
			}
		}

		public void VerboseError(string message)
		{
			if (Config.GetBool("settings.verbose") || Config.GetBool("settings.debug"))
			{
				Log.Error(message);
			}
		}

		public void Debug(string message)
		{
			if (Config.GetBool("settings.debug"))
			{
				Log.Info(message);
			}
		}

		public void DebugWarn(string message)
		{
			if (Config.GetBool("settings.debug"))
			{
				Log.Warning(message);
			}
		}

		public void DebugError(string message)
		{
			if (Config.GetBool("settings.debug"))
			{
				Log.Error(message);
			}
		}

		public void SendStringByID(ulong channelID, string message)
		{
			MessageWrapper wrapper = new MessageWrapper
			{
				ChatMessage = new ChatMessage
				{
					ChannelID = channelID,
					Content = message
				}
			};
			NetworkSystem.QueueMessage(wrapper);
		}

		public void SendEmbedByID(EmbedMessage message)
		{
			NetworkSystem.QueueMessage(new MessageWrapper { EmbedMessage = message });
		}

		public void SendMessage(string messagePath, Dictionary<string, string> variables = null)
		{
			Thread messageThread = new Thread(() => new ProcessMessageAsync(messagePath, variables));
			messageThread.Start();
		}

		public void SendEmbedWithMessage(string messagePath, EmbedMessage embed, Dictionary<string, string> variables = null)
		{
			Thread messageThread = new Thread(() => new ProcessEmbedMessageAsync(embed, messagePath, variables));
			messageThread.Start();
		}

		public void SendMessageByID(ulong channelID, string messagePath, Dictionary<string, string> variables = null)
		{
			new Thread(() => new ProcessMessageByIDAsync(channelID, messagePath, variables)).Start();
		}

		public void SendEmbedWithMessageByID(EmbedMessage embed, string messagePath, Dictionary<string, string> variables = null)
		{
			new Thread(() => new ProcessEmbedMessageByIDAsync(embed, messagePath, variables)).Start();
		}

		public bool KickPlayer(string steamID, string message = "Kicked from server")
		{
			foreach (Player player in Player.GetPlayers<Player>())
			{
				if (player.GetParsedUserID() == steamID)
				{
					player.Ban(message, 0);
					return true;
				}
			}
			return false;
		}

		public bool GetPlayerName(string steamID, ref string name)
		{
			foreach (Player player in Player.GetPlayers<Player>())
			{
				if (player.GetParsedUserID() == steamID)
				{
					name = player.Nickname;
					return true;
				}
			}
			return false;
		}
	}
}
