#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
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

using Essentials.Api;
using Essentials.Api.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Essentials.Configuration {

    public class WebResources : JsonConfig {

        public override string FileName => "web_resources.json";

        public bool Enabled { get; set; }

        public Dictionary<string, string> URLs { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Key: name (e.g. config)
        /// Value: file contents
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> Loaded { get; } = new Dictionary<string, string>();

        public override void Load(string filePath) {
            try {
                base.Load(filePath);

                if (!Enabled) {
                    return;
                }

                using (var webClient = new WebClient()) {
                    var logger = UEssentials.Logger;
                    foreach (var urL in URLs) {
                        logger.LogInfo($"WebResources: Loading '{urL.Key}' from '{urL.Value}'...");
                        var fileContents = webClient.DownloadString(urL.Value);
                        if (string.IsNullOrEmpty(fileContents)) {
                            logger.LogWarning($"WebResources: The fileContents of '{urL.Key}' " +
                                               "is null or empty. skipping...");
                            continue;
                        }
                        Loaded.Add(urL.Key, fileContents);
                        logger.LogInfo($"WebResources: Successfully loaded '{urL.Key}' from '{urL.Value}'.");
                    }
                }
            } catch (Exception ex) {
                UEssentials.Logger.LogError("Failed to load WebResources.");
                UEssentials.Logger.LogException(ex);
            }
        }

        public override void LoadDefaults() {
            Enabled = false;
            URLs = new Dictionary<string, string> {
                { "Config", "http://example.com/config.json" },
                { "Kits", "http://example.com/kits.json" }
            };
        }

    }

}