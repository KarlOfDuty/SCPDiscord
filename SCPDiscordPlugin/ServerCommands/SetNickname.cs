using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace SCPDiscord.Commands
{
  public class SetNickname : SCPDiscordCommand
  {
    public string Command { get; } = "setnickname";
    public string[] Aliases { get; } = { "nick" };
    public string Description { get; } = "Sets a nickname for a player.";
    public bool SanitizeResponse { get; } = true;
    public string[] ArgumentList { get; } = { "<playerid/steamid>", "<nickname>" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      Logger.Debug(sender.LogName + " used the setnickname command.");

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

      foreach (Player matchingPlayer in matchingPlayers)
      {
        matchingPlayer.DisplayName = string.Join(" ", arguments.Skip(1));
      }

      response = "Player nickname updated.";
      return true;
    }
  }
}