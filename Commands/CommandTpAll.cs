﻿#region License
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
        Description = "Teleport all players to a player/position",
        Usage = "[player/x y z]"
    )]
    public class CommandTpAll : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var players = UServer.Players.ToList();

            if (players.Count == (src.IsConsole ? 0 : 1)) {
                return CommandResult.LangError("NO_PLAYERS_FOR_TELEPORT");
            }

            switch (args.Length) {
                case 0:
                    if (src.IsConsole) {
                        return CommandResult.ShowUsage();
                    }

                    TeleportAll(src.ToPlayer().Position, players);
                    EssLang.Send(src, "TELEPORTED_ALL_YOU");
                    break;

                case 1:
                    if (!UPlayer.TryGet(args[0].ToString(), out var player)) {
                        return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                    }

                    TeleportAll(player.Position, players);
                    EssLang.Send(src, "TELEPORTED_ALL_PLAYER", player.DisplayName);
                    break;

                case 3:
                    var vec3 = args.GetVector3(0);

                    if (!vec3.HasValue) {
                        return CommandResult.LangError("INVALID_COORDS", src, args[0], args[1], args[2]);
                    }

                    var pos = vec3.Value;

                    TeleportAll(pos, players);
                    EssLang.Send(src, "TELEPORTED_ALL_COORDS", pos.x);
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

        private void TeleportAll(Vector3 pos, List<UPlayer> players) {
            players.ForEach(player => player.UnturnedPlayer.sendTeleport(pos, 0));
        }

    }

}