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
using Essentials.I18n;

namespace Essentials.NativeModules.Warp.Commands {

    [CommandInfo(
        Name = "warps",
        Description = "View available warps"
    )]
    public class CommandWarps : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var warps = (
                from warp in WarpModule.Instance.WarpManager.Warps
                where warp.CanBeUsedBy(src)
                select warp.Name
            ).ToArray();

            if (warps.Length == 0) {
                EssLang.Send(src, "WARP_NONE");
            } else {
                EssLang.Send(src, "WARP_LIST", string.Join(", ", warps));
            }

            return CommandResult.Success();
        }

    }

}