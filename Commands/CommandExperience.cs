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
        "experience",
        "Give experience to you/player",
        Aliases = new[] { "exp" },
        Syntax = "[amount] <target/*>"
    )]
    public class CommandExperience : EssCommand
    {
        public CommandExperience(IPlugin plugin) : base(plugin)
        {
        }

        private const int MAX_INPUT_VALUE = 10000000;

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if ((context.Parameters.Length < 2 && !(context.User is UnturnedUser)))
                throw new CommandWrongUsageException();

            var amount = context.Parameters.Get<int>(0);

            if (amount > MAX_INPUT_VALUE || amount < -MAX_INPUT_VALUE)
            {
                context.User.SendLocalizedMessage(Translations, "NUMBER_BETWEEN", -MAX_INPUT_VALUE, MAX_INPUT_VALUE);
                return;
            }

            if (context.Parameters.Length < 2)
            {
                GiveExp(((UnturnedUser) context.User).Player, amount);
                return;
            }

            if (context.Parameters[0].Equals("*"))
            {
                if (context.User.CheckPermission("Experience.all") != PermissionResult.Grant)
                    throw new NotEnoughPermissionsException(context.User, "Experience.all");

                var playerManager = context.Container.Resolve<IPlayerManager>();

                playerManager.OnlinePlayers
                    .Select(c => c as UnturnedPlayer)
                    .Where(c => c != null)
                    .ForEach(p => GiveExp(p, amount));

                if (amount >= 0)
                    context.User.SendLocalizedMessage(Translations, "EXPERIENCE_GIVEN", amount,
                        Translations.Get("EVERYONE"));
                else
                    context.User.SendLocalizedMessage(Translations, "EXPERIENCE_TAKE", -amount,
                        Translations.Get("EVERYONE"));
                return;
            }

            // Other player
            if (context.User.CheckPermission("Experience.other") != PermissionResult.Grant)
                throw new NotEnoughPermissionsException(context.User, "Experience.other");

            if (!(context.Parameters.Get<IPlayer>(1) is UnturnedPlayer player))
                throw new PlayerNameNotFoundException(context.Parameters[1]);

            if (amount >= 0)
                context.User.SendLocalizedMessage(Translations, "EXPERIENCE_GIVEN", amount, player.DisplayName);
            else
                context.User.SendLocalizedMessage(Translations, "EXPERIENCE_TAKE", -amount, player.DisplayName);

            GiveExp(player, amount);
        }

        private void GiveExp(UnturnedPlayer player, int amount)
        {
            var playerExp = player.NativePlayer.skills.experience;

            if (amount < 0)
            {
                if (playerExp - amount < 0)
                    playerExp = 0;
                else
                    playerExp += (uint)amount;
            }
            else
            {
                if (playerExp + amount > int.MaxValue)
                    playerExp = int.MaxValue;
                else
                    playerExp += (uint)amount;
            }

            if (amount >= 0)
                player.User?.SendLocalizedMessage(Translations, "EXPERIENCE_RECEIVED", amount);
            else
                player.User?.SendLocalizedMessage(Translations, "EXPERIENCE_LOST", -amount);

            player.Entity.Experience = playerExp;
        }
    }
}