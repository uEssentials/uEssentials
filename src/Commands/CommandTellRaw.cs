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

using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using UnityEngine;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "tellraw",
        Description = "Send raw message to an player.",
        Usage = "[player/*console*] [message]",
        MinArgs = 2
    )]
    public class CommandTellRaw : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var msg = args.Length == 2 ? args[1].ToString() : args.Join(1);
            var color = ColorUtil.HasColor(msg) ? ColorUtil.GetColorFromString(ref msg) : Color.green;

            if (args[0].Equals("*console*")) {
                UEssentials.ConsoleSource.SendMessage(msg, color);
            } else {
                if (!args[0].IsValidPlayerIdentifier) {
                    return CommandResult.Lang("PLAYER_NOT_FOUND", args[0]);
                }
                args[0].ToPlayer.SendMessage(msg, color);
            }

            return CommandResult.Success();
        }

    }

}