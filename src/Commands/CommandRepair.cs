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

using System.Collections.Generic;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common.Util;
using SDG.Unturned;
using Essentials.I18n;
using Essentials.Common;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "repair",
        Aliases = new[] { "fix" },
        Description = "Repair all items in your inventory.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandRepair : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();

            player.Inventory.items.ForEach(item => Repair(player, item));
            EssLang.Send(src, "ALL_REPAIRED");

            return CommandResult.Success();
        }

        private void Repair(UPlayer player, Items item)
        {
            if (item == null) return;

            var playerInv = player.UnturnedPlayer.inventory;
            var items = item.items;

            foreach (var itemJar in items.Where(itemJar => itemJar.item.quality != 100))
            {
                playerInv.sendUpdateQuality(item.page, itemJar.x, itemJar.y, 100);

                var barrel = ItemUtil.GetWeaponAttachment(itemJar.item, ItemUtil.AttachmentType.BARREL);
                barrel.IfPresent(attach => {
                    if (attach.Durability == 100) return;

                    attach.Durability = 100;
                    ItemUtil.SetWeaponAttachment(itemJar.item, ItemUtil.AttachmentType.BARREL, attach);
                });
            }
        }

    }

}