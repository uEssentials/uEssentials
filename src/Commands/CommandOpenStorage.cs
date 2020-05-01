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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "openstorage",
        Aliases = new[] { "storage" },
        Description = "Force open any storage!",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 0,
        MaxArgs = 0
    )]
    public class CommandOpenStorage : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var look = player.Look;

            if (PhysicsUtility.raycast(new Ray(look.aim.position, look.aim.forward), out RaycastHit hit, Mathf.Infinity, RayMasks.BARRICADE))
            {
                InteractableStorage storage = hit.transform.GetComponent<InteractableStorage>();

                if (storage != null)
                {
                    storage.isOpen = true;
                    storage.opener = player.UnturnedPlayer;
                    player.Inventory.isStoring = true;
                    player.Inventory.isStorageTrunk = false;
                    player.Inventory.storage = storage;
                    player.Inventory.updateItems(PlayerInventory.STORAGE, storage.items);
                    player.Inventory.sendStorage();

                    EssLang.Send(src, "STORAGE_OPEN");
                    return CommandResult.Success();
                }
                else
                {
                    return CommandResult.LangError("STORAGE_INVALID");
                }
            }
            else
            {
                return CommandResult.LangError("NO_OBJECT");
            }
        }

    }

}