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

using System.IO;
using Newtonsoft.Json;

namespace Essentials.Common.Util
{
    public static class JsonUtil
    {
        public static void Serialize( string filePath, object obj )
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream( filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );

                using (TextWriter writer = new StreamWriter( fs ))
                {
                    fs = null;
                    var jsonWriter = new JsonTextWriter( writer );
                    var serializer = JsonSerializer.Create();

                    serializer.Formatting = Formatting.Indented;
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Serialize( jsonWriter, obj );

                    jsonWriter.Close();
                    writer.Close();
                }
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public static T DeserializeFile<T>( string configPath )
        {
            var allText = File.ReadAllText( configPath );

            return JsonConvert.DeserializeObject<T>( allText );
        }
    }
}