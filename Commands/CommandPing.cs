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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "ping",
        Description = "View your/player ping",
        Usage = "<player>"
    )]
    public class CommandPing : EssCommand {

        public override void Execute(ICommandContext context) {
            if (args.IsEmpty || args.Length > 1) {
                if (src.IsConsole) {
                    return CommandResult.ShowUsage();
                }

                EssLang.Send(src, "PING", src.ToPlayer().Ping);
            } else {
                var target = args[0].ToPlayer;

                if (target == null) {
                    return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                }

                EssLang.Send(src, "PING_OTHER", target.DisplayName, target.Ping);
            }

            return CommandResult.Success();
        }

    }

}