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

using UnityEngine;

namespace Essentials.Api.Command {

    public static class CommandExtensions {

        /// <summary>
        /// Try get an vector3 from 3 arguments, starting in <paramref name="initialIndex"/>
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="initialIndex"> Initial index </param>
        /// <returns>New vector3 with given positions.</returns>
        public static Vector3? GetVector3(this ICommandArgs src, int initialIndex) {
            if (initialIndex + 3 > src.Length) {
                return null;
            }

            var x = src.Arguments[initialIndex];
            var y = src.Arguments[initialIndex + 1];
            var z = src.Arguments[initialIndex + 2];

            if (x.IsFloat && y.IsFloat && z.IsFloat) {
                return new Vector3(x.ToFloat, y.ToFloat, z.ToFloat);
            }

            return null;
        }

    }

}