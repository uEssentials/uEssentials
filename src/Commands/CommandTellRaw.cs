using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;

namespace Essentials.Commands
{ 
    [CommandInfo(
        Name = "tellraw",
        Description = "Send raw message to a player.",
        Usage = "[player/*console*] [message]",
        MinArgs = 2
    )]
    public class CommandTellRaw : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var msg = args.Length == 2 ? args[1].ToString() : args.Join(1);
            var color = ColorUtil.GetColorFromString(ref msg);

            if (args[0].Equals("*console*"))
            {
                UEssentials.ConsoleSource.SendMessage(msg, color);
            }
            else
            {
                if (!args[0].IsValidPlayerIdentifier)
                {
                    return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                }
                args[0].ToPlayer.SendMessage(msg, color);
            }

            return CommandResult.Success();
        }

    }

}