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
using Essentials.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Essentials.Configuration {

    public class TextCommands : JsonConfig {

        public override string FileName => "text_commands.json";

        public List<TextCommandData> Commands { get; } = new List<TextCommandData>();

        public override void LoadDefaults() {
            Commands.Add(new TextCommandData {
                Name = "rules",
                Text = new [] {
                    "Be Respectful.",
                    "Don't use cheats.",
                    "Have fun :D"
                }
            });
            Commands.Add(new TextCommandData {
                Name = "website",
                Text = new [] {
                    "Our website: github.com/uEssentials"
                }
            });
        }

        public override void Save(string filePath) {
            JsonUtil.Serialize(filePath, Commands);
        }

        public override void Load(string filePath) {
            if (File.Exists(filePath)) {
                try {
                    JsonConvert.PopulateObject(File.ReadAllText(filePath), Commands);
                } catch (Exception ex) {
                    UEssentials.Logger.LogError("Failed to load TextCommands.");
                    UEssentials.Logger.LogException(ex);
                    UEssentials.Logger.LogError("Using default...");
                    LoadDefaults();
                }
            } else {
                LoadDefaults();
                Save(filePath);
           }
        }

        public struct TextCommandData {

            public string Name;
            public string[] Text;

        }

    }

}