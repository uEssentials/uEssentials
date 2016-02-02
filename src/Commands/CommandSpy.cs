using System.Collections.Generic;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "spy",
        Description = "Toggle spy mode",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandSpy : EssCommand
    {
        public static readonly List<string> Spies = new List<string>();

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            var displayName = source.DisplayName;

            if ( Spies.Contains( displayName ) )
            {
                Spies.Remove( displayName );
                EssLang.SPY_MODE_OFF.SendTo( source );
            }
            else
            {
                Spies.Add( displayName );
                EssLang.SPY_MODE_ON.SendTo( source );
            }
        }
    }
}