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
using System.Collections.Generic;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Common;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Player;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "kickall",
        "Kick all players",
        Syntax= "<reason>"
    )]
    public class CommandKickAll : EssCommand
    {
        public CommandKickAll(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            var playerManager = context.Container.Resolve<IPlayerManager>();
            var players = playerManager.OnlinePlayers
                .Select(c => c as UnturnedPlayer)
                .Where(c => c != null)
                .ToList();

            if (players.Count == 0)
            {
                throw new CommandWrongUsageException(Translations.Get("NO_PLAYERS_FOR_KICK"));
            }

            var reason = context.Parameters.Length == 0
                ? Translations.Get("KICK_NO_SPECIFIED_REASON")
                : context.Parameters.GetArgumentLine(0);

            players.ForEach(player => { player.Kick(context.User, reason); });

            context.User.SendLocalizedMessage(Translations, "KICKED_ALL", players.Count);
        }
    }
}