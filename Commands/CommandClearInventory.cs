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
using Rocket.Core.Permissions;
using Rocket.Core.User;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        "clearinventory",
        "Clear your/player's inventory",
        Aliases = new[] { "ci" },
        Syntax = "<player | *>"
    )]
    public class CommandClearInventory : EssCommand
    {
        public readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0 && !(context.User is UnturnedUser))
                throw new CommandWrongUsageException();

            var playerManager = context.Container.Resolve<IPlayerManager>();

            if (context.Parameters.Length == 0)
            {
                // self
                ClearInventory(((UnturnedUser)context.User).Player);
                return;
            }

            if (context.Parameters[0].Equals("*"))
            {
                // all
                if (context.User.CheckPermission($"ClearInventory.all") != PermissionResult.Grant)
                {
                    throw new NotEnoughPermissionsException(context.User, "ClearInventory.all");
                }

                playerManager.OnlinePlayers
                    .Select(c => c as UnturnedPlayer)
                    .Where(c => c != null)
                    .ForEach(ClearInventory);

                context.User.SendLocalizedMessage(Translations, "INVENTORY_CLEARED_ALL");
                return;
            }

            if (!(context.Parameters.Get<IPlayer>(0) is UnturnedPlayer targetPlayer))
                throw new PlayerNameNotFoundException(context.Parameters[0]);

            if (context.User.CheckPermission($"ClearInventory.other") != PermissionResult.Grant)
                throw new NotEnoughPermissionsException(context.User, "ClearInventory.other");

            ClearInventory(targetPlayer);
            context.User.SendLocalizedMessage(Translations, "INVENTORY_CLEARED_PLAYER", targetPlayer.DisplayName);
        }

        private void ClearInventory(UnturnedPlayer player)
        {
            var playerInv = player.Inventory;

            // "Remove "models" of items from player "body""
            player.NativePlayer.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                (byte)0, (byte)0, EMPTY_BYTE_ARRAY);
            player.NativePlayer.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                (byte)1, (byte)0, EMPTY_BYTE_ARRAY);

            // Remove items
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (page == PlayerInventory.AREA)
                    continue;

                var count = playerInv.getItemCount(page);

                for (byte index = 0; index < count; index++)
                {
                    playerInv.removeItem(page, 0);
                }
            }

            // Remove clothes

            // Remove unequipped cloths
            System.Action removeUnequipped = () =>
            {
                for (byte i = 0; i < playerInv.getItemCount(2); i++)
                {
                    playerInv.removeItem(2, 0);
                }
            };

            // Unequip & remove from inventory
            player.NativePlayer.clothing.askWearBackpack(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.NativePlayer.clothing.askWearGlasses(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.NativePlayer.clothing.askWearHat(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.NativePlayer.clothing.askWearPants(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.NativePlayer.clothing.askWearMask(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.NativePlayer.clothing.askWearShirt(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.NativePlayer.clothing.askWearVest(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.User?.SendLocalizedMessage(Translations, "INVENTORY_CLEARED");
        }

        public CommandClearInventory(IPlugin plugin) : base(plugin)
        {
        }
    }
}