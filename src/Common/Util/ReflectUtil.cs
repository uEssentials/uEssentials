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

using System;
using System.Diagnostics;
using System.Reflection;

namespace Essentials.Common.Util {

    public static class ReflectUtil {

        // Empty object array, commonly used on method invocation.
        public static object[] EMPTY_ARGS = new object[0];

        // Common binding flags
        public static BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public static BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public static BindingFlags STATIC_INSTANCE_FLAGS = INSTANCE_FLAGS | STATIC_FLAGS;

        public static TAttribute GetAttributeFrom<TAttribute>(object instance) where TAttribute : Attribute {
            object[] attrs;

            if (instance is MethodInfo) {
                var methodInfo = (MethodInfo) instance;
                attrs = methodInfo.GetCustomAttributes(typeof(TAttribute), false);
                return attrs.Length == 0 ? default(TAttribute) : (TAttribute) attrs.GetValue(0);
            }

            attrs = instance.GetType().GetCustomAttributes(typeof(TAttribute), true);
            return attrs.Length == 0 ? default(TAttribute) : (TAttribute) attrs.GetValue(0);
        }

        public static MethodInfo GetMethod(Type type, string name, BindingFlags flags, Type[] argTypes) {
            var ret = type.GetMethod(name, flags, null, CallingConventions.Any, argTypes ?? new Type[0], null);
            Debug.WriteIf(ret == null, $"Could not find method '{type.FullName}::{name}'", "ReflectUtil");
            return ret;
        }
        
        public static MethodInfo GetMethod(Type type, string name, Type[] argTypes = null) {
            return GetMethod(type, name, STATIC_INSTANCE_FLAGS, argTypes);
        }

        public static MethodInfo GetMethod<T>(string name, BindingFlags flags, Type[] argTypes) {
            return GetMethod(typeof(T), name, flags, argTypes);
        }

        public static MethodInfo GetMethod<T>(string name, Type[] argTypes = null) {
            return GetMethod(typeof(T), name, STATIC_INSTANCE_FLAGS, argTypes);
        }

    }

}