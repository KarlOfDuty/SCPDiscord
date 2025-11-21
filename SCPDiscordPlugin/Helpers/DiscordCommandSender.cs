namespace SCPDiscordPlugin.Helpers
{
	public class DiscordCommandSender : CommandSender
	{
		public override string SenderId => $"{this.DiscordUserID}@discord";

		public override string Nickname => this.DiscordUsername;

		public string DiscordUsername { get; set; } = "UnknownUser";
		public ulong DiscordUserID { get; set; } = 0;

		public override ulong Permissions => 0;

		public override byte KickPower => 0;

		public override bool FullPermissions => false;

		public DiscordCommandSender(ulong DiscordUserId, string DiscordNickname)
		{
			DiscordUserID = DiscordUserId;
			DiscordUsername = DiscordNickname;
		}

		public override bool Available()
		{
			return true;
		}

		public override void Print(string text)
		{
			// this is a mocked implementation, we don't need to log in server console	
		}

		public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
		{
			// this mocked implementation, it is not a real player, so nothing to do with RaReply
		}
	}
}
