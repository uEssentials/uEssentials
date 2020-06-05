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

using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "tp",
        Usage = "[player|place|x y z] or [player] [player|place|x y z]",
        Description = "Teleport to a player or place."
    )]
    public class CommandTp : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (src.IsConsole && (args.Length == 1 || args.Length == 3)) {
                return CommandResult.ShowUsage();
            }

            switch (args.Length) {
                /*
                    /tp player  -> teleport sender to player
                    /tp place   -> teleport sender to place
                */
                case 1: {
                    FindPlaceOrPlayer(args[0].ToString(), out var dataFound, out var dataPosition, out var dataName);

                    if (!dataFound) {
                        return CommandResult.LangError("FAILED_FIND_PLACE_OR_PLAYER", args[0]);
                    }

                    src.ToPlayer().Teleport(dataPosition);
                    EssLang.Send(src, "TELEPORTED", dataName);
                    break;
                }

                /*
                    /tp player other player   -> teleport player to other player
                    /tp player place          -> teleport player to place
                */
                case 2: {
                    if (!args[0].IsValidPlayerIdentifier) {
                        return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                    }

                    var target = args[0].ToPlayer;

                    FindPlaceOrPlayer(args[1].ToString(), out var dataFound, out var dataPosition, out var dataName);

                    if (!dataFound) {
                        return CommandResult.LangError("FAILED_FIND_PLACE_OR_PLAYER", args[1]);
                    }

                    target.Teleport(dataPosition);
                    EssLang.Send(target, "TELEPORTED", dataName);
                    EssLang.Send(src, "TELEPORTED_SENDER", target, dataName);
                    break;
                }

                /*
                    /tp x y z          -> sender to x,y,z
                */
                case 3: {
                    var location = args.GetVector3(0);

                    if (location.HasValue) {
                        src.ToPlayer().Teleport(location.Value + new Vector3(0f, 0.5f, 0f));
                        EssLang.Send(src, "TELEPORTED", location);
                    } else {
                        return CommandResult.LangError("INVALID_COORDS", args[0], args[1], args[2]);
                    }
                    break;
                }

                /*
                    /tp player x y z   -> player to x, y, z
                */
                case 4: {
                    if (!args[0].IsValidPlayerIdentifier) {
                        return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                    }

                    var target = args[0].ToPlayer;
                    var location = args.GetVector3(1);

                    if (location.HasValue) {
                        target.Teleport(location.Value + new Vector3(0f, 0.5f, 0f));
                        EssLang.Send(target, "TELEPORTED", location);
                        EssLang.Send(src, "TELEPORTED_SENDER", target, location);
                    } else {
                        return CommandResult.LangError("INVALID_COORDS", args[1], args[2], args[3]);
                    }
                    break;
                }

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

        private static void FindPlaceOrPlayer(string arg, out bool found,
                                              out Vector3 position, out string placeOrPlayer) {
            position = Vector3.zero;
            placeOrPlayer = string.Empty;

            var player = UPlayer.From(arg);
            if (player != null) {
                found = true;
                position = player.Position;
                placeOrPlayer = player.DisplayName;
            } else {
                found = TryFindPlace(arg, out var node);

                if (found) {
                    placeOrPlayer = node.name;
                    position = node.point + new Vector3(0, 1, 0);
                }
            }
        }

        private static bool TryFindPlace(string name, out LocationNode outNode) {
            outNode = (
                from node in LevelNodes.nodes
                where node.type == ENodeType.LOCATION
                let locNode = node as LocationNode
                where locNode.name.ToLower().Contains(name.ToLower())
                select locNode
            ).FirstOrDefault();
            return outNode != null;
        }

    }

}
