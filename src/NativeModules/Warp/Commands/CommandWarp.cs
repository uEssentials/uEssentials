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
using Essentials.Event.Handling;
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.NativeModules.Warp.Commands {

    [CommandInfo(
        Name = "warp",
        Description = "Teleport you to given warp.",
        AllowedSource = AllowedSource.PLAYER,
        Usage = "[warp_name]"
    )]
    public class CommandWarp : EssCommand {

        internal static PlayerDictionary<Tasks.Task> Delay = new PlayerDictionary<Tasks.Task>(
            PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS |
            PlayerDictionaryOptions.REMOVE_ON_DEATH |
            PlayerDictionaryOptions.REMOVE_ON_DISCONNECT,
            task => task.Cancel()
        );

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();

            if (args.Length == 0 || args.Length > 1) {
                return CommandResult.ShowUsage();
            }

            if (!WarpModule.Instance.WarpManager.Contains(args[0].ToString())) {
                return CommandResult.Lang("WARP_NOT_EXIST", args[0]);
            }

            if (!player.HasPermission($"essentials.warp.{args[0]}")) {
                return CommandResult.Lang("WARP_NO_PERMISSION", args[0]);
            }

            if (player.RocketPlayer.Stance == EPlayerStance.DRIVING ||
                player.RocketPlayer.Stance == EPlayerStance.SITTING) {
                return CommandResult.Lang("CANNOT_TELEPORT_DRIVING");
            }

            var dest = WarpModule.Instance.WarpManager.GetByName(args[0].ToString());
            var cooldown = UEssentials.Config.WarpCommand.Cooldown;

            if (cooldown > 0 && !player.HasPermission("essentials.bypass.warpcooldown")) {
                EssLang.Send(src, "WARP_COOLDOWN", cooldown);
            }

            var task = Tasks.New(t => {
                Delay.Remove(player.CSteamId.m_SteamID);
                player.Teleport(dest.Location, dest.Rotation);
                EssLang.Send(src, "WARP_TELEPORTED", args[0]);
            });

            task.Delay(player.HasPermission("essentials.bypass.warpcooldown") ? 0 : cooldown*1000).Go();
            Delay.Add(player.CSteamId.m_SteamID, task);

            return CommandResult.Success();
        }

        protected override void OnUnregistered() {
            Delay.Clear();
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("WarpPlayerMove");
        }

    }

}