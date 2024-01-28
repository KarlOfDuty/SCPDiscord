using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;

namespace SCPDiscord
{
	public class DiscordAPI
	{
		public static DiscordAPI instance = null;
		public bool connected = false;

		private static readonly DiscordConfiguration config = new()
		{
			Token = "DUMMY_TOKEN",
			TokenType = TokenType.Bot,
			MinimumLogLevel = LogLevel.Debug,
			AutoReconnect = true,
			Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
			LogTimestampFormat = "HH:mm:ss",
			LogUnknownAuditlogs = false,
			LogUnknownEvents = false
		};
		public static DiscordClient client = new DiscordClient(config);
		private static DiscordRestClient restClient = null;
		private SlashCommandsExtension commands = null;

		public static async Task Init()
		{
			try
			{
				Logger.Log("Setting up Discord client...", LogID.DISCORD);

				instance = new DiscordAPI();

				// Checking log level
				if (!Enum.TryParse(ConfigParser.config.bot.logLevel, true, out LogLevel logLevel))
				{
					Logger.Warn("Log level '" + ConfigParser.config.bot.logLevel + "' invalid, using 'Information' instead.", LogID.CONFIG);
					logLevel = LogLevel.Information;
				}

				// Check if token is unset
				if (ConfigParser.config.bot.token == "add-your-token-here" || ConfigParser.config.bot.token == "")
				{
					Logger.Fatal("You need to set your bot token in the config and start the bot again.", LogID.CONFIG);
					throw new ArgumentException("Discord bot token has not been set in config");
				}

				// Setting up client configuration
				config.Token = ConfigParser.config.bot.token;
				config.MinimumLogLevel = logLevel;
				client = new DiscordClient(config);

				ConfigParser.PrintConfig();

				if (!ConfigParser.config.bot.disableCommands)
				{
					client.UseInteractivity(new InteractivityConfiguration
					{
						PaginationBehaviour = PaginationBehaviour.Ignore,
						PaginationDeletion = PaginationDeletion.DeleteMessage,
						Timeout = TimeSpan.FromMinutes(15)
					});

					Logger.Log("Registering commands...", LogID.DISCORD);
					instance.commands = client.UseSlashCommands();

					instance.commands.RegisterCommands<Commands.BanCommand>();
					instance.commands.RegisterCommands<Commands.HelpCommand>();
					instance.commands.RegisterCommands<Commands.KickAllCommand>();
					instance.commands.RegisterCommands<Commands.KickCommand>();
					instance.commands.RegisterCommands<Commands.ListCommand>();
					instance.commands.RegisterCommands<Commands.MuteCommand>();
					instance.commands.RegisterCommands<Commands.RACommand>();
					instance.commands.RegisterCommands<Commands.ServerCommand>();
					instance.commands.RegisterCommands<Commands.SyncIPCommand>();
					instance.commands.RegisterCommands<Commands.SyncSteamIDCommand>();
					instance.commands.RegisterCommands<Commands.UnbanCommand>();
					instance.commands.RegisterCommands<Commands.UnmuteCommand>();
					instance.commands.RegisterCommands<Commands.UnsyncCommand>();
					instance.commands.RegisterCommands<Commands.UnsyncPlayerCommand>();
				}

				Logger.Log("Hooking events...", LogID.DISCORD);
				client.SessionCreated += instance.OnReady;
				client.GuildAvailable += instance.OnGuildAvailable;
				client.ClientErrored += instance.OnClientError;
				client.SocketErrored += instance.OnSocketError;

				Logger.Log("Hooking command events...", LogID.DISCORD);
				instance.commands.SlashCommandErrored += instance.OnCommandError;

				Logger.Log("Connecting to Discord...", LogID.DISCORD);
				await client.ConnectAsync();

				Logger.Log("Initializing REST API...", LogID.DISCORD);
				restClient = new DiscordRestClient(new DiscordConfiguration
				{
					Token = ConfigParser.config.bot.token,
					TokenType = TokenType.Bot,
					MinimumLogLevel = logLevel,
					AutoReconnect = true,
					Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
					LogTimestampFormat = "yyyy-MM-dd HH:mm:ss"
				});
				await restClient.InitializeAsync();
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString(), LogID.DISCORD);
			}
		}

