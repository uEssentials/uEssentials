#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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
#endregion

using System.IO;
using Newtonsoft.Json;

namespace Essentials.Common.Util {

    public static class JsonUtil {

        public static void Serialize(string filePath, object obj) {
            var text = JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });
            File.WriteAllText(filePath, text);
        }

        public static T DeserializeFile<T>(string filePath) {
            var allText = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(allText);
        }

    }

}