using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Hosting.Systemd;
using Tmds.Systemd;
using ServiceState = Tmds.Systemd.ServiceState;

namespace SCPDiscord
{
  public class SCPDiscordBot
  {
    public class CommandLineArguments
    {
      [Option('c',
        "config",
        Required = false,
        HelpText = "Select a config file to use.",
        Default = "config.yml",
        MetaValue = "PATH")]
      public string ConfigPath { get; set; }

      [Option('l',
        "log-file",
        Required = false,
        HelpText = "Select log file to write bot logs to.",
        MetaValue = "PATH")]
      public string LogFilePath { get; set; }

      [Option(
        "leave",
        Required = false,
        HelpText = "Leaves one or more Discord servers. " +
                   "You can check which servers your bot is in when it starts up.",
        MetaValue = "ID,ID,ID...",
        Separator = ','
      )]
      public IEnumerable<ulong> ServersToLeave { get; set; }

      [Option("print-default-config",
        Required = false,
        HelpText = "Print the default config.")]
      public bool PrintDefaultConfig { get; set; }
    }

    internal static CommandLineArguments commandLineArgs;

    private static readonly Channel<PosixSignal> signalChannel = Channel.CreateUnbounded<PosixSignal>();

    // ServiceManager will steal this variable later so we have to copy it while we have the chance.
    private static readonly string systemdSocket = Environment.GetEnvironmentVariable("NOTIFY_SOCKET");

    private static void HandleSignal(PosixSignalContext context)
    {
      context.Cancel = true;
      signalChannel.Writer.TryWrite(context.Signal);
    }

    private static async Task<int> Main(string[] args)
    {
      if (SystemdHelpers.IsSystemdService())
      {
        Journal.SyslogIdentifier = Assembly.GetEntryAssembly()?.GetName().Name;
        PosixSignalRegistration.Create(PosixSignal.SIGHUP, HandleSignal);
      }

      PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal);
      PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);

      StringWriter sw = new StringWriter();
      commandLineArgs = new Parser(settings =>
      {
        settings.AutoHelp = true;
        settings.HelpWriter = sw;
        settings.AutoVersion = false;
      }).ParseArguments<CommandLineArguments>(args).Value;

      // CommandLineParser has some bugs related to the built-in version option, ignore the output if it isn't found.
      if (!sw.ToString().Contains("Option 'version' is unknown."))
      {
        Console.Write(sw);
      }

      if (args.Contains("--help"))
      {
        return 0;
      }

      if (args.Contains("--version"))
      {
        Console.WriteLine("SCPDiscord " + GetVersion());
        Console.WriteLine("Build time: " + BuildInfo.BuildTimeUTC.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
        return 0;
      }

      if (commandLineArgs.PrintDefaultConfig)
      {
        Console.Write(Utilities.ReadManifestData("default_config.yml"));
        return 0;
      }

      Logger.Log("Starting SCPDiscord version " + GetVersion() + "...");
      try
      {
        try
        {
          ConfigParser.LoadConfig();
        }
        catch (Exception e)
        {
          Logger.Fatal("Error loading config!", e);
          return 1;
        }

        await DiscordAPI.Init();

        NetworkSystem.Start();
        MessageScheduler.Start();

        ServiceManager.Notify(ServiceState.Ready);

        // Loop here until application closes, handle any signals received
        while (await signalChannel.Reader.WaitToReadAsync())
        {
          while (signalChannel.Reader.TryRead(out PosixSignal signal))
          {
            switch (signal)
            {
              case PosixSignal.SIGHUP:
                // Tmds.Systemd.ServiceManager doesn't support the notify-reload service type so we have to send the reloading message manually.
                // According to the documentation this shouldn't be the right way to calculate MONOTONIC_USEC, but it works for some reason.
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"RELOADING=1\nMONOTONIC_USEC={DateTimeOffset.UtcNow.ToUnixTimeMicroseconds()}\n");
                UnixDomainSocketEndPoint ep = new(systemdSocket);
                using (Socket cl = new(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified))
                {
                  await cl.ConnectAsync(ep);
                  cl.Send(data);
                }

                // TODO: Restart the network system
                await NetworkSystem.Stop();
                ConfigParser.LoadConfig();
                NetworkSystem.Start();
                ServiceManager.Notify(ServiceState.Ready);
                break;
              case PosixSignal.SIGTERM:
                Logger.Log("Shutting down...");
                ServiceManager.Notify(ServiceState.Stopping);
                await NetworkSystem.Stop();
                await MessageScheduler.Stop();
                // TODO: Stop Discord client, network connection
                return 0;
              case PosixSignal.SIGINT:
                Logger.Warn("Received interrupt signal, shutting down...");
                ServiceManager.Notify(ServiceState.Stopping);
                await NetworkSystem.Stop();
                await MessageScheduler.Stop();
                // TODO: Stop Discord client, network connection
                return 0;
              default:
                break;
            }
          }
        }
      }
      catch (Exception e)
      {
          Logger.Fatal("Fatal error.", e);
          return 3;
      }

      return 0;
    }

    public static string GetVersion()
    {
      Version version = Assembly.GetEntryAssembly()?.GetName().Version;
      return version?.Major + "."
           + version?.Minor + "."
           + version?.Build
           + (version?.Revision == 0 ? "" : "-" + (char)(64 + version?.Revision ?? 0))
           + " (" + ThisAssembly.Git.Commit + ")";
    }
  }
}