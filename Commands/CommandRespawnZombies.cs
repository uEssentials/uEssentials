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
using System.Linq;
using Essentials.Api.Command;
using Essentials.Common;
using SDG.Unturned;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.Core.I18N;
using Object = UnityEngine.Object;

namespace Essentials.Commands
{
    [CommandInfo(
        "respawnzombies",
        "Respawn all zombies"
    )]
    public class CommandRespawnZombies : EssCommand
    {
        public CommandRespawnZombies(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            var count = 0;

            var zombies = Object.FindObjectsOfType<Zombie>();

            zombies.Where(z => z.isDead).ForEach(zombie =>
            {
                ZombieManager.sendZombieAlive(
                    zombie,
                    zombie.type,
                    (byte) zombie.speciality,
                    zombie.shirt,
                    zombie.pants,
                    zombie.hat,
                    zombie.gear,
                    zombie.transform.position,
                    0
                );
                count++;
            });

            context.User.SendLocalizedMessage(Translations, "RESPAWNED_ZOMBIES", count);
        }
    }
}