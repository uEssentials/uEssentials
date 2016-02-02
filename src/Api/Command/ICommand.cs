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

namespace Essentials.Api.Command
{
    /// <summary>
    /// Author: Leonardosc
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Name of the command
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Usage of the command
        /// </summary>
        string Usage { get; }

        /// <summary>
        /// Aliases of the command
        /// </summary>
        string[] Aliases { get; }

        /// <summary>
        /// Description of the command
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Permission of the command
        /// </summary>
        string Permission { get; }

        /// <summary>
        /// Allowed sources
        /// </summary>
        AllowedSource AllowedSource { get; }

        /// <summary>
        /// This method is called when player execute this command
        /// </summary>
        /// <param name="src">Source who is calling this command, can be console or player</param>
        /// <param name="args">Arguments passed when this command is called</param>
        void OnExecute( ICommandSource src, ICommandArgs args ); 
    }
}