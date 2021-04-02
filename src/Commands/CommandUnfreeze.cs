#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Components.Player;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "unfreeze",
        Usage = "[player/*]",
        Description = "Unfreeze a player/everyone",
        MinArgs = 1,
        MaxArgs = 1
    )]
    public class CommandUnfreeze : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args[0].Equals("*")) {
                foreach (var player in UServer.Players.Where(player => player.HasComponent<FrozenPlayer>())) {
                    player.RemoveComponent<FrozenPlayer>();
                    EssLang.Send(player, "UNFROZEN_PLAYER", src.DisplayName);
                }

                EssLang.Send(src, "UNFROZEN_ALL");
            } else if (args[0].IsValidPlayerIdentifier) {
                var target = args[0].ToPlayer;

                if (!target.HasComponent<FrozenPlayer>()) {
                    return CommandResult.LangError("NOT_FROZEN", target.DisplayName);
                }
                // Add movement again
                target.Movement.sendPluginSpeedMultiplier(1);
                target.RemoveComponent<FrozenPlayer>();

                EssLang.Send(src, "UNFROZEN_SENDER", target.DisplayName);
                EssLang.Send(target, "UNFROZEN_PLAYER", src.DisplayName);
            } else {
                return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
            }
            return CommandResult.Success();
        }

    }

}