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

using System;
using Essentials.Api.Command;
using Essentials.Common.Util;
using SDG.Unturned;
using Rocket.API.Commands;
using Rocket.API.Permissions;
using Rocket.API.Plugins;
using Rocket.API.Scheduler;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Player;
using Rocket.Core.Scheduler;
using Rocket.UnityEngine.Extensions;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "home",
        "Teleport to your bed.",
        Aliases = new[] {"h"}
    )]
    public class CommandHome : EssCommand
    {
        internal static PlayerDictionary<ITask> Delay = new PlayerDictionary<ITask>(
            PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS |
            PlayerDictionaryOptions.REMOVE_ON_DEATH |
            PlayerDictionaryOptions.REMOVE_ON_DISCONNECT,
            task => task.Cancel()
        );

        public override bool SupportsUser(Type user)
        {
            return typeof(UnturnedUser).IsAssignableFrom(user);
        }

        public override void Execute(ICommandContext context)
        {
            var player = ((UnturnedUser)context.User).Player;
            var playerId = player.CSteamID;
            var scheduler = context.Container.Resolve<ITaskScheduler>();

            if (player.Entity.Stance == EPlayerStance.DRIVING ||
                player.Entity.Stance == EPlayerStance.SITTING)
            {
                throw new CommandWrongUsageException(Translations.Get("CANNOT_TELEPORT_DRIVING"));
            }

            if (!BarricadeManager.tryGetBed(player.CSteamID, out var bedPosition, out var bedAngle))
            {
                throw new CommandWrongUsageException(Translations.Get("WITHOUT_BED"));
            }

            if (Delay.ContainsKey(player.CSteamID.m_SteamID))
            {
                throw new CommandWrongUsageException(Translations.Get("ALREADY_WAITING"));
            }

            var homeCommand = UEssentials.Config.Home;
            var delay = homeCommand.TeleportDelay;

            if (player.CheckPermission("essentials.bypass.homecooldown") == PermissionResult.Grant)
            {
                delay = 0;
            }

            if (delay > 0)
            {
                context.User.SendLocalizedMessage(Translations, "TELEPORT_DELAY", TimeUtil.FormatSeconds((uint) delay));
            }

            var task = scheduler.ScheduleDelayed(UEssentials, () =>
            {
                Delay.Remove(playerId.m_SteamID);
                player.Entity.Teleport(bedPosition.ToSystemVector(), bedAngle);
                context.User.SendLocalizedMessage(Translations, "TELEPORTED_BED");
            }, "EssentialsHomeTeleport", TimeSpan.FromSeconds(delay));

            Delay.Add(playerId.m_SteamID, task);
        }

        public CommandHome(IPlugin plugin) : base(plugin)
        {
        }
    }
}