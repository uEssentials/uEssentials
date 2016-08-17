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

using Essentials.Api.Unturned;

namespace Essentials.Api.Command {

    public interface ICommandArgs {

        /// <summary>
        /// Array containing the raw arguments.
        /// </summary>
        /// <returns></returns>
        string[] RawArguments { get; }

        /// <summary>
        /// Array containing the "parsed" arguments.
        /// </summary>
        /// <returns></returns>
        ICommandArgument[] Arguments { get; }

        /// <summary>
        /// Get an argument at given index.
        /// </summary>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        ICommandArgument this[int argIndex] { get; }

        /// <summary>
        /// </summary>
        /// <returns> Length of arguments </returns>
        int Length { get; }

        /// <summary>
        /// </summary>
        /// <returns>Return if arguments are empty</returns>>
        bool IsEmpty { get; }

        /// <summary>
        /// Join all arguments starting in <paramref name="initialIndex"/>
        /// </summary>
        /// <param name="initialIndex">Initial join index</param>
        /// <returns>String containing the 'joined arguments'</returns>
        string Join(int initialIndex);

        /// <summary>
        /// Join all arguments between <paramref name="startIndex"/> and <paramref name="endIndex"/>
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <param name="separator">Argument separator</param>
        /// <returns>String containing the 'joined arguments'</returns>
        string Join(int startIndex, int endIndex, string separator);

    }

    /// <summary>
    /// Represent an argument
    /// </summary>
    public interface ICommandArgument {
        int Index { get; }

        string RawValue { get; }

        bool IsBool { get; }

        bool IsString { get; }

        /// <summary>
        /// Check if argument is an valid player name / steam 64 id.
        /// </summary>
        /// <returns> Return if argument is an valid player name / steam 64 id.</returns>
        bool IsValidPlayerIdentifier { get; }

        bool IsLong { get; }

        bool IsULong { get; }

        bool IsInt { get; }

        bool IsUInt { get; }

        bool IsShort { get; }

        bool IsUShort { get; }

        bool IsDouble { get; }

        bool IsFloat { get; }

        long ToLong { get; }

        ulong ToULong { get; }

        int ToInt { get; }

        uint ToUInt { get; }

        short ToShort { get; }

        ushort ToUShort { get; }

        string ToLowerString { get; }

        string ToUpperString { get; }

        bool ToBool { get; }

        double ToDouble { get; }

        float ToFloat { get; }

        UPlayer ToPlayer { get; }
    }

}