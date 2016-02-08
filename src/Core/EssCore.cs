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
using Essentials.Api.Event;
using Essentials.Api.Logging;
using Essentials.Api.Module;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Configuration;
using Essentials.Core.Command;
using Essentials.Core.Event;
using Essentials.I18n;
using Essentials.Kits;
using Essentials.Warps;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using Environment = Rocket.Core.Environment;
using Essentials.Common.Reflect;

// ReSharper disable InconsistentNaming

namespace Essentials.Core
{
    public sealed class EssCore : RocketPlugin
    {
       /*
            PERMISSIONS:
                - essentials.kit.<kit_name>
                - essentials.kit.<kit_name>.other // allow to give kit to another player
                - essentials.kits.bypasscooldown
                - essentials.warp.<warp_name>
                - essentials.warps.bypasscooldown
                - essentials.back
                - essentials.spy
                - essentials.antispam.bypass
                - essentials.command.poll.start
                - essentials.command.poll.stop
                - essentials.command.poll.list
                - essentials.command.poll.info
                - essentials.keepskill.<skill>
        */
        
        internal const string                         PLUGIN_VERSION              = "1.0.2.0";
        internal const string                         ROCKET_VERSION              = "4.9.0.0";
        internal const string                         UNTURNED_VERSION            = "3.14.3.1";
        
        internal static EssCore                       Instance                    { get; private set; }
        
        internal WarpManager                          WarpManager                 { get; private set; }
        internal KitManager                           KitManager                  { get; private set; }
        internal ModuleManager                        ModuleManager               { get; private set; }
        internal CommandManager                       CommandManager              { get; private set; }
        internal IEventManager                        EventManager                { get; private set; }

        internal EssConfig                            Config                      { get; private set; }
        internal EssLogger                            Logger                      { get; private set; }

        internal string                               Folder                      { get; private set; }
        internal string                               TranslationsFolder          { get; private set; }
        internal string                               DataFolder                  { get; private set; }
        internal string                               ModulesFolder               { get; private set; }

        internal HashSet<UPlayer>                     ConnectedPlayers            { get; private set; }

        protected override void Load()
        {
            Instance = this;

            Logger = new EssLogger( "[uEssentials] " );
            ConnectedPlayers = new HashSet<UPlayer>();

            U.Events.OnPlayerConnected += player =>
            {
                ConnectedPlayers.Add( new UPlayer( player ) );
            };

            U.Events.OnPlayerDisconnected += player =>
            {
                ConnectedPlayers.RemoveWhere( connectedPlayer => connectedPlayer.RocketPlayer == player );
            };

            Folder = Environment.PluginsDirectory + "/uEssentials/";
            TranslationsFolder = Folder + "translations/";
            DataFolder = Folder + "data/";
            ModulesFolder = Folder + "modules/";

            if ( !System.IO.Directory.Exists( Folder ) )
                System.IO.Directory.CreateDirectory( Folder );

            if ( !System.IO.Directory.Exists( TranslationsFolder ) )
                System.IO.Directory.CreateDirectory( TranslationsFolder );

            if ( !System.IO.Directory.Exists( DataFolder ) )
                System.IO.Directory.CreateDirectory( DataFolder );
                
            if ( !System.IO.Directory.Exists( ModulesFolder ) )
                System.IO.Directory.CreateDirectory( ModulesFolder );

            var configPath = $"{Folder}config.json";

            Config = new EssConfig();
            Config.Load( configPath );

            if (  Config.AutoAnnouncer.Enabled )
                Config.AutoAnnouncer.Start();

            EssLang.Load();

            EventManager = new EventManager();
            EventManager.RegisterAll( GetType().Assembly );

            Logger.Log( "Loaded uEssentials", ConsoleColor.Green );

            Logger.Log( "Plugin version: ", ConsoleColor.Green, suffix: "" );
            Logger.Log( PLUGIN_VERSION, ConsoleColor.White, "" );

            Logger.Log( "Recommended Rocket version: ", ConsoleColor.Green, suffix: "" );
            Logger.Log( ROCKET_VERSION, ConsoleColor.White, "" );

            Logger.Log( "Recommended Unturned version: ", ConsoleColor.Green, suffix: "" );
            Logger.Log( UNTURNED_VERSION, ConsoleColor.White, "" );

            Logger.Log( "Author: ", ConsoleColor.Green, suffix: "" );
            Logger.Log( "leonardosc", ConsoleColor.White, "" );

            Logger.Log( "Wiki: ", ConsoleColor.Green, suffix: "" );
            Logger.Log( "uessentials.github.io", ConsoleColor.White, "" );

            UnregisterRocketCommand<CommandTp>();

            CommandManager = new CommandManager();
            CommandManager.RegisterAll( GetType().Assembly );
            Logger.LogInfo( $"Loaded {CommandManager.Commands.Count()} commands" );

            WarpManager = new WarpManager();
            WarpManager.Load();
            Logger.LogInfo( $"Loaded {WarpManager.Count} warps" );

            KitManager = new KitManager();
            KitManager.Load();
            Logger.LogInfo( $"Loaded {KitManager.Count} kits" );

            Logger.LogInfo( "Loading modules..." );
            ModuleManager = new ModuleManager();
            ModuleManager.LoadAll( ModulesFolder );
            Logger.LogInfo( $"Loaded {ModuleManager.RunningModules.Count} modules" );

            #if DEV
            SDG.Unturned.CommandWindow.ConsoleOutput.title = "Unturned Server";
            #endif

            #if !DEV
            Essentials.Analytics.Metrics.Init();
            #endif

            TryAddComponent<Tasks>();

            Tasks.New( t => {
                WarpManager.Save();
            }).Delay( 60 * 1000 ).Interval( 60 * 1000 ).Go();

            Tasks.New( t => {
                File.Delete( $"{Folder}uEssentials.en.translation.xml" );
                File.Delete( $"{Folder}uEssentials.configuration.xml" );
            } ).Delay( 100 ).Go();
        }


        protected override void Unload()
        {
            WarpManager.Save();
            var executingAssembly = GetType().Assembly;
            
            CommandManager.UnregisterAll( executingAssembly );
            EventManager.UnregisterAll( executingAssembly ); 
            ModuleManager.UnloadAll();

            Tasks.CancelAll();
        }

        private static void UnregisterRocketCommand<T>() where T : IRocketCommand
        {
            var rocketCommands = AccessorFactory.AccessField<List<IRocketCommand>>( R.Commands, "commands" );

            rocketCommands.Value.RemoveAll( cmd => cmd is T );
        }
    }
}
