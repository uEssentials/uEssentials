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
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Components.Player;
using Essentials.Event.Handling;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "freeze",
        Usage = "[player/*]",
        Description = "Freeze a player/everyone",
        MinArgs = 1,
        MaxArgs = 1
    )]
    public class CommandFreeze : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args[0].Equals("*")) {
                UServer.Players
                    .Where(player => !player.HasComponent<FrozenPlayer>())
                    .ForEach(player => {
                        player.AddComponent<FrozenPlayer>();
                        EssLang.Send(player, "FROZEN_PLAYER", src.DisplayName);
                        // Better
                        player.Movement.sendPluginSpeedMultiplier(0);
                    });

                EssLang.Send(src, "FROZEN_ALL");
            } else {
                if (!UPlayer.TryGet(args[0].ToString(), out var player)) {
                    return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                }

                if (player.HasComponent<FrozenPlayer>()) {
                    EssLang.Send(src, "ALREADY_FROZEN", player.DisplayName);
                } else {
                    player.AddComponent<FrozenPlayer>();

                    EssLang.Send(src, "FROZEN_SENDER", player.DisplayName);
                    EssLang.Send(player, "FROZEN_PLAYER", src.DisplayName);
                    // Better
                    player.Movement.sendPluginSpeedMultiplier(0);
                }
            }

            return CommandResult.Success();
        }

        protected override void OnUnregistered() {
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("FreezePlayerDisconnect");
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("FreezePlayerConnected");
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("FreezePlayerDeath");
        }

    }

}