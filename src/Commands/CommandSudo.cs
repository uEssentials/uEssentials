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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using SDG.Unturned;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "sudo",
        Description = "Make player or console execute an command",
        Usage = "[player/*/*console*/]"
    )]
    public class CommandSudo : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.Length < 2) {
                return CommandResult.ShowUsage();
            }

            string name;

            if (args[0].Is(name = "*console*")) {
                CommandWindow.ConsoleInput.onInputText(args.Join(1));
            } else if (args[0].Is("*")) {
                UServer.Players.ForEach(p => {
                    ChatManager.Instance.askChat(p.CSteamId, (byte) EChatMode.GLOBAL, args.Join(1));
                });

                name = "Everyone";
            } else {
                if (!args[0].IsValidPlayerName) {
                    return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                }

                var targetPlayer = args[0].ToPlayer;

                ChatManager.Instance.askChat(targetPlayer.CSteamId, (byte) EChatMode.GLOBAL, args.Join(1));

                name = targetPlayer.CharacterName;
            }

            EssLang.Send(src, "SUDO_EXECUTED", name, args.Join(1));

            return CommandResult.Success();
        }

    }

}