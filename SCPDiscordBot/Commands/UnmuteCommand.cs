using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands;

public class UnmuteCommand : ApplicationCommandModule
{
    [SlashRequireGuild]
    [SlashCommand("unmute", "Unmutes a player from the server")]
    public async Task OnExecute(InteractionContext command, [Option("SteamID", "Steam ID of the user to unmute.")] string steamID)
    {
        await command.DeferAsync();
        Interface.MessageWrapper message = new Interface.MessageWrapper
        {
            MuteCommand = new Interface.MuteCommand
            {
                ChannelID = command.Channel.Id,
                SteamID = steamID,
                Duration = "0",
                AdminTag = command.Member?.Username,
                AdminID = command.Member.Id,
                Reason = "",
                InteractionID = command.InteractionId
            }
        };
        MessageScheduler.CacheInteraction(command);
        await NetworkSystem.SendMessage(message, command);
        Logger.Debug("Sending UnbanCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
    }
}