using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandSystem;

// TODO: Remove this command when removing the old rolesync compatibility
namespace SCPDiscord.Commands
{
  public class RemoveReservedSlotCommand : SCPDiscordCommand
  {
    public string Command { get; } = "removereservedslot";
    public string[] Aliases { get; } = { "rrs" };
    public string Description { get; } = "Removes a player from the reserved slots list.";
    public bool SanitizeResponse { get; } = true;
    public string[] ArgumentList { get; } = { "<steamid>" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      Logger.Warn(sender.LogName + " used the removereservedslot command. Note: this command will be removed in the future!");

      if (arguments.Count < 1 || arguments.At(0).Length < 10)
      {
        response = "Invalid arguments.";
        return false;
      }

      bool found = false;
      string steamID = arguments.At(0);
      List<string> reservedSlotsFileRows = File.ReadAllLines(Config.GetReservedSlotPath()).ToList();
      for (int i = 0; i < reservedSlotsFileRows.Count; ++i)
      {
        if (reservedSlotsFileRows[i].Trim().StartsWith(steamID))
        {
          found = true;
          reservedSlotsFileRows.RemoveAt(i);
          --i;

          // Remove SCPDiscord comment if there is one
          if (i >= 0 && reservedSlotsFileRows[i].Trim().StartsWith("# SCPDiscord"))
          {
            reservedSlotsFileRows.RemoveAt(i);
            --i;
          }
        }
      }

      if (found)
      {
        File.WriteAllLines(Config.GetReservedSlotPath(), reservedSlotsFileRows);
        response = "Reserved slot removed.";
        return true;
      }
      else
      {
        response = "Could not find a reserved slot with that Steam ID.";
        return false;
      }
    }
  }
}