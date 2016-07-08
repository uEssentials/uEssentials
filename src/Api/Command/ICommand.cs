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

using Essentials.Api.Command.Source;

namespace Essentials.Api.Command {

    /// <summary>
    /// This class represents an command
    /// </summary>
    public interface ICommand {

        /// <summary>
        /// Name of this command
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Usage of this command
        /// </summary>
        string Usage { get; set; }

        /// <summary>
        /// Aliases of this command
        /// </summary>
        string[] Aliases { get; set; }

        /// <summary>
        /// Description of this command
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Permission of this command
        /// </summary>
        string Permission { get; set; }

        /// <summary>
        /// Allowed sources
        /// </summary>
        /// <see cref="AllowedSource"/>
        AllowedSource AllowedSource { get; set; }

        /// <summary>
        /// Called when someone execute this command.
        /// </summary>
        /// <param name="src">Source who is calling this command, can be console or player</param>
        /// <param name="args">Arguments passed when this command is called</param>
        CommandResult OnExecute(ICommandSource src, ICommandArgs args);

    }

}