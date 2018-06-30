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
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "jump",
        "Teleport to a position that you are looking towards.",
        Syntax = "<max_distance>"
    )]
    public class CommandJump : EssCommand
    {
        public CommandJump(IPlugin plugin) : base(plugin)
        {
        }
        public override bool SupportsUser(Type user)
        {
            return typeof(UnturnedUser).IsAssignableFrom(user);
        }

        public override void Execute(ICommandContext context)
        {
            var player = ((UnturnedUser)context.User).Player;
            var dist = 1000f;

            if (context.Parameters.Length == 1)
            {
                dist = context.Parameters.Get<float>(0);
            }

            var eyePos = player.GetEyePosition(dist);

            if (!eyePos.HasValue)
            {
                throw new CommandWrongUsageException(Translations.Get("JUMP_NO_POSITION"));
            }

            var point = eyePos.Value;
            point.y += 6;

            player.Entity.Teleport(point);
            context.User.SendLocalizedMessage(Translations, "JUMPED", new object[] { point.x, point.y, point.z });
        }
    }
}