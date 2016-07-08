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

using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Essentials.Common.Reflect {

    public class MethodAccessor<TReturnType> : AbstractAccessor {

        public MethodInfo Info { get; }

        public MethodAccessor(object obj, MethodInfo methodInfo) : base(obj) {
            Info = Preconditions.NotNull(methodInfo, "methodInfo cannot be null");
        }

        public TReturnType Invoke(params object[] parameters) {
            var argList = Info.GetParameters().Where(pi => !pi.IsOptional).ToList();

            if (parameters.Length == argList.Count)
                return (TReturnType) Info.Invoke(Owner, parameters);

            var argBuilder = new StringBuilder();

            argList.ForEach(pi => {
                if (pi.IsOut)
                    argBuilder.Append("out ");

                argBuilder.Append(pi.ParameterType);
                argBuilder.Append(" ");
                argBuilder.Append(pi.Name);
                argBuilder.Append(", ");
                argBuilder.Replace("&", "");
            });

            var argSign = argBuilder.ToString();
            argSign = argSign.Substring(0, argSign.Length - 2);

            var methodSign = $"{Info.Name}({argSign})";
            throw new ArgumentException($"Arguments does not match signature, expected {methodSign}");
        }

    }

}