		public static void SetDisconnectedActivity()
		{
			// Checking activity type
			if (!Enum.TryParse(ConfigParser.config.bot.presenceType, true, out ActivityType activityType))
			{
				Logger.Warn("Presence type '" + ConfigParser.config.bot.presenceType + "' invalid, using 'Playing' instead.", LogID.DISCORD);
				activityType = ActivityType.Playing;
			}

			SetActivity(ConfigParser.config.bot.presenceText, activityType, UserStatus.DoNotDisturb);
		}

		public static void SetActivity(string activityText, ActivityType activityType, UserStatus status)
		{
			if (instance.connected)
				client.UpdateStatusAsync(new DiscordActivity(activityText, activityType), status);
		}

		public static async Task SendMessage(ulong channelID, string message)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await client.GetChannelAsync(channelID);
				try
				{
					Logger.Debug("Sending message to " + channelID, LogID.DISCORD);
					await channel.SendMessageAsync(message);
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send message in '" + channel.Name + "'", LogID.DISCORD);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not send message in text channel '" + channelID + "'", LogID.DISCORD);
			}
		}

		public static async Task SendMessage(ulong channelID, DiscordEmbed message)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await client.GetChannelAsync(channelID);
				try
				{
					await channel.SendMessageAsync(message);
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send message in '" + channel.Name + "'", LogID.DISCORD);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not send embed in text channel '" + channelID + "'", LogID.DISCORD);
			}
		}

		public static async Task SendInteractionResponse(ulong interactionID, ulong channelID, DiscordEmbed message)
		{
			if (!instance.connected) return;

			try
			{
				try
				{
					if (MessageScheduler.TryUncacheInteraction(interactionID, out InteractionContext interaction))
					{
						await interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(message));
					}
					else
					{
						Logger.Error("Couldn't find interaction in the cache, sending as normal message instead.");
						await SendMessage(channelID, message);
					}
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send command response.", LogID.DISCORD);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Could not send command response.\n" + e, LogID.DISCORD);
			}
		}

		public static async Task SendPaginatedResponse(ulong interactionID, ulong channelID, ulong userID, List<Page> message)
		{
			if (!instance.connected) return;

			try
			{
				try
				{
					if (MessageScheduler.TryUncacheInteraction(interactionID, out InteractionContext interaction))
					{
						await interaction.Interaction.SendPaginatedResponseAsync(false, interaction.User, message, default,
							                                                     default, default, default, true);
					}
					else
					{
						Logger.Error("Couldn't find interaction in the cache, sending as normal message instead.");
						await SendPaginatedMessage(channelID, userID, message);
					}
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send command response.", LogID.DISCORD);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Could not send command response.\n" + e, LogID.DISCORD);
			}
		}

		public static async Task SendPaginatedMessage(ulong channelID, ulong userID, IEnumerable<Page> pages)
		{
			if (!instance.connected) return;

			try
			{
				try
				{
					DiscordChannel channel = await client.GetChannelAsync(channelID);
					DiscordUser user = await client.GetUserAsync(userID);

					await channel.SendPaginatedMessageAsync(user, pages);
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send command response.", LogID.DISCORD);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Could not send command response.\n" + e, LogID.DISCORD);
			}
		}

		public static async void GetPlayerRoles(ulong userID, string steamID)
		{
			if (!instance.connected) return;

			if (ConfigParser.config.bot.serverId == 0)
			{
				Logger.Warn("Plugin attempted to use role sync, but no server ID was set in the config. Ignoring request...", LogID.DISCORD);
				return;
			}

			try
			{
				DiscordGuild guild = await client.GetGuildAsync(ConfigParser.config.bot.serverId);
				DiscordMember member = await guild.GetMemberAsync(userID);

				Interface.MessageWrapper message = new Interface.MessageWrapper
				{
					UserInfo = new Interface.UserInfo
					{
						DiscordID = userID,
						SteamIDOrIP = steamID,
						DiscordDisplayName = member.DisplayName,
						DiscordUsername = member.Username
					}
				};
				message.UserInfo.RoleIDs.AddRange(member.Roles.Select(role => role.Id));
				message.UserInfo.RoleIDs.Add(member.Guild.EveryoneRole.Id);
				await NetworkSystem.SendMessage(message, null);
			}
			catch (Exception e)
			{
				Logger.Warn("Couldn't find discord server or server member for role syncing requested by plugin. Discord ID: " + userID + " SteamID/IP: " + steamID, LogID.DISCORD);
				Logger.Debug("Exception: \n" + e, LogID.DISCORD);
			}
		}

		public Task OnReady(DiscordClient discordClient, SessionReadyEventArgs e)
		{
			instance.connected = true;
			Logger.Log("Connected to Discord.", LogID.DISCORD);
			SetDisconnectedActivity();
			return Task.CompletedTask;
		}

		public Task OnSocketError(DiscordClient discordClient, SocketErrorEventArgs e)
		{
			Logger.Debug("Discord socket error: " + e.Exception, LogID.DISCORD);
			return Task.CompletedTask;
		}

		public Task OnGuildAvailable(DiscordClient discordClient, GuildCreateEventArgs e)
		{
			Logger.Log("Found Discord server: " + e.Guild.Name, LogID.DISCORD);

			IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;

			foreach ((ulong roleID, DiscordRole role) in roles)
			{
				Logger.Debug(role.Name.PadRight(40, '.') + roleID, LogID.DISCORD);
			}
			return Task.CompletedTask;
		}

		public Task OnClientError(DiscordClient discordClient, ClientErrorEventArgs e)
		{
			Logger.Error($"Exception occured: {e.Exception.GetType()}: {e.Exception}", LogID.DISCORD);

			return Task.CompletedTask;
		}

		public Task OnCommandError(SlashCommandsExtension commandSystem, SlashCommandErrorEventArgs e)
		{
			switch (e.Exception)
			{
				case ArgumentException:
				{
					DiscordEmbed error = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Red,
						Description = "Internal error occurred."
					};
					e.Context.Channel.SendMessageAsync(error);
					return Task.CompletedTask;
				}

				case SlashExecutionChecksFailedException ex:
				{
					foreach (SlashCheckBaseAttribute attr in ex.FailedChecks)
					{
						DiscordEmbed error = new DiscordEmbedBuilder
						{
							Color = DiscordColor.Red,
							Description = ParseFailedCheck(attr)
						};
						e.Context.Channel.SendMessageAsync(error);
					}
					return Task.CompletedTask;
				}

				default:
				{
					Logger.Error("Exception occured: " + e.Exception, LogID.DISCORD);
					DiscordEmbed error = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Red,
						Description = "Internal error occured, please report this to the developer."
					};
					e.Context.Channel.SendMessageAsync(error);
					return Task.CompletedTask;
				}
			}
		}

		private static string ParseFailedCheck(SlashCheckBaseAttribute attr)
		{
			return attr switch
			{
				SlashRequireDirectMessageAttribute => "This command can only be used in direct messages!",
				SlashRequireOwnerAttribute => "Only the server owner can use that command!",
				SlashRequirePermissionsAttribute => "You don't have permission to do that!",
				SlashRequireBotPermissionsAttribute => "The bot doesn't have the required permissions to do that!",
				SlashRequireUserPermissionsAttribute => "You don't have permission to do that!",
				SlashRequireGuildAttribute => "This command has to be used in a Discord server!",
				_ => "Unknown Discord API error occured, please try again later."
			};
		}
	}
}
