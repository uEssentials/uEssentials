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
using Essentials.NativeModules.Warp.Commands;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.I18N;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "requesturl",
        "Request player to open an URL.",
        Aliases = new[] {"requrl"},
        Syntax= "[player/*] [message] [url]"
    )]
    public class CommandRequestUrl : EssCommand
    {
        public CommandRequestUrl(IPlugin plugin) : base(plugin)
        {
        }
        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if(context.Parameters.Length != 3)
                throw new CommandWrongUsageException();

            var message = context.Parameters[1];
            var url = context.Parameters[2];

            if (context.Parameters[0].Equals("*"))
            {
                var playerManager = context.Container.Resolve<IPlayerManager>();
                playerManager.OnlinePlayers.Select(c => c as UnturnedPlayer)
                    .Where(c => c != null)
                    .ForEach(p => p.NativePlayer.sendBrowserRequest(message, url));

                context.User.SendLocalizedMessage(Translations, "REQUEST_URL_SUCCESS", Translations.Get("EVERYONE"), url);
                return;
            }

            if(!(context.Parameters.Get<IPlayer>(0) is UnturnedPlayer target))
                throw new PlayerNameNotFoundException(context.Parameters[0]);

            target.NativePlayer.sendBrowserRequest(message, url);
            context.User.SendLocalizedMessage(Translations, "REQUEST_URL_SUCCESS", target.DisplayName, url);
        }
    }
}