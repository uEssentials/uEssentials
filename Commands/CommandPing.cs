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
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "ping",
        "View your/player ping",
        Syntax = "[player]"
    )]
    public class CommandPing : EssCommand
    {
        public CommandPing(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if (context.Parameters.Length < 1)
            {
                if (!(context.User is UnturnedUser))
                    throw new CommandWrongUsageException();

                context.User.SendLocalizedMessage(Translations, "PING", ((UnturnedUser)context.User).Player.Ping);
            }
            else
            {
                if (!(context.Parameters.Get<IPlayer>(0) is UnturnedPlayer targetPlayer))
                    throw new PlayerNotOnlineException(context.Parameters[0]);

                context.User.SendLocalizedMessage(Translations, "PING_OTHER", targetPlayer.DisplayName, targetPlayer.Ping);
            }
        }
    }
}