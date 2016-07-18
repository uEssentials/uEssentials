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
using Newtonsoft.Json;
using UnityEngine;

namespace Essentials.Json.Converters {

    public class Vector3Converter : JsonConverter {

        public override bool CanRead => true;
        private readonly string[] _propNames = { "X", "Y", "Z" };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var vec = (Vector3) value;

            writer.WriteStartObject();
            for (var i = 0; i < 3; i++) {
                writer.WritePropertyName(_propNames[i]);
                writer.WriteValue(vec[i]);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var ret = new Vector3();
            reader.Read(); // Read StartObject
            for (var i = 0; i < 3; i++) {
                reader.Read(); // Read Value
                ret[i] = (float) (double) reader.Value;
                reader.Read(); // Read PropertyName & EndObject
            }
            return ret;
        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Vector3);
        }

    }

}