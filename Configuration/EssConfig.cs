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

using Essentials.Misc;
using Rocket.Core.Configuration;

namespace Essentials.Configuration
{

    public class EssConfig
    {

        public virtual string Locale { get; set; } = "en";

        public virtual PrivateMessageSettings PrivateMessage { get; set; } = new PrivateMessageSettings
        {
            FormatFrom = "(From {0}): {1}",
            FormatTo = "(To {0}): {1}",
            FormatSpy = "<gray>Spy: {0} -> {1}: {2}",
            ConsoleDisplayName = "*console*"
        };

        public virtual bool UnfreezeOnDeath { get; set; } = true;
        public virtual bool UnfreezeOnQuit { get; set; } = true;

        public virtual bool EnableTextCommands { get; set; } = true;
        public virtual bool EnableDeathMessages { get; set; } = true;
        public virtual bool EnableJoinLeaveMessage { get; set; } = true;
        public virtual bool ShowPermissionOnErrorMessage { get; set; } = true;
        public virtual bool SaveCommandCooldowns { get; set; } = false;

        public virtual bool EnablePollRunningMessage { get; set; } = true;
        public virtual int PollRunningMessageCooldown { get; set; } = 20;
        public virtual int ServerFrameRate { get; set; } = -1;
        public virtual int BackDelay { get; set; } = 10;

        public virtual ushort ItemSpawnLimit { get; set; } = 10;

        public virtual AntiSpamSettings AntiSpam { get; set; } = new AntiSpamSettings
        {
            Enabled = true,
            Interval = 3
        };

        public virtual HomeCommandSettings Home { get; set; } = new HomeCommandSettings
        {
            TeleportDelay = 5,
            CancelTeleportWhenMove = true
        };

        public virtual WarpCommandSettings Warp { get; set; } = new WarpCommandSettings
        {
            TeleportDelay = 5,
            PerWarpPermission = true,
            CancelTeleportWhenMove = false
        };

        public virtual VehicleFeaturesSettings VehicleFeatures { get; set; } = new VehicleFeaturesSettings
        {
            RefuelPercentage = 20,
            RepairPercentage = 70
        };

        public virtual ItemFeaturesSettings ItemFeatures { get; set; } = new ItemFeaturesSettings
        {
            ReloadPercentage = 80,
            RepairPercentage = 80
        };

        public virtual KitSettings Kit { get; set; } = new KitSettings
        {
            ShowCost = true,
            ShowCostIfZero = false,
            CostFormat = "{0}({1}{2})",
            GlobalCooldown = 0,
            ResetGlobalCooldownWhenDie = false
        };

        public virtual TpaSettings Tpa { get; set; } = new TpaSettings
        {
            ExpireDelay = 10,
            TeleportDelay = 5,
            CancelTeleportWhenMove = true
        };

        public virtual EconomySettings Economy { get; set; } = new EconomySettings
        {
            UseXp = false,
            UconomyCurrency = "$",
            XpCurrency = "Xp"
        };

        public virtual AutoAnnouncer AutoAnnouncer { get; set; } = new AutoAnnouncer();
        public virtual AutoCommands AutoCommands { get; set; } = new AutoCommands();

        [ConfigArray(ElementName = "ItemId")]
        public virtual ushort[] GiveItemBlacklist { get; set; } = new ushort[0];
        [ConfigArray(ElementName = "VehicleId")]
        public virtual ushort[] VehicleBlacklist { get; set; } = new ushort[0];

        [ConfigArray(ElementName = "System")]
        public virtual string[] EnabledSystems { get; set; } = { "kits", "warps" };
        public class PrivateMessageSettings
        {
            public virtual string FormatFrom { get; set; }
            public virtual string FormatTo { get; set; }
            public virtual string FormatSpy { get; set; }
            public virtual string ConsoleDisplayName { get; set; }
        }

        public class AntiSpamSettings
        {
            public virtual bool Enabled { get; set; }
            public virtual int Interval { get; set; }
        }

        public class HomeCommandSettings
        {
            public virtual int TeleportDelay { get; set; }
            public virtual bool CancelTeleportWhenMove { get; set; }
        }

        public class WarpCommandSettings
        {
            public virtual int TeleportDelay { get; set; }
            public virtual bool CancelTeleportWhenMove { get; set; }
            public virtual bool PerWarpPermission { get; set; }
        }

        public class KitSettings
        {
            public virtual bool ShowCost { get; set; }
            public virtual bool ShowCostIfZero { get; set; }
            public virtual string CostFormat { get; set; }
            public virtual uint GlobalCooldown { get; set; }
            public virtual bool ResetGlobalCooldownWhenDie { get; set; }
        }

        public class TpaSettings
        {
            public virtual bool CancelTeleportWhenMove { get; set; }
            public virtual int ExpireDelay { get; set; }
            public virtual int TeleportDelay { get; set; }
        }

        public class EconomySettings
        {
            public virtual bool UseXp { get; set; }
            public virtual string XpCurrency { get; set; }
            public virtual string UconomyCurrency { get; set; }
        }

        public class VehicleFeaturesSettings
        {
            public virtual int RefuelPercentage { get; set; }
            public virtual int RepairPercentage { get; set; }
        }

        public class ItemFeaturesSettings
        {
            public virtual int ReloadPercentage { get; set; }
            public virtual int RepairPercentage { get; set; }
        }

    }

}
