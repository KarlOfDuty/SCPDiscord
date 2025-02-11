using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace SCPDiscord.Commands
{
  public class GrantVanillaRankCommand : SCPDiscordCommand
  {
    public string Command { get; } = "grantvanillarank";
    public string[] Aliases { get; } = { "gvr" };
    public string Description { get; } = "Grants a player the vanilla rank provided.";
    public bool SanitizeResponse { get; } = true;
    public string[] ArgumentList { get; } = { "<playerid/steamid>", "<rank>" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      Logger.Debug(sender.LogName + " used the grantvanillarank command.");

      if (arguments.Count < 2)
      {
        response = "Invalid arguments.";
        return false;
      }

      string steamIDOrPlayerID = arguments.At(0).Replace("@steam", ""); // Remove steam suffix if there is one

      List<Player> matchingPlayers = new List<Player>();
      try
      {
        Logger.Debug("Looking for player with SteamID/PlayerID: " + steamIDOrPlayerID);
        foreach (Player pl in Player.List)
        {
          Logger.Debug("Player " + pl.PlayerId + ": SteamID " + pl.UserId + " PlayerID " + pl.PlayerId);
          if (pl.GetParsedUserID() == steamIDOrPlayerID)
          {
            Logger.Debug("Matching SteamID found");
            matchingPlayers.Add(pl);
          }
          else if (pl.PlayerId.ToString() == steamIDOrPlayerID)
          {
            Logger.Debug("Matching playerID found");
            matchingPlayers.Add(pl);
          }
        }
      }
      catch (Exception) { /* ignored */ }

      if (!matchingPlayers.Any())
      {
        response = "Player \"" + arguments.At(0) + "\"not found.";
        return false;
      }

      try
      {
        foreach (Player matchingPlayer in matchingPlayers)
        {
          matchingPlayer.SetRank(null, null, arguments.At(1));
        }
      }
      catch (Exception)
      {
        response = "Vanilla rank \"" + arguments.At(1) + "\" not found. Are you sure you are using the RA config role name and not the role title/badge?";
        return false;
      }

      response = "Player rank updated.";
      return true;
    }
  }
}