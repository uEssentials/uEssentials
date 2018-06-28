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
using SDG.Unturned;
using Essentials.Common;
using System.Linq;
using Steamworks;
using System.Collections.Generic;
using System.Numerics;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.Player;
using Rocket.Core.User;
using Rocket.UnityEngine.Extensions;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo("boom",
        "Create an explosion on player's/given position",
        Aliases = new[] { "explode" },
        Syntax = "[player | * | x, y, z]")]
    public class CommandBoom : EssCommand
    {
        public CommandBoom(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            var player = (context.User as UnturnedUser)?.GetPlayer();
            var playerMgr = context.Container.Resolve<IPlayerManager>();

            switch (context.Parameters.Length)
            {
                case 0:
                    if (!(context.User is UnturnedUser))
                        throw new CommandWrongUsageException();

                    var eyePos = player.GetEyePosition(3000);

                    if (eyePos.HasValue)
                    {
                        Explode(eyePos.Value);
                    }

                    break;

                case 1:
                    if (context.Parameters[0].Equals("*"))
                    {
                        playerMgr.OnlinePlayers.Where(p => p != context.User)
                            .ForEach(p => Explode(p.GetEntity().Position));
                    }
                    else
                    {
                        Explode(context.Parameters.Get<IPlayer>(0).GetEntity().Position);
                    }

                    break;

                case 3:
                    float x = context.Parameters.Get<float>(0);
                    float y = context.Parameters.Get<float>(1);
                    float z = context.Parameters.Get<float>(2);
                    Explode(new Vector3(x, y, z));
                    break;

                default:
                    throw new CommandWrongUsageException();
            }
        }

        private static void Explode(Vector3 pos)
        {
            const float DAMAGE = 200;

            EffectManager.sendEffect(20, EffectManager.INSANE, pos.ToUnityVector());
            DamageTool.explode(pos.ToUnityVector(), 10f, EDeathCause.GRENADE, CSteamID.Nil, DAMAGE, DAMAGE, DAMAGE,
                DAMAGE, DAMAGE,
                DAMAGE, DAMAGE, DAMAGE, out List<EPlayerKill> unused, EExplosionDamageType.CONVENTIONAL, 32, true,
                false);
        }
    }
}