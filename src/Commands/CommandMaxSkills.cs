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

using Essentials.I18n;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using System.Threading;
using SDG.Unturned;
using Rocket.Unturned.Player;
using Essentials.Api.Task;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "maxskills",
        Description = "Set to max level all of your/player skills",
        Usage = "<[true|false]> <player | *>"
    )]
    public class CommandMaxSkills : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.IsEmpty)
            {
                if (src.IsConsole)
                {
                    return CommandResult.ShowUsage();
                }

                GiveMaxSkills(src.ToPlayer());
            }
            else
            {
                if (args.Length < 2 && src.IsConsole)
                {
                    return CommandResult.ShowUsage();
                }

                // player or all
                if (args.Length > 1)
                {
                    if (args[1].Equals("*"))
                    {
                        if (!src.HasPermission($"{Permission}.all"))
                        {
                            return CommandResult.NoPermission($"{Permission}.all");
                        }
                        // idk why i changed this, anyways is working better i think
                        foreach (SteamPlayer sPlayer in Provider.clients)
                        {
                            GiveMaxSkills(UPlayer.From(sPlayer));
                        }

                        EssLang.Send(src, "MAX_SKILLS_ALL");
                    }
                    else
                    {
                        if (!src.HasPermission($"{Permission}.other"))
                        {
                            return CommandResult.NoPermission($"{Permission}.other");
                        }
                        if (!args[1].IsValidPlayerIdentifier)
                        {
                            return CommandResult.LangError("PLAYER_NOT_FOUND", args[1]);
                        }
                        var targetPlayer = args[1].ToPlayer;
                        GiveMaxSkills(targetPlayer);
                        EssLang.Send(src, "MAX_SKILLS_TARGET", targetPlayer.DisplayName);
                    }
                }
            }

            return CommandResult.Success();
        }

        private void GiveMaxSkills(UPlayer player)
        {
            // lets try with this
            player.UnturnedPlayer.skills.ServerUnlockAllSkills();
            EssLang.Send(player, "MAX_SKILLS");
        }
    }
}