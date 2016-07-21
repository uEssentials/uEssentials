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

using System.IO;
using Steamworks;
using System;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "resetplayer",
        Description = "Reset all player data.",
        Usage = "[player/playerid]",
        MinArgs = 1,
        MaxArgs = 1
    )]
    public class CommandResetPlayer : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty || args.Length > 1) {
                return CommandResult.ShowUsage();
            }

            try {
                var steamId = new CSteamID(ulong.Parse(args[0].ToString()));

                if (!steamId.IsValid()) {
                    return CommandResult.Lang("INVALID_STEAMID", steamId.m_SteamID);
                }

                ResetPlayer(steamId.m_SteamID);
                EssLang.Send(src, "PLAYER_RESET");
            } catch (FormatException) {
                var target = args[0].ToPlayer;

                if (target == null) {
                    return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                }

                target.Kick(EssLang.Translate("PLAYER_RESET_KICK"));
                ResetPlayer(target.CSteamId.m_SteamID);

                EssLang.Send(src, "PLAYER_RESET");
            }

            return CommandResult.Success();
        }

        private void ResetPlayer(ulong steamId) {
            var sep = Path.DirectorySeparatorChar.ToString();
            var idStr = steamId.ToString();
            var parentDir = Directory.GetParent(Directory.GetCurrentDirectory());

            Directory.GetDirectories(parentDir + $"{sep}Players{sep}")
                .Where(dic => dic.Substring(dic.LastIndexOf(sep, StringComparison.Ordinal) + 1).StartsWith(idStr))
                .ForEach(dic => Directory.Delete(dic, true));
        }

    }

}