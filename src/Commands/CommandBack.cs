#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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

using System;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using UnityEngine;
using Essentials.I18n;
using Essentials.Api;
using Essentials.Common.Util;
using Essentials.Event.Handling;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "back",
        Aliases = new[] { "ret" },
        Description = "Return to position of your death.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandBack : EssCommand {

        internal const string META_KEY_DELAY = "back_delay";
        internal const string META_KEY_POS = "back_pos";

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();
            var playerMeta = player.Metadata;

            if (!playerMeta.Has(META_KEY_POS) || !playerMeta.Has(META_KEY_DELAY)) {
                return CommandResult.LangError("NOT_DIED_YET");
            }

            var deathTime = playerMeta.Get<DateTime>(META_KEY_DELAY);
            var delta = UEssentials.Config.BackDelay - (DateTime.Now - deathTime).Seconds;

            if (delta > 0 && !player.HasPermission($"essentials.bypass.backdelay")) {
                return CommandResult.LangError("BACK_DELAY", TimeUtil.FormatSeconds((uint) delta));
            }

            var backPosition = playerMeta.Get<Vector3>(META_KEY_POS);
            src.ToPlayer().Teleport(backPosition);
            EssLang.Send(src, "RETURNED");

            playerMeta.Remove(META_KEY_DELAY);
            playerMeta.Remove(META_KEY_POS);

            return CommandResult.Success();
        }

        protected override void OnUnregistered()
            => UEssentials.EventManager.Unregister<EssentialsEventHandler>("BackPlayerDeath");
    }

}