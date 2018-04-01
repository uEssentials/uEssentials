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
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using static Essentials.Commands.MiscCommands;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "tell",
        Aliases = new[] { "msg", "pm" },
        Description = "Send private message to a player",
        Usage = "[player] [message]",
        MinArgs = 2
    )]
    public class CommandTell : EssCommand {

        internal static readonly Dictionary<ulong, ulong> ReplyTo = new Dictionary<ulong, ulong>();

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var target = args[0].ToPlayer;

            if (target == null) {
                return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
            }

            var pmSettings = UEssentials.Config.PrivateMessage;
            var formatFrom = pmSettings.FormatFrom;
            var formatTo = pmSettings.FormatTo;
            var formatSpy = pmSettings.FormatSpy;

            var formatFromColor = ColorUtil.GetColorFromString(ref formatFrom);
            var formatToColor = ColorUtil.GetColorFromString(ref formatTo);
            var formatSpyColor = ColorUtil.GetColorFromString(ref formatSpy);

            var targetName = target.DisplayName;
            var srcName = src.IsConsole ? pmSettings.ConsoleDisplayName : src.DisplayName;

            formatFrom = string.Format(formatFrom, srcName, args.Join(1));
            formatTo = string.Format(formatTo, targetName, args.Join(1));
            formatSpy = string.Format(formatSpy, srcName, targetName, args.Join(1));

            target.SendMessage(formatFrom, formatFromColor);
            src.SendMessage(formatTo, formatToColor);

            Spies.ForEach(p => {
                UPlayer.From(p).SendMessage(formatSpy, formatSpyColor);
            });

            if (!src.IsConsole) {
                ReplyTo[target.CSteamId.m_SteamID] = src.ToPlayer().CSteamId.m_SteamID;
            }

            return CommandResult.Success();
        }

        protected override void OnUnregistered() => ReplyTo.Clear();

    }

}