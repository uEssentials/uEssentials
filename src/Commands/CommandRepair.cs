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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using SDG.Unturned;
using Rocket.API;
using Rocket.Unturned.Player;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "repair",
        Aliases = new[] { "fix" },
        Description = "Repair all items in your inventory.",
        AllowedSource = AllowedSource.BOTH
    )]
    public class CommandRepair : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "repair";
        public string Help => "Repair all items in your inventory.";
        public string Syntax => "/repair";
        public List<string> Aliases => new List<string>() { "fix" };
        public List<string> Permissions => new List<string>() { "repair", "fix" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            switch (command.Length)
            {
                case (0):
                    if (caller is ConsolePlayer)
                        Rocket.Core.Logging.Logger.LogException(new System.InvalidOperationException($"Console does not have an iventory to repair! You must be a player to execute /repair or /fix"));
                    else
                        Repair((UnturnedPlayer)caller);
                    break;
                case (1):
                    CheckCommand();
                    break;
                default:
                    Rocket.Core.Logging.Logger.LogException(new System.InvalidOperationException($"Command can contain only 2 parameters!"));
                    break;
            }

            void Repair(UnturnedPlayer player)
            {
                for (byte page = 0; page < 7; page++)//7, becuase there are currently 7 types of wear where player possibly can store items: EItemType.HAT/PANTS/SHIRT/MASK/BACKPACK/VEST/GLASSES
                {
                    var itemsCount = player.Inventory.getItemCount(page);
                    for (byte index = 0; index < itemsCount; index++)
                    {
                        player.Inventory.sendUpdateQuality(page, player.Inventory.getItem(page, index).x, player.Inventory.getItem(page, index).y, 100);
                        player.Inventory.sendUpdateInvState(page, player.Inventory.getItem(page, index).x, player.Inventory.getItem(page, index).y, new byte[] { 100 });
                    }
                }
            }
            void CheckCommand()
            {
                if (command[0].ToLower() == "all")
                {
                    if (Provider.clients.Count != 0)
                    {
                        foreach (var steamplayer in Provider.clients)
                        {
                            Repair(UnturnedPlayer.FromSteamPlayer(steamplayer));
                        }
                    }
                    else
                        Rocket.Core.Logging.Logger.LogError("players not found!");
                }
                else
                {
                    UnturnedPlayer player = UnturnedPlayer.FromName(command[0]);
                    if (player != null)
                        Repair(player);
                    else
                        Rocket.Core.Logging.Logger.LogError("player not found!");
                }
            }
        }
    }
}
