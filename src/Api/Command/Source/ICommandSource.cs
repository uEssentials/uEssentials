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

using Essentials.Api.Permission;
using UnityEngine;

namespace Essentials.Api.Command.Source {

    /// <summary>
    /// This classe represents a source of <see cref="ICommand"/>,
    /// that executed the command.
    /// </summary>
    public interface ICommandSource : IPermissible {

        /// <summary>
        /// Returns the CSteamID of source ( as string )
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Returns the display name
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Returns if this is Console
        /// </summary>
        bool IsConsole { get; }

        /// <summary>
        /// Return if source is Admin
        /// </summary>
        bool IsAdmin { get; }

        /// <summary>
        /// Send an message to player/console
        /// </summary>
        void SendMessage(object message);

        /// <summary>
        /// Send an message to player/console
        /// </summary>
        void SendMessage(object message, Color color);

        /// <summary>
        /// Dispatch an command
        /// </summary>
        /// <param name="command"> Command that you want to dispatch </param>
        void DispatchCommand(string command);

    }

}