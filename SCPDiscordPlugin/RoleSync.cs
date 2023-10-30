using Newtonsoft.Json.Linq;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;

namespace SCPDiscord
{
	public class RoleSync
	{
		private Dictionary<string, ulong> syncedPlayers = new Dictionary<string, ulong>();

		private readonly SCPDiscord plugin;

		public RoleSync(SCPDiscord plugin)
		{
			this.plugin = plugin;
			Reload();
			plugin.Info("RoleSync system loaded.");
		}

		public void Reload()
		{
			if (!Directory.Exists(Config.GetRolesyncDir()))
			{
				Directory.CreateDirectory(Config.GetRolesyncDir());
			}

			if (!File.Exists(Config.GetRolesyncPath()))
			{
				plugin.Info("Config file rolesync.json does not exist, creating...");
				File.WriteAllText(Config.GetRolesyncPath(), "[]");
			}

			syncedPlayers = JArray.Parse(File.ReadAllText(Config.GetRolesyncPath())).ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<ulong>());
			plugin.Info("Successfully loaded rolesync '" + Config.GetRolesyncDir() + "rolesync.json'.");
		}

		private void SavePlayers()
		{
			// Save the state to file
			StringBuilder builder = new StringBuilder();
			builder.Append("[\n");
			foreach (KeyValuePair<string, ulong> player in syncedPlayers)
			{
				builder.Append("    {\"" + player.Key + "\": \"" + player.Value + "\"},\n");
			}
			builder.Append("]");
			File.WriteAllText(Config.GetRolesyncDir() + "rolesync.json", builder.ToString());
		}

		public void SendRoleQuery(Player player)
		{
			if (CharacterClassManager.OnlineMode)
			{
				if (!syncedPlayers.ContainsKey(player.UserId))
				{
					plugin.Debug("User ID '" + player.UserId + "' is not in rolesync list.");
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
					plugin.Debug("IP '" + player.IpAddress + "' is not in rolesync list.");
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

		public void ReceiveQueryResponse(UserInfo userInfo)
		{
			Task.Delay(1000);
			try
			{
				// For online servers this should always be one player but for offline servers it may match several
				List<Player> matchingPlayers = new List<Player>();
				try
				{
					plugin.Debug("Looking for player with SteamID/IP: " + userInfo.SteamIDOrIP);
					foreach (Player pl in Player.GetPlayers<Player>())
					{
						plugin.Debug("Player " + pl.PlayerId + ": SteamID " + pl.UserId + " IP " + pl.IpAddress);
						if (pl.UserId == userInfo.SteamIDOrIP)
						{
							plugin.Debug("Matching SteamID found");
							matchingPlayers.Add(pl);
						}
						else if (pl.IpAddress == userInfo.SteamIDOrIP)
						{
							plugin.Debug("Matching IP found");
							matchingPlayers.Add(pl);
						}
					}
				}
				catch (NullReferenceException e)
				{
					plugin.Error("Error getting player for RoleSync: " + e);
					return;
				}

				if (matchingPlayers.Count == 0)
				{
					plugin.Error("Could not get player for rolesync, did they disconnect immediately?");
					return;
				}

				foreach (Player player in matchingPlayers)
				{
					foreach (KeyValuePair<ulong, string[]> keyValuePair in Config.roleDictionary)
					{
						plugin.Debug("User has discord role " + keyValuePair.Key + ": " + userInfo.RoleIDs.Contains(keyValuePair.Key));
						if (userInfo.RoleIDs.Contains(keyValuePair.Key))
						{
							Dictionary<string, string> variables = new Dictionary<string, string>
							{
								{ "ipaddress",                        player.IpAddress                          },
								{ "name",                             player.Nickname                           },
								{ "playerid",                         player.PlayerId.ToString()                },
								{ "userid",                           player.UserId                             },
								{ "steamid",                          player.GetParsedUserID()                  },
								{ "discorddisplayname",               userInfo.DiscordDisplayName               },
								{ "discordusername",                  userInfo.DiscordUsername                  },
								{ "discordusernamewithdiscriminator", userInfo.DiscordUsernameWithDiscriminator },
								{ "discordid",                        userInfo.DiscordID.ToString()             }
							};
							foreach (string unparsedCommand in keyValuePair.Value)
							{
								string command = unparsedCommand;
								// Variable insertion
								foreach (KeyValuePair<string, string> variable in variables)
								{
									command = command.Replace("<var:" + variable.Key + ">", variable.Value);
								}
								plugin.Debug("Running rolesync command: " + command);
								plugin.sync.ScheduleRoleSyncCommand(command);
							}

							plugin.Verbose("Synced " + player.Nickname + " (" + userInfo.SteamIDOrIP + ") with Discord role id " + keyValuePair.Key);
							return;
						}
					}
				}
			}
			catch (InvalidOperationException)
			{
				plugin.Warn("Tried to run commands on a player who is not on the server anymore.");
			}
		}

		public EmbedMessage AddPlayer(SyncRoleCommand command)
		{
			if (CharacterClassManager.OnlineMode)
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

				string response = "";
				if (!CheckSteamAccount(command.SteamIDOrIP, ref response))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = command.ChannelID,
						Description = response,
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

		private bool CheckSteamAccount(string steamID, ref string response)
		{
			ServicePointManager.ServerCertificateValidationCallback = SSLValidation;
			HttpWebResponse webResponse = null;
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/profiles/" + steamID + "?xml=1");
				request.Method = "GET";

				webResponse = (HttpWebResponse)request.GetResponse();

				string xmlResponse = new StreamReader(webResponse.GetResponseStream() ?? new MemoryStream()).ReadToEnd();

				string[] foundStrings = xmlResponse.Split('\n').Where(w => w.Contains("steamID64")).ToArray();

				if (foundStrings.Length == 0)
				{
					response = "SteamID does not seem to exist.";
					plugin.Debug(response);
					return false;
				}
				response = "SteamID found.";
				return true;

			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					webResponse = (HttpWebResponse)e.Response;
					response = "Error occured connecting to steam services.";
					plugin.Error("Steam profile connection error: " + webResponse.StatusCode);
				}
				else
				{
					response = "Error occured connecting to steam services.";
					plugin.Error("Steam profile connection error: " + e.Status.ToString());
				}
			}
			finally
			{
				webResponse?.Close();
			}
			return false;
		}

		private bool SSLValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{

			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}

			// If there are errors in the certificate chain,
			// look at each error to determine the cause.
			foreach (X509ChainStatus element in chain.ChainStatus)
			{
				if (element.Status == X509ChainStatusFlags.RevocationStatusUnknown)
				{
					continue;
				}

				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
				chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
				chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

				// If chain is not valid
				if (!chain.Build((X509Certificate2)certificate))
				{
					return false;
				}
			}
			return true;
		}

		public EmbedMessage RemovePlayer(UnsyncRoleCommand command)
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

		public string RemovePlayerLocally(ulong discordID)
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
	}
}
