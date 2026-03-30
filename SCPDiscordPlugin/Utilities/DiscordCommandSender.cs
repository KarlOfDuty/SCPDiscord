namespace SCPDiscordPlugin.Utilities;

public class DiscordCommandSender : CommandSender
{
  public override string SenderId => $"{DiscordUserID}@discord";

  public override string Nickname => DiscordUsername;

  public string DiscordUsername { get; set; }
  public ulong DiscordUserID { get; set; }

  public override ulong Permissions => 0;

  public override byte KickPower => 0;

  public override bool FullPermissions => false;

  public DiscordCommandSender(ulong discordUserID, string discordUsername)
  {
    DiscordUserID = discordUserID;
    DiscordUsername = discordUsername;
  }

  public override bool Available()
  {
    return true;
  }

  public override void Print(string text) { /* ignored */ }

  public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay) { /* ignored */ }
}