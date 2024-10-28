using Newtonsoft.Json;
using SCPDiscord.EventListeners;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using GameCore;
using Mirror.LiteNetLib4Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using YamlDotNet.Core;

[assembly:AssemblyVersion(SCPDiscord.SCPDiscord.VERSION)]
[assembly:AssemblyTitle("SCP:SL Discord Plugin")]
[assembly:AssemblyCompany("https://github.com/KarlOfDuty/SCPDiscord")]
[assembly:AssemblyCopyright("© 2018-2024 Karl Essinger and contributors - licenced under GPLv3")]
[assembly:AssemblyProduct("SCPDiscord")]

namespace SCPDiscord
{
  public class SCPDiscord
  {
    public const string VERSION = "3.2.2";

    private readonly Stopwatch serverStartTime = new Stopwatch();

    internal SynchronousExecutor sync;

    internal static SCPDiscord plugin;

    internal bool roundStarted = false;

    internal bool shutdown;

    private Utilities.FileWatcher reservedSlotsWatcher;

    //private Utilities.FileWatcher vanillaMutesWatcher;
    private Utilities.FileWatcher whitelistWatcher;

    [PluginEntryPoint("SCPDiscord", VERSION, "SCP:SL - Discord bridge.", "Karl Essinger")]
    public void Start()
    {
      plugin = this;

      if (!LoadConfig())
        return;

      serverStartTime.Start();

      LiteNetLib4MirrorNetworkManager.singleton.gameObject.AddComponent<SynchronousExecutor>();
      sync = LiteNetLib4MirrorNetworkManager.singleton.gameObject.GetComponent<SynchronousExecutor>();

      // Event handlers
      EventManager.RegisterEvents(this, sync);
      EventManager.RegisterEvents(this, new MuteEventListener());
      EventManager.RegisterEvents(this, new TimeTrackingListener());
      EventManager.RegisterEvents(this, new SyncPlayerRole());
      EventManager.RegisterEvents(this, new PlayerEventListener(this));
      EventManager.RegisterEvents(this, new ServerEventListener(this));
      EventManager.RegisterEvents(this, new EnvironmentEventListener(this));

      if (Server.Port == Config.GetInt("bot.port"))
      {
        Logger.Error("ERROR: Server is running on the same port as the plugin, aborting...");
        throw new Exception();
      }

      Logger.Info("Loading language system...");
      Language.Reload();

      Logger.Info("Loading rolesync system...");
      RoleSync.Reload();

      Logger.Info("Loading mute system...");
      MuteSystem.Reload();

      Logger.Info("Loading playtime system...");
      PlayTime.Reload();

      new Thread(() => new StartNetworkSystem()).Start();

      Logger.Info("SCPDiscord " + VERSION + " enabled.");
    }

    private class SyncPlayerRole
    {
      [PluginEvent(ServerEventType.PlayerJoined)]
      public void OnPlayerJoin(Player player)
      {
        if (player == null || !Config.GetBool("settings.rolesync")) return;

        try
        {
          RoleSync.SendRoleQuery(player);
        }
        catch (Exception e)
        {
          Logger.Error("Error occured when checking player for rolesync!\n" + e);
        }
      }
    }

    public bool LoadConfig()
    {
      try
      {
        reservedSlotsWatcher?.Dispose();
        //vanillaMutesWatcher?.Dispose();
        whitelistWatcher?.Dispose();
        Config.Reload(plugin);

        if (Config.GetBool("settings.autoreload.reservedslots"))
        {
          reservedSlotsWatcher = new Utilities.FileWatcher(Config.GetReservedSlotDir(), "UserIDReservedSlots.txt", ReservedSlot.Reload);
        }

        /*if (Config.GetBool("settings.autoreload.mutes"))
        {
          vanillaMutesWatcher = new Utilities.FileWatcher(ConfigSharing.Paths[1], "mutes.txt", VoiceChatMutes.LoadMutes);
        }*/

        if (Config.GetBool("settings.autoreload.whitelist"))
        {
          whitelistWatcher = new Utilities.FileWatcher(ConfigSharing.Paths[2], "UserIDWhitelist.txt", WhiteList.Reload);
        }

        Logger.Info("Loaded config \"" + Config.GetConfigPath() + "\".");
        return true;
      }
      catch (Exception e)
      {
        switch (e)
        {
          case DirectoryNotFoundException _:
            Logger.Error("Config directory not found.");
            break;
          case UnauthorizedAccessException _:
            Logger.Error("Config file access denied.");
            break;
          case FileNotFoundException _:
            Logger.Error("\"" + Config.GetConfigPath() + "\" was not found.");
            break;
          case JsonReaderException jsonEx:
            Logger.Error("\"" + Config.GetConfigPath() + "\" formatting error:\n" + jsonEx.Message);
            break;
          case YamlException yamlEx:
            Logger.Error("\"" + Config.GetConfigPath() + "\" formatting error:\n" + yamlEx.Message);
            break;
          case Config.ConfigParseException _:
            Logger.Error("Formatting issue in config file \"" + Config.GetConfigPath() + "\". Aborting startup.");
            break;
          default:
            Logger.Error("Error reading config file \"" + Config.GetConfigPath() + "\". Aborting startup.\n" + e);
            break;
        }
      }

      return false;
    }

    public void OnDisable()
    {
      shutdown = true;
      NetworkSystem.Disconnect();
      Logger.Info("SCPDiscord disabled.");
    }

    public static void SendStringByID(ulong channelID, string message)
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

    public static void SendEmbedByID(EmbedMessage message)
    {
      NetworkSystem.QueueMessage(new MessageWrapper { EmbedMessage = message });
    }

    public static void SendMessage(string messagePath, Dictionary<string, string> variables = null)
    {
      List<ulong> channelIDs = Config.GetChannelIDs(messagePath);
      if (channelIDs.Count == 0)
      {
        return;
      }

      Thread messageThread = new Thread(() => new ProcessMessageAsync(channelIDs, messagePath, variables));
      messageThread.Start();
    }

    public static void SendEmbedWithMessage(string messagePath, EmbedMessage embed, Dictionary<string, string> variables = null)
    {
      List<ulong> channelIDs = Config.GetChannelIDs(messagePath);
      if (channelIDs.Count == 0)
      {
        return;
      }

      Thread messageThread = new Thread(() => new ProcessEmbedMessageAsync(embed, channelIDs, messagePath, variables));
      messageThread.Start();
    }

    public static void SendMessageByID(ulong channelID, string messagePath, Dictionary<string, string> variables = null)
    {
      new Thread(() => new ProcessMessageByIDAsync(channelID, messagePath, variables)).Start();
    }

    public static void SendEmbedWithMessageByID(EmbedMessage embed, string messagePath, Dictionary<string, string> variables = null)
    {
      new Thread(() => new ProcessEmbedMessageByIDAsync(embed, messagePath, variables)).Start();
    }
  }
}