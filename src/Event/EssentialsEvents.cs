/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Events;

namespace Essentials.Event {

    public class EssentialsEvents {

        public delegate void CommandPreExecute(CommandPreExecuteEvent e);
        public static event CommandPreExecute OnCommandPreExecute;

        public delegate void CommandPosExecute(CommandPosExecuteEvent e);
        public static event CommandPosExecute OnCommandPosExecute;


        internal static CommandPreExecuteEvent CallCommandPreExecute(ICommand command, ref ICommandArgs cmdArgs, 
                                                                     ref ICommandSource commandSource) {
            var evt = new CommandPreExecuteEvent(command, cmdArgs, commandSource);
            OnCommandPreExecute?.Invoke(evt);
            cmdArgs = evt.Arguments;
            commandSource = evt.Source;
            return evt;
        }

        internal static CommandPosExecuteEvent CallCommandPosExecute(ICommand command, ref ICommandArgs cmdArgs, 
                                                                    ref ICommandSource commandSource, ref CommandResult result) {
            var evt = new CommandPosExecuteEvent(command, cmdArgs, commandSource, result);
            OnCommandPosExecute?.Invoke(evt);
            cmdArgs = evt.Arguments;
            commandSource = evt.Source;
            result = evt.Result;
            return evt;
        }
    }

}