using Newtonsoft.Json.Linq;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralAuth;
using Newtonsoft.Json;
using PluginAPI.Core;

namespace SCPDiscord
{
	public static class RoleSync
	{
		private static Dictionary<string, ulong> syncedPlayers = new Dictionary<string, ulong>();

		private static Utilities.FileWatcher fileWatcher;

		public static void Reload()
		{
			fileWatcher?.Dispose();

			if (!Directory.Exists(Config.GetRolesyncDir()))
			{
				Directory.CreateDirectory(Config.GetRolesyncDir());
			}

			if (!File.Exists(Config.GetRolesyncPath()))
			{
				Logger.Info("Rolesync file " + Config.GetRolesyncPath() + " does not exist, creating...");
				File.WriteAllText(Config.GetRolesyncPath(), "{}");
			}

			try
			{
				syncedPlayers = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Config.GetRolesyncPath()));
			}
			catch (Exception)
			{
				try
				{
					Logger.Warn("Could not read rolesync file '" + Config.GetRolesyncPath() + "', attempting to convert file from old format...");
					syncedPlayers = JArray.Parse(File.ReadAllText(Config.GetRolesyncPath())).ToDictionary(
						k => ((JObject)k).Properties().First().Name,
						v => v.Values().First().Value<ulong>());
					File.WriteAllText(Config.GetRolesyncPath(), JsonConvert.SerializeObject(syncedPlayers, Formatting.Indented));
				}
				catch (Exception e)
				{
					Logger.Error("Could not read rolesync file '" + Config.GetRolesyncPath() + "', check the file formatting and try again.");
					throw;
				}
			}

