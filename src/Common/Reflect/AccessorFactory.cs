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
using System.Reflection;

namespace Essentials.Common.Reflect {

    public static class AccessorFactory {

        public static FieldAccessor<TFieldType> AccessField<TFieldType>(
            object obj, string fieldName) {
            Preconditions.NotNull(obj, "obj cannot be null");
            Preconditions.NotNull(fieldName, "fieldName cannot be null");

            var objType = obj is Type ? (Type) obj : obj.GetType();
            var fieldInfo = objType.GetField(fieldName, (BindingFlags) 60);

            if (fieldInfo == null) return null;

            Type fieldType = fieldInfo.FieldType,
                informedFieldType = typeof(TFieldType);

            Preconditions.IsFalse(informedFieldType == fieldType,
                "Inconsistent given type {0} and field type {1}",
                informedFieldType, fieldType);

            return new FieldAccessor<TFieldType>(obj, fieldInfo);
        }

        public static FieldAccessor<TFieldType> AccessProperty<TFieldType>(
            object obj, string fieldName) {
            return AccessField<TFieldType>(obj, $"<{fieldName}>k__BackingField");
        }

        public static MethodAccessor<TReturnType> AccessMethod<TReturnType>(
            object obj, string methodName) {
            Preconditions.NotNull(obj, "obj cannot be null");
            Preconditions.NotNull(methodName, "methodName cannot be null");

            var objType = obj is Type ? (Type) obj : obj.GetType();
            var methodInfo = objType.GetMethod(methodName, (BindingFlags) 60);

            if (methodInfo == null) return null;

            return new MethodAccessor<TReturnType>(obj, methodInfo);
        }

    }

}