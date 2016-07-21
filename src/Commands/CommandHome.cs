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
using Essentials.Api.Task;
using Essentials.Common.Util;
using Essentials.I18n;
using Essentials.Misc;
using UnityEngine;
using SDG.Unturned;
using Essentials.Event.Handling;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "home",
        Aliases = new[] { "h" },
        Description = "Teleport to your bed.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandHome : EssCommand {

        internal static SimpleCooldown Cooldown = new SimpleCooldown();

        internal static PlayerDictionary<Tasks.Task> Delay = new PlayerDictionary<Tasks.Task>(
            PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS |
            PlayerDictionaryOptions.REMOVE_ON_DEATH |
            PlayerDictionaryOptions.REMOVE_ON_DISCONNECT,
            task => task.Cancel()
        );

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();
            var playerId = player.CSteamId;

            if (Cooldown.HasEntry(playerId)) {
                return CommandResult.Lang("USE_COOLDOWN",
                    TimeUtil.FormatSeconds((uint) Cooldown.GetRemainingTime(playerId)));
            }

            Vector3 position;
            byte angle;

            if (player.RocketPlayer.Stance == EPlayerStance.DRIVING ||
                player.RocketPlayer.Stance == EPlayerStance.SITTING) {
                return CommandResult.Lang("CANNOT_TELEPORT_DRIVING");
            }

            if (!BarricadeManager.tryGetBed(player.CSteamId, out position, out angle)) {
                return CommandResult.Lang("WITHOUT_BED");
            }

            var homeCommand = UEssentials.Config.HomeCommand;
            var delay = homeCommand.Delay;
            var cooldown = homeCommand.Cooldown;

            if (player.HasPermission("essentials.bypass.homecooldown")) {
                delay = 0;
                cooldown = 0;
            }

            if (delay > 0) {
                EssLang.Send(src, "TELEPORT_DELAY", TimeUtil.FormatSeconds((uint) delay));
            }

            var task = Tasks.New(t => {
                Delay.Remove(playerId.m_SteamID);
                player.Teleport(position, angle);
                EssLang.Send(src, "TELEPORTED_BED");
            }).Delay(delay*1000);

            task.Go();

            Delay.Add(playerId.m_SteamID, task);
            Cooldown.AddEntry(playerId, cooldown);

            return CommandResult.Success();
        }

        protected override void OnUnregistered() {
          Cooldown.Clear();
          Delay.Clear();
          UEssentials.EventManager.Unregister<EssentialsEventHandler>("HomePlayerMove");
        }

    }

}