using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands;

public class MuteCommand : ApplicationCommandModule
{
    [SlashRequireGuild]
    [SlashCommand("mute", "Mutes a player on the server")]
    public async Task OnExecute(InteractionContext command,
        [Option("SteamID", "Steam ID of the user to mute.")] string steamID,
        [Option("Duration", "Mute duration (ex: 2d is 2 days).")] string duration,
        [Option("Reason", "Reason for the mute.")] string reason)
    {
        await command.DeferAsync();
        Interface.MessageWrapper message = new Interface.MessageWrapper
        {
            MuteCommand = new Interface.MuteCommand
            {
                ChannelID = command.Channel.Id,
                SteamID = steamID,
                Duration = duration,
                AdminTag = "@" + command.Member?.Username,
                AdminID = command.Member.Id,
                Reason = reason,
                InteractionID = command.InteractionId
            }
        };

        MessageScheduler.CacheInteraction(command);
        await NetworkSystem.SendMessage(message, command);
        Logger.Debug("Sending MuteCommand to plugin from " + command.Member?.Username, LogID.DISCORD);
    }
}