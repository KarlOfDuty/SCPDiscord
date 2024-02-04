using System;
using System.Linq;
using CommandSystem;

namespace SCPDiscord.Commands
{
    public interface SCPDiscordCommand : ICommand
    {
        string[] ArgumentList { get; }
    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class RootCommand : ParentCommand
    {
        public RootCommand() => LoadGeneratedCommands();
        public override string Command { get; } = "scpdiscord";
        public override string[] Aliases { get; } = { "scpd" };
        public override string Description { get; } = "Root command for SCPDiscord.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new DebugCommand());
            RegisterCommand(new GrantReservedSlotCommand());
            RegisterCommand(new GrantVanillaRankCommand());
            RegisterCommand(new ReconnectCommand());
            RegisterCommand(new ReloadCommand());
            RegisterCommand(new RemoveReservedSlotCommand());
            RegisterCommand(new SetNickname());
            RegisterCommand(new UnsyncCommand());
            RegisterCommand(new ValidateCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Usage: " + arguments.At(0) + " [COMMAND]";
            foreach (SCPDiscordCommand command in AllCommands.OfType<SCPDiscordCommand>())
            {
                string line = "\n" + command.Command;
                if (command.Aliases != null && command.Aliases.Length != 0)
                {
                    line += "/" + string.Join("/", command.Aliases);
                }

                if (command.ArgumentList != null && command.ArgumentList.Length != 0)
                {
                    line += " " + string.Join(" ", command.ArgumentList);
                }

                response += line.PadRight(50, '.') + command.Description;
            }
            return false;
        }
    }
}