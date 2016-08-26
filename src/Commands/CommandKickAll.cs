#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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

using System.Collections.Generic;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "kickall",
        Description = "Kick all players",
        Usage = "<reason>"
    )]
    public class CommandKickAll : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var players = new List<UPlayer>(UServer.Players);

            if (players.Count == 0) {
                return CommandResult.Lang("NO_PLAYERS_FOR_KICK");
            }

            var reason = args.IsEmpty
                ? EssLang.Translate("KICK_NO_SPECIFIED_REASON")
                : args.Join(0);

            players.ForEach(player => {
                player.Kick(reason);
            });

            EssLang.Send(src, "KICKED_ALL", players.Count);

            return CommandResult.Success();
        }

    }

}