using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord.Commands
{
  public class AdminChatCommand
  {
    [RequireGuild]
    [Command("achat")]
    [Description("Allows you to talk to staff on server through adminchat system")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("message")] [Description("Message you want to send")] string text)
    {
      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        AdminChatCommand = new Interface.AdminChatCommand
        {
          ChannelID = command.Channel.Id,
          Message = text,
          InteractionID = command.Interaction.Id,
          DiscordUsername = command.User.Username,
          DiscordUserID = command.User.Id
        }
      };

      MessageScheduler.CacheInteraction(command);
      await NetworkSystem.SendMessage(message, command);
    }
  }
}
