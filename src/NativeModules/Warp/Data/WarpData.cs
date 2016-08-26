#region License
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
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Essentials.Api;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core.Storage;
using Newtonsoft.Json;

namespace Essentials.NativeModules.Warp.Data {

    public class WarpData : IData<Dictionary<string, Warp>> {

        private static string DataFilePath {
            get {
                var dataFolder = UEssentials.DataFolder;
                var filePath = $"{dataFolder}warps.json";

                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                if (!File.Exists(filePath))
                    File.Create(filePath).Close();

                return filePath;
            }
        }

        public void Save(Dictionary<string, Warp> warps) {
            JsonUtil.Serialize(DataFilePath, warps.Values.ToArray());
        }

        public Dictionary<string, Warp> Load() {
            var loadedWarps = new Dictionary<string, Warp>();
            var deserializedWarpArray = JsonConvert.DeserializeObject<Warp[]>(File.ReadAllText(DataFilePath));

            deserializedWarpArray?.ForEach(warp => {
                loadedWarps.Add(warp.Name.ToLowerInvariant(), warp);
            });
            return loadedWarps;
        }

    }

}