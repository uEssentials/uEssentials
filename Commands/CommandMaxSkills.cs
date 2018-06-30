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
using Rocket.API.Commands;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Migration.LegacyPermissions;
using Rocket.Core.Permissions;
using Rocket.Core.User;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "maxskills",
        "Set to max level all of your/player skills",
        Syntax = "<overpower[true|false]> <player | *>"
    )]
    public class CommandMaxSkills : EssCommand
    {
        public CommandMaxSkills(IPlugin plugin) : base(plugin)
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
                if (!(context.User is UnturnedUser))
                {
                    throw new CommandWrongUsageException();
                }

                GiveMaxSkills(((UnturnedUser)context.User).Player, false);
            }
            else
            {
                if (context.Parameters.Length < 2 && !(context.User is UnturnedUser))
                {
                    throw new CommandWrongUsageException();
                }

                bool overpower = context.Parameters.Get<bool>(0);

                if (overpower && context.User.CheckPermission($"MaxSkills.overpower") != PermissionResult.Grant)
                {
                    throw new NotEnoughPermissionsException(context.User, "MaxSkills.overpower");
                }

                // player or all
                if (context.Parameters.Length > 1)
                {
                    if (context.Parameters[0].Equals("*"))
                    {
                        if (context.User.CheckPermission($"MaxSkills.all") != PermissionResult.Grant)
                        {
                            throw new NotEnoughPermissionsException(context.User, "MaxSkills.all");
                        }

                        var playerManager = context.Container.Resolve<IPlayerManager>();
                        playerManager.OnlinePlayers.Select(c => c as UnturnedPlayer)
                            .Where(c => c != null)
                            .ForEach(p => GiveMaxSkills(p, overpower));

                        context.User.SendLocalizedMessage(Translations, "MAX_SKILLS_ALL");
                    }
                    else
                    {
                        if (context.User.CheckPermission($"MaxSkills.other") != PermissionResult.Grant)
                        {
                            throw new NotEnoughPermissionsException(context.User, "MaxSkills.other");
                        }

                        if(!(context.Parameters.Get<IPlayer>(1) is UnturnedPlayer targetPlayer))
                            throw new PlayerNotOnlineException(context.Parameters[1]);

                        GiveMaxSkills(targetPlayer, overpower);
                        context.User.SendLocalizedMessage(Translations, "MAX_SKILLS_TARGET", targetPlayer.DisplayName);
                    }
                }
                else
                {
                    // self (with overpower)
                    GiveMaxSkills(((UnturnedUser)context.User).Player, overpower);
                }
            }
        }

        private void GiveMaxSkills(UnturnedPlayer player, bool overpower)
        {
            var pSkills = player.NativePlayer.skills;

            foreach (var skill in pSkills.skills.SelectMany(skArr => skArr))
            {
                skill.level = overpower ? byte.MaxValue : skill.max;
            }

            pSkills.askSkills(player.CSteamID);
            player.User.SendLocalizedMessage(Translations, "MAX_SKILLS");
        }
    }
}