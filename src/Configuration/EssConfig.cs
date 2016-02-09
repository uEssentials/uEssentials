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

using Essentials.Api.Configuration;
using Essentials.Misc;
using Newtonsoft.Json;

namespace Essentials.Configuration
{
    public class EssConfig : JsonConfig
    {
        public string           Locale;
        
        public string           PrivateMessageFormat;
        
        public bool             UnfreezeOnDeath;
        public bool             UnfreezeOnQuit;
        
        public bool             EnableUnknownMessage;
        
        public bool             AdminBypassWarpCooldown;
        public bool             PerWarpPermission;
        public int              WarpCooldown;
        
        public bool             EnablePollRunningMessage;
        public int              PollRunningMessageCooldown;

        public AntiSpam         AntiSpam;
        public Updater          Updater;

        public AutoAnnouncer    AutoAnnouncer;

        internal EssConfig() { }

        public override void LoadDefaults()
        {
            Locale                      = "en";

            PrivateMessageFormat        = "(From {0}): {1}";

            UnfreezeOnDeath             = true;
            UnfreezeOnQuit              = true;

            AdminBypassWarpCooldown     = true;
            PerWarpPermission           = true;
            WarpCooldown                = 5;

            EnableUnknownMessage        = true;
            
            EnablePollRunningMessage    = true;
            PollRunningMessageCooldown  = 20;

            AutoAnnouncer               = new AutoAnnouncer();
            AutoAnnouncer               .LoadDefaults();

            AntiSpam                    = new AntiSpam { Enabled = true,
                                                         Interval = 3 };

            Updater                     = new Updater { CheckUpdates = true,
                                                        DownloadLatest = true };
        }
    }

    [JsonObject]
    public struct AntiSpam
    {
        public bool Enabled;
        public int Interval;
    }

    [JsonObject]
    public struct Updater
    {
        public bool CheckUpdates;
        public bool DownloadLatest;
    }
}