			fileWatcher = new Utilities.FileWatcher(Config.GetRolesyncDir(), "rolesync.json", Reload);
			Logger.Debug("Successfully loaded '" + Config.GetRolesyncPath() + "'.");
		}

		private static void SavePlayers()
		{
			File.WriteAllText(Config.GetRolesyncPath(), JsonConvert.SerializeObject(syncedPlayers, Formatting.Indented));
		}

		public static void SendRoleQuery(Player player)
		{
			if (PlayerAuthenticationManager.OnlineMode)
			{
				if (!syncedPlayers.ContainsKey(player.UserId))
				{
					Logger.Debug("User ID '" + player.UserId + "' is not in rolesync list.");
					return;
				}

				MessageWrapper message = new MessageWrapper
				{
					UserQuery = new UserQuery
					{
						SteamIDOrIP = player.UserId,
						DiscordID = syncedPlayers[player.UserId]
					}
				};

				NetworkSystem.QueueMessage(message);
			}
			else
			{
				if (!syncedPlayers.ContainsKey(player.IpAddress))
				{
					Logger.Debug("IP '" + player.IpAddress + "' is not in rolesync list.");
					return;
				}

				MessageWrapper message = new MessageWrapper
				{
					UserQuery = new UserQuery
					{
						SteamIDOrIP = player.IpAddress,
						DiscordID = syncedPlayers[player.IpAddress]
					}
				};

				NetworkSystem.QueueMessage(message);
			}
		}

		public static void ReceiveQueryResponse(UserInfo userInfo)
		{
			Task.Delay(1000);
			try
			{
				// For online servers this should always be one player but for offline servers it may match several
				List<Player> matchingPlayers = new List<Player>();
				try
				{
					Logger.Debug("Looking for player with SteamID/IP: " + userInfo.SteamIDOrIP);
					foreach (Player pl in Player.GetPlayers<Player>())
					{
						Logger.Debug("Player " + pl.PlayerId + ": SteamID " + pl.UserId + " IP " + pl.IpAddress);
						if (pl.UserId == userInfo.SteamIDOrIP)
						{
							Logger.Debug("Matching SteamID found");
							matchingPlayers.Add(pl);
						}
						else if (pl.IpAddress == userInfo.SteamIDOrIP)
						{
							Logger.Debug("Matching IP found");
							matchingPlayers.Add(pl);
						}
					}
				}
				catch (NullReferenceException e)
				{
					Logger.Error("Error getting player for RoleSync: " + e);
					return;
				}

				if (matchingPlayers.Count == 0)
				{
					Logger.Error("Could not get player for rolesync, did they disconnect immediately?");
					return;
				}

				foreach (Player player in matchingPlayers)
				{
					foreach (KeyValuePair<ulong, string[]> keyValuePair in Config.roleDictionary)
					{
						Logger.Debug("User has discord role " + keyValuePair.Key + ": " + userInfo.RoleIDs.Contains(keyValuePair.Key));
						if (userInfo.RoleIDs.Contains(keyValuePair.Key))
						{
							Dictionary<string, string> variables = new Dictionary<string, string>
							{
								{ "discorddisplayname", userInfo.DiscordDisplayName   },
								{ "discordusername",    userInfo.DiscordUsername      },
								{ "discordid",          userInfo.DiscordID.ToString() }
							};
							variables.AddPlayerVariables(player, "player");

							foreach (string unparsedCommand in keyValuePair.Value)
							{
								string command = unparsedCommand;
								// Variable insertion
								foreach (KeyValuePair<string, string> variable in variables)
								{
									command = command.Replace("<var:" + variable.Key + ">", variable.Value);
								}
								Logger.Debug("Running rolesync command: " + command);
								SCPDiscord.plugin.sync.ScheduleRoleSyncCommand(command);
							}

							Logger.Info("Synced " + player.Nickname + " (" + userInfo.SteamIDOrIP + ") with Discord role id " + keyValuePair.Key);
							return;
						}
					}
				}
			}
			catch (InvalidOperationException)
			{
				Logger.Warn("Tried to run commands on a player who is not on the server anymore.");
			}
		}

		public static EmbedMessage AddPlayer(SyncRoleCommand command)
		{
			if (PlayerAuthenticationManager.OnlineMode)
			{
				if (syncedPlayers.ContainsKey(command.SteamIDOrIP + "@steam"))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = command.ChannelID,
						Description = "SteamID is already linked to a Discord account. You will have to remove it first.",
						InteractionID = command.InteractionID
					};
				}

				if (syncedPlayers.ContainsValue(command.DiscordID))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = command.ChannelID,
						Description = "Discord user ID is already linked to a Steam account. You will have to remove it first.",
						InteractionID = command.InteractionID
					};
				}

				if (!Utilities.TryGetSteamName(command.SteamIDOrIP, out string _))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = command.ChannelID,
						Description = "Could not find a user in the Steam API using that ID.",
						InteractionID = command.InteractionID
					};
				}

				syncedPlayers.Add(command.SteamIDOrIP + "@steam", command.DiscordID);
				SavePlayers();
				return new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Green,
					ChannelID = command.ChannelID,
					Description = "Successfully linked accounts.",
					InteractionID = command.InteractionID
				};
			}
			else
			{
				if (syncedPlayers.ContainsKey(command.SteamIDOrIP))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = command.DiscordID,
						Description = "IP is already linked to a Discord account. You will have to remove it first.",
						InteractionID = command.InteractionID
					};
				}

				if (syncedPlayers.ContainsValue(command.DiscordID))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = command.ChannelID,
						Description = "Discord user ID is already linked to an IP. You will have to remove it first.",
						InteractionID = command.InteractionID
					};
				}

				syncedPlayers.Add(command.SteamIDOrIP, command.DiscordID);
				SavePlayers();
				return new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Green,
					ChannelID = command.ChannelID,
					Description = "Successfully linked accounts.",
					InteractionID = command.InteractionID
				};
			}
		}

		public static EmbedMessage RemovePlayer(UnsyncRoleCommand command)
		{
			if (!syncedPlayers.ContainsValue(command.DiscordID))
			{
				return new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Red,
					ChannelID = command.ChannelID,
					Description = "Discord user ID is not linked to a Steam account or IP",
					InteractionID = command.InteractionID
				};
			}

			KeyValuePair<string, ulong> player = syncedPlayers.First(kvp => kvp.Value == command.DiscordID);
			syncedPlayers.Remove(player.Key);
			SavePlayers();
			return new EmbedMessage
			{
				Colour = EmbedMessage.Types.DiscordColour.Green,
				ChannelID = command.ChannelID,
				Description = "Discord user ID link has been removed.",
				InteractionID = command.InteractionID
			};
		}

		public static string RemovePlayerLocally(ulong discordID)
		{
			if (!syncedPlayers.ContainsValue(discordID))
			{
				return "Discord user ID is not linked to a Steam account or IP";
			}

			KeyValuePair<string, ulong> player = syncedPlayers.First(kvp => kvp.Value == discordID);
			syncedPlayers.Remove(player.Key);
			SavePlayers();
			return "Discord user ID link has been removed.";
		}

		public static bool IsPlayerSynced(string userID, out ulong discordID)
		{
			return syncedPlayers.TryGetValue(userID.EndsWith("@steam") ? userID : userID + "@steam", out discordID);
		}

		public static bool IsPlayerSynced(ulong discordID, out string userID)
		{
			return syncedPlayers.TryGetFirstKey(discordID, out userID);
		}

		public static Dictionary<string, ulong> GetSyncedPlayers()
		{
			return syncedPlayers;
		}
	}
}
