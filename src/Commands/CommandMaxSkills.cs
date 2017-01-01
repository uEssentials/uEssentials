#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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
using Essentials.I18n;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "maxskills",
        Description = "Set to max level all of your/player skills",
        Usage = "<overpower[true|false]> <player | *>"
    )]
    public class CommandMaxSkills : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty) {
                if (src.IsConsole) {
                    return CommandResult.ShowUsage();
                }

                GiveMaxSkills(src.ToPlayer(), false);
            } else {
                if (args.Length < 2 && src.IsConsole) {
                    return CommandResult.ShowUsage();
                }

                if (!args[0].IsBool) {
                    return CommandResult.Lang("INVALID_BOOLEAN", args[0]);
                }

                var overpower = args[0].ToBool;

                // player or all
                if (args.Length > 1) {
                    if (args[1].Equals("*")) {
                        if (!src.HasPermission($"{Permission}.all")) {
                            return CommandResult.NoPermission($"{Permission}.all");
                        }
                        UServer.Players.ForEach(p => GiveMaxSkills(p, overpower));
                        EssLang.Send(src, "MAX_SKILLS_ALL");
                    } else {
                        if (!src.HasPermission($"{Permission}.other")) {
                            return CommandResult.NoPermission($"{Permission}.other");
                        }
                        if (!args[1].IsValidPlayerIdentifier) {
                            return CommandResult.Lang("PLAYER_NOT_FOUND", args[1]);
                        }
                        var targetPlayer = args[1].ToPlayer;
                        GiveMaxSkills(targetPlayer, overpower);
                        EssLang.Send(src, "MAX_SKILLS_TARGET", targetPlayer.DisplayName);
                    }
                } else { // self (with overpower)
                    GiveMaxSkills(src.ToPlayer(), overpower);
                }
            }

            return CommandResult.Success();
        }

        private void GiveMaxSkills(UPlayer player, bool overpower) {
            var pSkills = player.UnturnedPlayer.skills;

            foreach (var skill in pSkills.skills.SelectMany(skArr => skArr)) {
                skill.level = overpower ? byte.MaxValue : skill.max;
            }

            pSkills.askSkills(player.CSteamId);
            EssLang.Send(player, "MAX_SKILLS");
        }

    }

}