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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.I18n;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "reputation",
        Aliases = new[] {"rep"},
        Description = "Give reputation to you/player",
        Usage = "[amount] <target/*>"
    )]
    public class CommandReputation : EssCommand {

        private const int MAX_INPUT_VALUE = 10000000;

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {

            if (args.Length == 0 || (args.Length == 1 && src.IsConsole)) { return CommandResult.ShowUsage(); }

            if (!args[0].IsInt) { return CommandResult.Lang("INVALID_NUMBER", args[0]); }

            int amount = args[0].ToInt;

            if (amount > MAX_INPUT_VALUE || amount < -MAX_INPUT_VALUE) {
                return CommandResult.Lang("NUMBER_BETWEEN", -MAX_INPUT_VALUE, MAX_INPUT_VALUE);
            }

            if (args.Length == 2) {
                if (args[1].RawValue == "*") {
                    UServer.Players.ForEach(p => GiveRep(p, amount));

                    if (amount >= 0) {
                        EssLang.Send(src, "REPUTATION_GIVEN", amount, EssLang.Translate("EVERYONE"));
                    } else {
                        EssLang.Send(src, "REPUTATION_TAKE", -amount, EssLang.Translate("EVERYONE"));
                    }
                } else if (!args[1].IsValidPlayerIdentifier) {
                    return CommandResult.Lang("PLAYER_NOT_FOUND", args[1]);
                } else {
                    UPlayer player = args[1].ToPlayer;

                    if (amount >= 0) {
                        EssLang.Send(src, "REPUTATION_GIVEN", amount, player.DisplayName);
                    } else {
                        EssLang.Send(src, "REPUTATION_TAKE", -amount, player.DisplayName);
                    }

                    GiveRep(player, amount);
                }
            } else {
                GiveRep(src.ToPlayer(), amount);
            }

            return CommandResult.Success();
        }

        private static void GiveRep(UPlayer player, int amount) {

            player.RocketPlayer.Player.skills.askRep(amount);

            if (amount >= 0) { EssLang.Send(player, "REPUTATION_RECEIVED", amount); } else { EssLang.Send(player, "REPUTATION_LOST", -amount); }

        }

    }

}