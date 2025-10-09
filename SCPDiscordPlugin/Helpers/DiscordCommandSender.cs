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

		public DiscordCommandSender(ulong DiscordUserId, string DiscordNickname) : base()
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
		}

		public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
		{
		}
	}
}
