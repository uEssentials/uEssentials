#region License

/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt and contributors
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
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "reputation",
        "Give reputation to you/player",
        Aliases = new[] {"rep"},
        Syntax = "[amount] <target/*>"
    )]
    public class CommandReputation : EssCommand
    {
        private const int MAX_INPUT_VALUE = 10000000;
        public CommandReputation(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0 || (context.Parameters.Length == 1 && !(context.User is UnturnedUser)))
            {
                throw new CommandWrongUsageException();
            }

            int amount = context.Parameters.Get<int>(0);

            if (amount > MAX_INPUT_VALUE || amount < -MAX_INPUT_VALUE)
            {
                throw new CommandWrongUsageException(Translations.Get("NUMBER_BETWEEN", -MAX_INPUT_VALUE, MAX_INPUT_VALUE));
            }

            if (context.Parameters.Length == 1)
            {
                GiveRep(((UnturnedUser)context.User).Player, amount);
                return;
            }

            if (context.Parameters[0] == "*")
            {
                var playerManager = context.Container.Resolve<IPlayerManager>();

                playerManager.OnlinePlayers
                    .Select(c => c as UnturnedPlayer)
                    .Where(c => c != null)
                    .ForEach(p => GiveRep(p, amount));

                if (amount >= 0)
                {
                    context.User.SendLocalizedMessage(Translations, "REPUTATION_GIVEN", amount,
                        Translations.Get("EVERYONE"));
                }
                else
                {
                    context.User.SendLocalizedMessage(Translations, "REPUTATION_TAKE", -amount,
                        Translations.Get("EVERYONE"));
                }

                return;
            }

            if(!(context.Parameters.Get<IPlayer>(1) is UnturnedPlayer player))
                throw new PlayerNameNotFoundException(context.Parameters[1]);

            if (amount >= 0)
            {
                context.User.SendLocalizedMessage(Translations, "REPUTATION_GIVEN", amount, player.DisplayName);
            }
            else
            {
                context.User.SendLocalizedMessage(Translations, "REPUTATION_TAKE", -amount, player.DisplayName);
            }

            GiveRep(player, amount);
        }

        public void GiveRep(UnturnedPlayer player, int amount)
        {
            player.NativePlayer.skills.askRep(amount);

            if (amount >= 0)
            {
                player.User.SendLocalizedMessage(Translations, "REPUTATION_RECEIVED", amount);
            }
            else
            {
                player.User.SendLocalizedMessage(Translations, "REPUTATION_LOST", -amount);
            }
        }
    }
}