/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
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

using System;
using System.Collections.Generic;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using UnityEngine;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "tpall",
        Description = "Teleport all players to an player/position",
        Usage = "[player/position[x, y, z]]"
    )]
    public class CommandTpAll : EssCommand {

        private static readonly Action<Vector3, List<UPlayer>> TeleportAll = (pos, players) => {
            players.ForEach(player => player.UnturnedPlayer.sendTeleport(pos, 0));
        };

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var players = UServer.Players.ToList();

            if (players.Count == (src.IsConsole ? 0 : 1)) {
                return CommandResult.Lang("NO_PLAYERS_FOR_TELEPORT");
            }

            switch (args.Length) {
                case 0:
                    if (src.IsConsole) {
                        return CommandResult.ShowUsage();
                    }

                    TeleportAll(src.ToPlayer().RocketPlayer.Position, players);
                    EssLang.Send(src, "TELEPORTED_ALL_YOU");
                    break;

                case 1:
                    var found = UPlayer.TryGet(args[0], player => {
                        TeleportAll(player.Position, players);
                        EssLang.Send(src, "TELEPORTED_ALL_PLAYER", player.DisplayName);
                    });

                    if (!found) {
                        return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                    }
                    break;

                case 3:
                    try {
                        var x = (float) args[0].ToDouble;
                        var y = (float) args[1].ToDouble;
                        var z = (float) args[2].ToDouble;

                        var pos = new Vector3(x, y, z);

                        TeleportAll(pos, players);
                        EssLang.Send(src, "TELEPORTED_ALL_COORDS", x, y, z);
                    } catch (FormatException) {
                        return CommandResult.Lang("INVALID_COORDS",
                            src, args[0], args[1], args[2]);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

    }

}