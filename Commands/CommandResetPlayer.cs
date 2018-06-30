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

using System.IO;
using System;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Common;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.API.User;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.User;

namespace Essentials.Commands
{
    [CommandInfo(
        "resetplayer",
        "Reset all player data.",
        Syntax = "[player/playerid]"
    )]
    public class CommandResetPlayer : EssCommand
    {
        public CommandResetPlayer(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
            {
                throw new CommandWrongUsageException();
            }

            var id = context.Parameters[0];
            var userManager = context.Container.Resolve<IUserManager>();

            foreach (var user in userManager.OnlineUsers.Where(c => c.Id == id))
                user.Kick(context.User, Translations.Get("PLAYER_RESET_KICK"));

            ResetPlayer(id);
            context.User.SendLocalizedMessage(Translations, "PLAYER_RESET");
        }

        private void ResetPlayer(string userId)
        {
            var sep = Path.DirectorySeparatorChar.ToString();
            var parentDir = Directory.GetParent(Directory.GetCurrentDirectory());

            Directory.GetDirectories(parentDir + $"{sep}Players{sep}")
                .Where(dic => dic.Substring(dic.LastIndexOf(sep, StringComparison.Ordinal) + 1).StartsWith(userId))
                .ForEach(dic => Directory.Delete(dic, true));
        }
    }
}