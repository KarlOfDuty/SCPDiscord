using System;
using CommandSystem;

namespace SCPDiscord.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SCPDiscordCommand : ParentCommand
    {
        public SCPDiscordCommand() => LoadGeneratedCommands();
        public override string Command { get; } = "scpdiscord";
        public override string[] Aliases { get; } = new string[] { "scpd" };
        public override string Description { get; } = "Main command for SCPDiscord.";

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
            RegisterCommand(new VerboseCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"[SCPDiscord]\nUsage: {arguments.Array[0]} [Command]\n";
            foreach (ICommand command in AllCommands)
            {
                response = string.Concat(new string[]
                {
                    response,
                    "\n",
                    "Command name: " + command.Command,
                    ", ",
                    "description: " + command.Description,
                });
                if (command.Aliases != null && command.Aliases.Length != 0)
                {
                    response = response + ", " + "aliases: " + string.Join(", ", command.Aliases);
                }
            }
            return false;
        }
    }
}