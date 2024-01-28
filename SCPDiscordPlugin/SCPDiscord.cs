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

		public const string VERSION = "3.1.5";

		[PluginEntryPoint("SCPDiscord", VERSION, "SCP:SL - Discord bridge.", "Karl Essinger")]
		public void Start()
		{
			plugin = this;

			serverStartTime.Start();

			LiteNetLib4MirrorNetworkManager.singleton.gameObject.AddComponent<SynchronousExecutor>();
			sync = LiteNetLib4MirrorNetworkManager.singleton.gameObject.GetComponent<SynchronousExecutor>();

			// Event handlers
            EventManager.RegisterEvents(this, sync);
            EventManager.RegisterEvents(this, new MuteEventListener());
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
				Logger.Error("ERROR: Server is running on the same port as the plugin, aborting...");
				throw new Exception();
			}
			Language.Reload();

			new Thread(() => new MuteFileReloader());
			new Thread(() => new StartNetworkSystem(plugin)).Start();

			Logger.Info("SCPDiscord " + VERSION + " enabled.");
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
				Logger.Info("Successfully loaded config '" + Config.GetConfigPath() + "'.");
				return true;
			}
			catch (Exception e)
			{
				if (e is DirectoryNotFoundException)
				{
					Logger.Error("Config directory not found.");
				}
				else if (e is UnauthorizedAccessException)
				{
					Logger.Error("Primary language file access denied.");
				}
				else if (e is FileNotFoundException)
				{
					Logger.Error("'" + Config.GetConfigPath() + "' was not found.");
				}
				else if (e is JsonReaderException || e is YamlException)
				{
					Logger.Error("'" + Config.GetConfigPath() + "' formatting error.");
				}
				else if (e is Config.ConfigParseException)
				{
					Logger.Error("Formatting issue in config file '" + Config.GetConfigPath() + "'. Aborting startup.");
				}
				else
				{
					Logger.Error("Error reading config file '" + Config.GetConfigPath() + "'. Aborting startup.\n" + e);
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
	}
}
