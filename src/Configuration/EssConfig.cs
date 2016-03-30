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
using System.Linq;
using Essentials.Api;
using Essentials.Api.Configuration;
using Essentials.Common;
using Essentials.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Essentials.Configuration
{
    public class EssConfig : JsonConfig
    {
        public string           Locale;
        
        public string           PrivateMessageFormat;
        public string           PrivateMessageFormat2;
        
        public bool             UnfreezeOnDeath;
        public bool             UnfreezeOnQuit;
        
        public bool             EnableUnknownMessage;
        
        public bool             AdminBypassWarpCooldown;
        public bool             PerWarpPermission;
        public int              WarpCooldown;

        public bool             EnableJoinLeaveMessage;
        
        public bool             EnablePollRunningMessage;
        public int              PollRunningMessageCooldown;
        public int              ServerFrameRate;

        public ushort           ItemSpawnLimit;

        public AntiSpam         AntiSpam;
        public Updater          Updater;
        public HomeCommand      HomeCommand;
        public WebKits          WebKits;
        public WebConfig        WebConfig;
        public Kit              Kit;

        public AutoAnnouncer    AutoAnnouncer;
        public AutoCommands     AutoCommands;

        public List<string>     EnabledSystems; 
        public List<string>     DisabledCommands; 

        internal EssConfig() { }

        public override void LoadDefaults()
        {
            Locale                      = "en";

            PrivateMessageFormat        = "(From {0}): {1}";
            PrivateMessageFormat2       = "(To {0}): {1}";

            UnfreezeOnDeath             = true;
            UnfreezeOnQuit              = true;

            AdminBypassWarpCooldown     = true;
            PerWarpPermission           = true;
            WarpCooldown                = 5;

            EnableJoinLeaveMessage      = true;

            EnableUnknownMessage        = true;
            
            EnablePollRunningMessage    = true;
            PollRunningMessageCooldown  = 20;
            ServerFrameRate             = -1; // http://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html

            AutoAnnouncer               = new AutoAnnouncer();
            AutoAnnouncer               .LoadDefaults();

            AutoCommands                = new AutoCommands();
            AutoCommands                .LoadDefaults();

            AntiSpam                    = new AntiSpam { Enabled = true,
                                                         Interval = 3 };

            Updater                     = new Updater { CheckUpdates = true,
                                                        DownloadLatest = true,
                                                        AlertOnJoin = true };

            HomeCommand                 = new HomeCommand { Cooldown = 30, Delay = 5,
                                                            CancelWhenMove = true };

            WebKits                     = new WebKits { Enabled = false, Url = "" };
            
            WebConfig                   = new WebConfig { Enabled = false, Url = "" };

            Kit                         = new Kit { ShowCost = true, ShowCostIfZero = false, CostFormat = "{0}({1}$)" };

            DisabledCommands            = new List<string>();
            EnabledSystems              = new List<string> { "kits", "warps" };

            ItemSpawnLimit              = 10;
        }

        public override void Load( string filePath )
        {
            if ( !File.Exists( filePath ) )
            {
                base.Load( filePath );
            }

            try
            {
                var json = JObject.Parse( File.ReadAllText( filePath ) );
                base.Load( filePath );

                var configFiels = GetType().GetFields();
                var needUpdate = configFiels.Length != json.Count;
                var nonNullFields = new Dictionary<string, object>();

                if ( needUpdate )
                {
                    configFiels.Where( f => json[f.Name] != null ).ForEach( f =>
                    {
                        nonNullFields.Add( f.Name, f.GetValue( this ) );
                    } );

                    LoadDefaults();

                    nonNullFields.ForEach( pair =>
                    {
                        GetType().GetField( pair.Key ).SetValue( this, pair.Value );
                    } );

                    Save( filePath );
                }

                if ( json["Updater"]["AlertOnJoin"] == null )
                {
                    Updater.AlertOnJoin = true;
                    Save( filePath );
                }
            }
            catch (Exception ex)
            {
                EssProvider.Logger.LogError( $"Invalid configuration ({filePath})" );
                EssProvider.Logger.LogError( ex.Message );
            }
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
        public bool AlertOnJoin;
    }

    [JsonObject]
    public struct HomeCommand
    {
        public int Cooldown;
        public int Delay;
        public bool CancelWhenMove;
    }

    [JsonObject]
    public struct WebKits
    {
        public string Url;
        public bool Enabled;
    }
    
    [JsonObject]
    public struct WebConfig
    {
        public string Url;
        public bool Enabled;
    }

    [JsonObject]
    public struct Kit
    {
        public bool ShowCost;
        public bool ShowCostIfZero;
        public string CostFormat;
    }
}