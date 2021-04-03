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
using Essentials.Common;
using Essentials.Misc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Essentials.Configuration {

    public class EssConfig : JsonConfig {

        public string Locale;

        public PrivateMessageSettings PrivateMessage;

        public bool OldFormatMessages;

        public bool UnfreezeOnDeath;
        public bool UnfreezeOnQuit;

        public bool EnableTextCommands;
        public bool EnableDeathMessages;
        public bool EnableJoinLeaveMessage;
        public bool ShowPermissionOnErrorMessage;
        public bool SaveCommandCooldowns;

        public bool EnablePollRunningMessage;
        public int PollRunningMessageCooldown;
        public int ServerFrameRate;
        public int BackDelay;

        public ushort ItemSpawnLimit;

        public AntiSpamSettings AntiSpam;
        public UpdaterSettings Updater;
        public HomeCommandSettings Home;
        public WarpCommandSettings Warp;
        public VehicleFeaturesSettings VehicleFeatures;
        public ItemFeaturesSettings ItemFeatures;
        public KitSettings Kit;
        public TpaSettings Tpa;
        public EconomySettings Economy;

        public AutoAnnouncer AutoAnnouncer;
        public AutoCommands AutoCommands;

        public HashSet<ushort> GiveItemBlacklist;
        public HashSet<ushort> VehicleBlacklist;
        public HashSet<string> EnabledSystems;
        public HashSet<string> CommandsToOverride;
        public List<string> DisabledCommands;

        internal EssConfig() {}

        public override void LoadDefaults() {
            Locale = "en";

            BackDelay = 10;
            OldFormatMessages = false;
            UnfreezeOnDeath = true;
            UnfreezeOnQuit = true;

            EnableJoinLeaveMessage = true;
            EnableTextCommands = true;
            EnableDeathMessages = true;
            ShowPermissionOnErrorMessage = true;
            SaveCommandCooldowns = false;

            EnablePollRunningMessage = true;
            PollRunningMessageCooldown = 20;
            ServerFrameRate = -1; // http://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html

            AutoAnnouncer = new AutoAnnouncer();
            AutoAnnouncer.LoadDefaults();

            AutoCommands = new AutoCommands();
            AutoCommands.LoadDefaults();

            PrivateMessage = new PrivateMessageSettings {
                FormatFrom = "(From {0}): {1}",
                FormatTo = "(To {0}): {1}",
                FormatSpy = "<gray>Spy: {0} -> {1}: {2}",
                ConsoleDisplayName = "*console*"
            };

            AntiSpam = new AntiSpamSettings {
                Enabled = true,
                Interval = 3
            };

            Home = new HomeCommandSettings {
                TeleportDelay = 5,
                CancelTeleportWhenMove = true
            };

            Warp = new WarpCommandSettings {
                TeleportDelay = 5,
                PerWarpPermission = true,
                CancelTeleportWhenMove = false
            };

            Kit = new KitSettings {
                ShowCost = true,
                ShowCostIfZero = false,
                CostFormat = "{0}({1}{2})",
                GlobalCooldown = 0,
                ResetGlobalCooldownWhenDie = false
            };

            Tpa = new TpaSettings {
                ExpireDelay = 10,
                TeleportDelay = 5,
                CancelTeleportWhenMove = true
            };

            Economy = new EconomySettings {
                UseXp = false,
                UconomyCurrency = "$",
                XpCurrency = "Xp"
            };

            VehicleFeatures = new VehicleFeaturesSettings {
                RefuelPercentage = 20,
                RepairPercentage = 70
            };

            ItemFeatures = new ItemFeaturesSettings {
                ReloadPercentage = 80,
                RepairPercentage = 80
            };

            GiveItemBlacklist = new HashSet<ushort>();
            VehicleBlacklist = new HashSet<ushort>();
            DisabledCommands = new List<string>();
            CommandsToOverride = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            EnabledSystems = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
                "kits", "warps"
            };

            ItemSpawnLimit = 10;
        }

        public override void Load(string filePath) {
            if (!File.Exists(filePath)) {
                base.Load(filePath);
                return;
            }

            // Custom config load
            try {
                var json = JObject.Parse(File.ReadAllText(filePath));

                base.Load(filePath);

                /*
                    Add missing fields...
                */
                var configFields = GetType().GetFields();
                var needUpdate = configFields.Length != json.Count;
                var nonNullFields = new Dictionary<string, object>();

                if (needUpdate) {
                    configFields.Where(f => json[f.Name] != null).ForEach(f => {
                        nonNullFields.Add(f.Name, f.GetValue(this));
                    });

                    LoadDefaults();

                    nonNullFields.ForEach(pair => {
                        GetType().GetField(pair.Key).SetValue(this, pair.Value);
                    });

                    Save(filePath);
                }

                HashSet<string> assureEqualityComparer(HashSet<string> set)
                {
                    if (set == null)
                    {
                        return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                    }
                    return new HashSet<string>(set, StringComparer.InvariantCultureIgnoreCase);
                }

                // Make sure that these HashSets use StringComparer.InvariantCultureIgnoreCase as the Comparer.
                CommandsToOverride = assureEqualityComparer(CommandsToOverride);
                EnabledSystems = assureEqualityComparer(EnabledSystems);

                // Make sure that num < max && num > min
                //  It will return the max value if num > max or
                //  the min value if num < min
                int assureRange(int num, int min, int max) => Math.Min(Math.Max(num, min), max);

                VehicleFeatures.RefuelPercentage = assureRange(VehicleFeatures.RefuelPercentage, 0, 100);
                VehicleFeatures.RepairPercentage = assureRange(VehicleFeatures.RepairPercentage, 0, 100);
                ItemFeatures.ReloadPercentage = assureRange(ItemFeatures.ReloadPercentage, 0, 100);
                ItemFeatures.RepairPercentage = assureRange(ItemFeatures.RepairPercentage, 0, 100);
            } catch (Exception ex) {
                UEssentials.Logger.LogError("Failed to load 'config.json'.");
                UEssentials.Logger.LogException(ex);
                UEssentials.Logger.LogError("Using default...");
                LoadDefaults();
            }
        }

        public struct PrivateMessageSettings {
            public string FormatFrom;
            public string FormatTo;
            public string FormatSpy;
            public string ConsoleDisplayName;
        }

        public struct AntiSpamSettings {
            public bool Enabled;
            public int Interval;
        }

        public struct UpdaterSettings {
            public bool CheckUpdates;
            public bool AlertOnJoin;
        }

        public struct HomeCommandSettings {
            public int TeleportDelay;
            public bool CancelTeleportWhenMove;
        }

        public struct WarpCommandSettings {
            public int TeleportDelay;
            public bool CancelTeleportWhenMove;
            public bool PerWarpPermission;
        }

        public struct KitSettings {
            public bool ShowCost;
            public bool ShowCostIfZero;
            public string CostFormat;
            public uint GlobalCooldown;
            public bool ResetGlobalCooldownWhenDie;
        }

        public struct TpaSettings {
            public bool CancelTeleportWhenMove;
            public int ExpireDelay;
            public int TeleportDelay;
        }

        public struct EconomySettings {
            public bool UseXp;
            public string XpCurrency;
            public string UconomyCurrency;
        }

        public struct VehicleFeaturesSettings {
            public int RefuelPercentage;
            public int RepairPercentage;
        }

        public struct ItemFeaturesSettings {
            public int ReloadPercentage;
            public int RepairPercentage;
        }

    }

}
