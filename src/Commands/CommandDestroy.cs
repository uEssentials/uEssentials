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
using System.Reflection;
using UnityEngine;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "destroy",
        Description = "Destroys the barricade or structure that you are looking at.",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 0,
        MaxArgs = 0
    )]
    public class CommandDestroy : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var look = player.Look;

            if (PhysicsUtility.raycast(new Ray(look.aim.position, look.aim.forward), out RaycastHit hit, Mathf.Infinity, RayMasks.BARRICADE | RayMasks.STRUCTURE))
            {
                Interactable2SalvageBarricade barri = hit.transform.GetComponent<Interactable2SalvageBarricade>();
                Interactable2SalvageStructure struc = hit.transform.GetComponent<Interactable2SalvageStructure>();

                if (barri != null)
                {
                    // changed
                    BarricadeManager.tryGetInfo(barri.root, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);

                    region.barricades.RemoveAt(index);

                    BarricadeManager.instance.channel.send("tellTakeBarricade", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                    {
                        x,
                        y,
                        plant,
                        index
                    });

                    EssLang.Send(src, "BARRICADE_REMOVED");
                    return CommandResult.Success();
                }
                else if (struc != null)
                {
                    // changed
                    StructureManager.tryGetInfo(struc.transform, out byte x, out byte y, out ushort index, out StructureRegion region);

                    region.structures.RemoveAt(index);

                    StructureManager.instance.channel.send("tellTakeStructure", ESteamCall.ALL, x, y, StructureManager.STRUCTURE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                    {
                        x,
                        y,
                        index,
                        (region.drops[index].model.position - player.UnturnedPlayer.transform.position).normalized * 100f
                    });

                    EssLang.Send(src, "STRUCTURE_REMOVED");
                    return CommandResult.Success();
                }
                else
                {
                    return CommandResult.LangError("DESTROY_INVALID");
                }
            }
            else
            {
                return CommandResult.LangError("NO_OBJECT");
            }
        }

    }

}