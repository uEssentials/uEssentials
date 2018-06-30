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
using Essentials.Components.Player;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "freeze",
        "Freeze a player/everyone",
        Syntax= "[player/*]"
    )]
    public class CommandFreeze : EssCommand
    {
        public CommandFreeze(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if(context.Parameters.Length < 1)
                throw new CommandWrongUsageException();

            if (context.Parameters[0].Equals("*"))
            {
                var playerManager = context.Container.Resolve<IPlayerManager>();

                playerManager.OnlinePlayers
                    .Select(c => c as UnturnedPlayer)
                    .Where(c => c != null)
                    .Where(player => !player.HasComponent<FrozenPlayer>())
                    .ForEach(player =>
                    {
                        player.AddComponent<FrozenPlayer>();
                        player.User.SendLocalizedMessage(Translations, "FROZEN_PLAYER", context.User.Name);
                    });

                context.User.SendLocalizedMessage(Translations, "FROZEN_ALL");
            }
            else
            {
                var player = context.Parameters.Get<IPlayer>(0) as UnturnedPlayer;
                if(player == null)
                    throw new PlayerNotFoundException(context.Parameters[0]);

                if (player.HasComponent<FrozenPlayer>())
                {
                    context.User.SendLocalizedMessage(Translations, "ALREADY_FROZEN", player.DisplayName);
                }
                else
                {
                    player.AddComponent<FrozenPlayer>();

                    context.User.SendLocalizedMessage(Translations, "FROZEN_SENDER", player.DisplayName);
                    player.User.SendLocalizedMessage(Translations, "FROZEN_PLAYER", context.User.Name);
                }
            }
        }
    }
}