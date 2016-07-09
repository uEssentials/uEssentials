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
using System.Collections.Generic;
using System.IO;
using Essentials.Api;
using Essentials.Common;
using Essentials.Core.Storage;
using UnityEngine;

namespace Essentials.NativeModules.Warp.Data {

    public class BinaryWarpData : IData<Dictionary<string, Warp>> {

        private readonly WarpData _legacyData = new WarpData();

        public void Save(Dictionary<string, Warp> data) {
            var dataFile = Path.Combine(UEssentials.DataFolder, "warps.dat");
            var fs = new FileStream(dataFile, FileMode.OpenOrCreate);
            var buffer = new BinaryWriter(fs);

            data.Values.ForEach(warp => {
                buffer.Write(warp.Name);
                buffer.Write(warp.Location.x);
                buffer.Write(warp.Location.y);
                buffer.Write(warp.Location.z);
                buffer.Write(warp.Rotation);
            });

            fs.Close();
            buffer.Close();
        }

        public Dictionary<string, Warp> Load() {
            var legacyFile = Path.Combine(UEssentials.DataFolder, "warps.json");
            var dataFile = Path.Combine(UEssentials.DataFolder, "warps.dat");

            if (File.Exists(legacyFile)) {
                var legacyRet = _legacyData.Load();
                File.Delete(legacyFile);
                return legacyRet;
            }

            var ret = new Dictionary<string, Warp>();
            var fs = new FileStream(dataFile, FileMode.OpenOrCreate);
            var buffer = new BinaryReader(fs);

            for (;;) {
                try {
                    var name = buffer.ReadString();
                    var x = buffer.ReadSingle();
                    var y = buffer.ReadSingle();
                    var z = buffer.ReadSingle();
                    var rotation = buffer.ReadSingle();

                    var warp = new Warp(name, new Vector3(x, y, z), rotation);
                    ret.Add(name.ToLowerInvariant(), warp);
                } catch (EndOfStreamException) {
                    break;
                } catch (Exception e) {
                    UEssentials.Logger.LogError("Corrupted data?");
                    UEssentials.Logger.LogError(e.ToString());
                }
            }

            fs.Close();
            buffer.Close();
            return ret;
        }

    }

}