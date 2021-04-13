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
        Name = "jump",
        Description = "Teleport to a position that you are looking towards.",
        Usage = "<max_distance>",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandJump : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();
            var dist = 1000f;

            if (args.Length == 1) {
                if (!args[0].IsDouble) {
                    return CommandResult.ShowUsage();
                }

                dist = (float) args[0].ToDouble;
            }

            var eyePos = player.GetEyePosition(dist);

            if (!eyePos.HasValue) {
                return CommandResult.LangError("JUMP_NO_POSITION");
            }

            var point = eyePos.Value;
            point.y += 6;
            // fix
            player.UnturnedPlayer.teleportToLocationUnsafe(point, player.Rotation);
            EssLang.Send(src, "JUMPED", point.x, point.y, point.z);

            return CommandResult.Success();
        }

    }

}