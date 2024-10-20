using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

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
            public string configPath { get; set; }

            [Option(
                "leave",
                Required = false,
                HelpText = "Leaves one or more Discord servers. " +
                           "You can check which servers your bot is in when it starts up.",
                MetaValue = "ID,ID,ID...",
                Separator = ','
            )]
            public IEnumerable<ulong> serversToLeave { get; set; }
        }

        internal static CommandLineArguments commandLineArgs;

        private static void Main(string[] args)
        {
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
                return;
            }

            if (args.Contains("--version"))
            {
                Console.WriteLine(Assembly.GetEntryAssembly()?.GetName().Name + ' ' + GetVersion());
                Console.WriteLine("Build time: " + BuildInfo.BuildTimeUTC.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
                return;
            }

            new SCPDiscordBot().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            Logger.Log("Starting SCPDiscord version " + GetVersion() + "...", LogID.GENERAL);
            try
            {
                try
                {
                    ConfigParser.LoadConfig();
                }
                catch (Exception e)
                {
                    Logger.Fatal("Error loading config!\n" + e);
                    return;
                }


                new Thread(() => new StartNetworkSystem()).Start();
                new Thread(() => new StartMessageScheduler()).Start();
                await DiscordAPI.Init();

                // Block this task until the program is closed.
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Logger.Fatal("Fatal error:", LogID.GENERAL);
                Logger.Fatal(e.ToString(), LogID.GENERAL);
                Console.ReadLine();
            }
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
