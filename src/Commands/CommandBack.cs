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
using UnityEngine;
using Essentials.I18n;
using Essentials.Api;
using Essentials.Core;
using Essentials.Event.Handling;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "back",
        Aliases = new[] { "ret" },
        Description = "Return to position of your death.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandBack : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();

            if (!player.HasMetadata(Consts.BACK_METADATA_KEY)) {
                return CommandResult.Lang("NOT_DIED_YET");
            }

            var backPosition = player.GetMetadata<Vector3>(Consts.BACK_METADATA_KEY);
            src.ToPlayer().Teleport(backPosition);
            EssLang.Send(src, "RETURNED");

            return CommandResult.Success();
        }

        protected override void OnUnregistered()
            => UEssentials.EventManager.Unregister<EssentialsEventHandler>("BackPlayerDeath");

    }

}