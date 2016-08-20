/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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
using Essentials.CodeAnalysis;
using Essentials.Common;

namespace Essentials.Api.Events {

    public class CommandEvent {

        /// <summary>
        /// Command that was/will be executed.
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// Arguments
        /// </summary>
        public ICommandArgs Arguments {
            get {
                return _args;
            }
            set {
                _args = Preconditions.NotNull(value, "args cannot be null");
            }
        }

        /// <summary>
        /// Who is executing the <see cref="Command"/>.
        /// </summary>
        public ICommandSource Source {
            get {
                return _source;
            }
            set {
                _source = Preconditions.NotNull(value, "source cannot be null");
            }
        }

        public CommandEvent([NotNull] ICommand command, ICommandArgs args, ICommandSource src) {
            Command = command;
            Arguments = args;
            Source = src;
        }

        private ICommandSource _source;
        private ICommandArgs _args;
    }

}