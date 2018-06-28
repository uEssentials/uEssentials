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
using Essentials.Api.Metadata;
using UnityEngine;
using Essentials.Common.Util;
using Rocket.API.Commands;
using Rocket.API.Permissions;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Core.Player;
using Rocket.UnityEngine.Extensions;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo("back",
        "Return to position of your death.",
        Aliases = new[] {"ret"}
    )]
    public class CommandBack : EssCommand
    {
        internal const string META_KEY_DELAY = "back_delay";
        internal const string META_KEY_POS = "back_pos";

        public override void Execute(ICommandContext context)
        {
            var player = ((UnturnedUser) context.User).Player;
            var playerMeta = player.Container.Resolve<IMetadataStore>();

            if (!playerMeta.Has(META_KEY_POS) || !playerMeta.Has(META_KEY_DELAY))
            {
                throw new CommandWrongUsageException(Translations.Get("NOT_DIED_YET"));
            }

            var deathTime = playerMeta.Get<DateTime>(META_KEY_DELAY);
            var delta = UEssentials.ConfigurationInstance.BackDelay - (DateTime.Now - deathTime).Seconds;

            if (delta > 0 && player.CheckPermission("essentials.bypass.backdelay") != PermissionResult.Grant)
            {
                throw new CommandWrongUsageException(Translations.Get("BACK_DELAY",
                    TimeUtil.FormatSeconds((uint) delta)));
            }

            var backPosition = playerMeta.Get<Vector3>(META_KEY_POS);
            player.GetEntity().Teleport(backPosition.ToSystemVector());
            context.User.SendLocalizedMessage(Translations, "RETURNED");

            playerMeta.Remove(META_KEY_DELAY);
            playerMeta.Remove(META_KEY_POS);
        }

        public override bool SupportsUser(Type user)
        {
            return typeof(UnturnedUser).IsAssignableFrom(user);
        }

        public CommandBack(IPlugin plugin) : base(plugin)
        {
        }
    }
}