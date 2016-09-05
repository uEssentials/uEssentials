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

using System;
using System.Collections.Generic;
using System.IO;
using Essentials.Api;
using Essentials.Api.Configuration;
using Essentials.Common.Util;
using Newtonsoft.Json.Linq;

namespace Essentials.Configuration {

    public class CommandOptions : JsonConfig {

        public override string FileName => "command_options.json";

        /// <summary>
        /// Key: command name (lower case)
        /// </summary>
        public IDictionary<string, CommandEntry> Commands { get; } = new Dictionary<string, CommandEntry>();

        public override void Save(string filePath) {
            JsonUtil.Serialize(filePath, Commands);
        }

        public override void Load(string filePath) {
            try {
                if (File.Exists(filePath)) {
                    var json = File.ReadAllText(filePath);
                    foreach (var entry in JObject.Parse(json)) {
                        Commands.Add(entry.Key.ToLowerInvariant(), entry.Value.ToObject<CommandEntry>());
                    }
                } else {
                    base.Load(filePath);
                }
            }  catch (Exception ex) {
                UEssentials.Logger.LogError("Failed to load 'command_options.json'.");
                UEssentials.Logger.LogError($"Error: {ex}");
                UEssentials.Logger.LogError("Using default...");
                LoadDefaults();
            }
        }

        public override void LoadDefaults() {
            Commands.Add("example_1", new CommandEntry {
                Cost = 0m,
                CustomAliases = new [] {
                    "these aliases",
                    "will be",
                    "concatenated",
                    "with the default",
                    "aliases"
                },
                OverridedAliases = new [] {
                    "these alises",
                    "will override",
                    "default aliases"
                },
                Usage = "Usage",
                Description = "Description"
            });
            Commands.Add("example_2", new CommandEntry {
                Cost = 1110m,
                Usage = "Usage",
                Description = "Description"
            });
            Commands.Add("home", new CommandEntry {
                Cooldown = 30
            });
        }

        public struct CommandEntry {
            public string[] CustomAliases;
            public string[] OverridedAliases;
            public string Usage;
            public string Description;
            public decimal Cost;
            public uint Cooldown;
        }

    }

}