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
using static Essentials.Commands.CommandTell;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "reply",
        Aliases = new[] { "r" },
        Description = "Reply to the most recent private message",
        Usage = "[message]",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 1
    )]
    public class CommandReply : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var playerId = src.ToPlayer().CSteamId.m_SteamID;

            if (!ReplyTo.TryGetValue(playerId, out var targetId)) {
                return CommandResult.LangError("NOBODY_TO_REPLY");
            }

            var target = UPlayer.From(targetId);

            if (target == null) {
                return CommandResult.LangError("NO_LONGER_ONLINE");
            }

            src.DispatchCommand($"tell \"{target.DisplayName}\" \"{args.Join(0)}\"");

            return CommandResult.Success();
        }

    }

}