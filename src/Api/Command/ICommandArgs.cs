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
using UnityEngine;

namespace Essentials.Api.Command {

    /// <summary>
    /// The purpose of this is help the manipulation
    /// of arguments given when an command is executed.
    /// </summary>
    public interface ICommandArgs {

        /// <summary>
        /// Array containing the raw arguments.
        /// </summary>
        /// <returns></returns>
        string[] RawArguments { get; }

        /// <summary>
        /// Array containing the "treated" arguments.
        /// </summary>
        /// <returns></returns>
        ICommandArgument[] Arguments { get; }

        /// <summary>
        /// Get and argument at given index.
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
        /// <returns>Return is arguments are empty</returns>>
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

        /// <summary>
        /// Try get an vector3 from 3 arguments, starting in <paramref name="initialIndex"/>
        /// </summary>
        /// <param name="initialIndex"> Initial index </param>
        /// <returns>New vector3 with given positions.</returns>
        Vector3? GetVector3(int initialIndex);

    }

    /// <summary>
    /// Represent an command argument
    /// </summary>
    public interface ICommandArgument {

        /// <summary>
        ///
        /// </summary>
        int Index { get; }

        /// <summary>
        ///
        /// </summary>
        string RawValue { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsBool { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsString { get; }

        /// <summary>
        /// Check if argument in determined index is an valid player name
        /// </summary>
        /// <returns> Return is argument is an valid player name </returns>
        bool IsValidPlayerName { get; }

        /// <summary>
        /// Check if argument in determined index is int
        /// </summary>
        /// <returns> Return is argument is int </returns>
        bool IsInt { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsUint { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsShort { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsUshort { get; }

        /// <summary>
        /// Check if argument in determined index is double
        /// </summary>
        /// <returns> Return is argument is double </returns>
        bool IsDouble { get; }

        /// <summary>
        ///
        /// </summary>
        /// <returns> Return is argument is double </returns>
        bool IsFloat { get; }

        /// <summary>
        ///
        /// </summary>
        int ToInt { get; }

        /// <summary>
        ///
        /// </summary>
        uint ToUint { get; }

        /// <summary>
        ///
        /// </summary>
        short ToShort { get; }

        /// <summary>
        ///
        /// </summary>
        ushort ToUshort { get; }

        /// <summary>
        ///
        /// </summary>
        string ToLowerString { get; }

        /// <summary>
        ///
        /// </summary>
        string ToUpperString { get; }

        /// <summary>
        ///
        /// </summary>
        bool ToBool { get; }

        /// <summary>
        ///
        /// </summary>
        double ToDouble { get; }

        /// <summary>
        ///
        /// </summary>
        float ToFloat { get; }

        /// <summary>
        ///
        /// </summary>
        UPlayer ToPlayer { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool Is(int other);

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool Is(double other);

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        bool Is(string other, bool ignoreCase = true);

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool Is(UPlayer other);

        /// <summary>
        ///
        /// </summary>
        /// <param name="ignoreCase"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        bool IsOneOf(string[] others, bool ignoreCase = true);

    }

}