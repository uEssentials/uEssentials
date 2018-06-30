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
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using static Essentials.Commands.CommandTell;

namespace Essentials.Commands
{
    [CommandInfo(
        "reply",
        "Reply to the most recent private message",
        Aliases = new[] { "r" },
        Syntax = "[message]"
    )]
    public class CommandReply : EssCommand
    {
        public CommandReply(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            var playerId = context.User.Id;

            if (!ReplyTo.TryGetValue(playerId, out var targetId))
            {
                throw new CommandWrongUsageException(Translations.Get("NOBODY_TO_REPLY"));
            }

            var playerManager = context.Container.Resolve<IPlayerManager>();
            var commandHandler = context.Container.Resolve<ICommandHandler>();

            var target = playerManager.GetOnlinePlayerById(targetId);

            if (target == null)
            {
                throw new CommandWrongUsageException("NO_LONGER_ONLINE");
            }

            commandHandler.HandleCommand(context.User, $"tell \"{target.Id}\" \"{context.Parameters.GetArgumentLine(0)}\"", context.CommandPrefix);
        }
    }
}