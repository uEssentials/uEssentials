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
using System.Collections.Generic;
using Essentials.Api.Command;
using Essentials.Common.Util;
using SDG.Unturned;
using Essentials.Common;
using System.Reflection;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.Core.I18N;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        "repair",
        "Repair all items in your inventory.",
        Aliases = new[] {"fix"}
    )]
    public class CommandRepair : EssCommand
    {
        public CommandRepair(IPlugin plugin) : base(plugin)
        {
        }

        private readonly FieldInfo _itemsField = ReflectUtil.GetField<Items>("items");

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            var player = ((UnturnedUser)context.User).Player;

            player.Inventory.items.ForEach(item => Repair(player, item));
            context.User.SendLocalizedMessage(Translations, "ALL_REPAIRED");
        }

        private void Repair(UnturnedPlayer player, Items item)
        {
            if (item == null) return;

            var playerInv = player.NativePlayer.inventory;
            var items = (List<ItemJar>) _itemsField.GetValue(item);
            byte index = 0;

            items.ForEach(itemJar =>
            {
                item.updateQuality(index, 100);

                playerInv.channel.send("tellUpdateQuality", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                    new object[]
                    {
                        item.page,
                        playerInv.getIndex(item.page, itemJar.x, itemJar.y),
                        100
                    });

                var barrel = ItemUtil.GetWeaponAttachment(itemJar.item, ItemUtil.AttachmentType.BARREL);
                barrel.IfPresent(attach =>
                {
                    attach.Durability = 100;
                    ItemUtil.SetWeaponAttachment(itemJar.item, ItemUtil.AttachmentType.BARREL, attach);
                });
                index++;
            });
        }
    }
}