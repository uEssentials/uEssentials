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
using System.Globalization;
using System.IO;
using System.Linq;
using Essentials.Api;
using Essentials.Api.Event;
using Essentials.Api.Logging;
using Essentials.Api.Module;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Commands;
using Essentials.Common;
using Essentials.Configuration;
using Essentials.Core.Command;
using Essentials.Core.Event;
using Essentials.I18n;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Plugins;
using Environment = Rocket.Core.Environment;
using Essentials.Common.Reflect;
using Essentials.Compatibility;
using Essentials.Core.Permission;
using Essentials.Event.Handling;
using Essentials.NativeModules;
using Essentials.NativeModules.Kit.Commands;
using Essentials.Updater;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace Essentials.Core
{
    public sealed class EssCore : RocketPlugin
    {
       /*
            TODO:
                - AFK KICK
                - Improve event system to avoid duplicate events.
        */
        
        internal const string                         PLUGIN_VERSION              = "1.1.7.0";
        internal const string                         ROCKET_VERSION              = "4.9.3.1";
        internal const string                         UNTURNED_VERSION            = "3.14.10.3";
        
        internal static EssCore                       Instance                    { get; set; }
        
        internal ModuleManager                        ModuleManager               { get; set; }
        internal CommandManager                       CommandManager              { get; set; }
        internal IEventManager                        EventManager                { get; set; }
        internal HookManager                          HookManager                 { get; set; }
        internal IUpdater                             Updater                     { get; set; }

        internal CommandsConfig                       CommandsConfig              { get; set; }
        internal EssConfig                            Config                      { get; set; }
        internal EssLogger                            Logger                      { get; set; }

        internal HashSet<UPlayer>                     ConnectedPlayers            { get; set; }

        private string _folder;
        private string _translationFolder;
        private string _dataFolder;
        private string _modulesFolder;

        internal string Folder
        {
            get
            {
                if ( !System.IO.Directory.Exists( _folder ) )
                {
                    System.IO.Directory.CreateDirectory( _folder );
                }
                return _folder;
            }
        }

        internal string TranslationFolder
        {
            get
            {
                if ( !System.IO.Directory.Exists( _translationFolder ) )
                {
                    System.IO.Directory.CreateDirectory( _translationFolder );
                }
                return _translationFolder;
            }
        }

        internal string DataFolder
        {
            get
            {
                if ( !System.IO.Directory.Exists( _dataFolder ) )
                {
                    System.IO.Directory.CreateDirectory( _dataFolder );
                }
                return _dataFolder;
            }
        }

        internal string ModulesFolder
        {
            get
            {
                if ( !System.IO.Directory.Exists( _modulesFolder ) )
                {
                    System.IO.Directory.CreateDirectory( _modulesFolder );
                }
                return _modulesFolder;
            }
        }

        protected override void Load()
        {
            Instance = this;

            R.Permissions = new EssentialsPermissionsProvider();

            Provider.onServerDisconnected += PlayerDisconnectCallback;
            Provider.onServerConnected += PlayerConnectCallback;

            Logger = new EssLogger( "[uEssentials] " );
            ConnectedPlayers = new HashSet<UPlayer>();

            if ( Provider.Players.Count > 0 )
            {
                Provider.Players.ForEach( p => {
                    ConnectedPlayers.Add( new UPlayer( UnturnedPlayer.FromSteamPlayer( p ) ) );
                } );
            }

            _folder = Environment.PluginsDirectory + "/uEssentials/";
            _translationFolder = Folder + "translations/";
            _dataFolder = Folder + "data/";
            _modulesFolder = Folder + "modules/";

            (
                from directory in new[] { _folder, _translationFolder, _dataFolder, _modulesFolder }
                where !System.IO.Directory.Exists( directory )
                select directory
            ).ForEach( dir => System.IO.Directory.CreateDirectory( dir ) );

            var configPath = $"{Folder}config.json";

            Config = new EssConfig();
            Config.Load( configPath );
            
            if ( Config.WebConfig.Enabled )
            {
                Config = new EssWebConfig();
                Config.Load( configPath );
            }

            CommandsConfig = new CommandsConfig();
            CommandsConfig.Load( $"{Folder}commands.json" );

            EventManager = new EventManager();
            CommandManager = new CommandManager();
            ModuleManager = new ModuleManager();
            Updater = new GithubUpdater();
            HookManager = new HookManager();

            EssLang.Load();

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

            EventManager.RegisterAll( GetType().Assembly );

            if ( !Config.EnableJoinLeaveMessage )
            {
                EventManager.Unregister<EssentialsEventHandler>( "JoinMessage" );
                EventManager.Unregister<EssentialsEventHandler>( "LeaveMessage" );
            }

            CommandManager.RegisterAll( "Essentials.Commands" );

            HookManager.RegisterAll();
            HookManager.LoadAll();

            /*
                Load native modules
            */
            (
                from type in Assembly.GetTypes()
                where typeof(NativeModule).IsAssignableFrom( type )
                where !type.IsAbstract
                let mAttr = (ModuleInfo) type.GetCustomAttributes( typeof(ModuleInfo), false )[0]
                where Config.EnabledSystems.Any( s => s.Equals(mAttr.Name, StringComparison.OrdinalIgnoreCase) )
                select type
             ).ForEach( type => {
                 ModuleManager.LoadModule( (NativeModule) Activator.CreateInstance( type ) );
             } );

            Logger.LogInfo( $"Loaded {CommandManager.Commands.Count()} commands" );

            Logger.LogInfo( "Loading modules..." );
            ModuleManager.LoadAll( ModulesFolder );
            Logger.LogInfo( $"Loaded {ModuleManager.RunningModules.Count( t => !(t is NativeModule) )} modules" );

            if ( Config.AutoAnnouncer.Enabled )
            {
                Config.AutoAnnouncer.Start();
            }

            if ( Config.AutoCommands.Enabled )
            {
                Config.AutoCommands.Start();
            }

            if ( !Config.Updater.AlertOnJoin )
            {
                EventManager.Unregister<EssentialsEventHandler>( "UpdaterAlert" );
            }
            
            if ( Config.ServerFrameRate != -1 )
            {
                var frameRate = Config.ServerFrameRate;
                
                if ( Config.ServerFrameRate < -1 )
                {
                    frameRate = -1; // Default    
                }
                
                UnityEngine.Application.targetFrameRate = frameRate;
            }

            if ( Config.DisabledCommands.Count != 0 )
            {
                Config.DisabledCommands.ForEach( cmdName => {
                    var command = CommandManager.GetByName( cmdName );

                    if ( command == null || command is CommandEssentials )
                    {
                        Logger.LogWarning( $"There is no command named '{cmdName}' to disable." );
                    }
                    else
                    {
                        CommandManager.Unregister( command );
                        Logger.LogInfo( $"Disabled command: '{command.Name}'" );

                        if ( command is CommandBack )
                        {
                            EventManager.Unregister<EssentialsEventHandler>( "BackPlayerDeath" );
                        }
                        else if ( command is CommandKit )
                        {
                            EventManager.Unregister<EssentialsEventHandler>( "KitPlayerDeath" );
                        }
                        else if ( command is CommandHome )
                        {
                            EventManager.Unregister<EssentialsEventHandler>( "HomePlayerMove" );
                        }
                    }
                } );
            }

#if DEV
            CommandWindow.ConsoleOutput.title = "Unturned Server";
#else
            if ( Config.Updater.CheckUpdates )
            {
                new System.Threading.Thread( () =>
                {
                    Logger.LogInfo( "Checking updates." );

                    var isUpdated = Updater.IsUpdated();
                    var lastResult = Updater.LastResult;

                    if ( isUpdated )
                    {
                        Logger.LogInfo( "Plugin is up-to-date!" );
                        return;
                    }

                    Logger.LogInfo( $"New version avalaible: {lastResult.LatestVersion}" );

                    if ( !lastResult.AdditionalData.IsNullOrEmpty() )
                    {
                        Newtonsoft.Json.Linq.JToken changesStr;
                        if ( Newtonsoft.Json.Linq.JObject.Parse( lastResult.AdditionalData ).TryGetValue( "changes", out changesStr ) )
                        {
                            Logger.LogInfo( "========================= [ Changes ] =========================" );

                            changesStr.ToString().Split( '\n' ).ForEach( msg =>
                            {
                                Logger.Log( "", ConsoleColor.Green, suffix: "" );
                                Logger.Log( "  " + msg, ConsoleColor.White, "" );
                            } );

                            Logger.LogInfo( "" );
                            Logger.Log( "", ConsoleColor.Green, suffix: "" );
                            Logger.Log( "  " +  $"See more in: https://github.com/uEssentials/uEssentials/releases/tag/{lastResult.LatestVersion}", ConsoleColor.White, "" );

                            Logger.LogInfo( "===============================================================" );
                        }
                    }

                    if ( Config.Updater.DownloadLatest )
                    {
                        Updater.DownloadLatestRelease( $"{Folder}/updates/" );
                    }
                } ).Start();
            }

            Analytics.Metrics.Init();
#endif

            TryAddComponent<Tasks>();

            Tasks.New( t => {
                File.Delete( $"{Folder}uEssentials.en.translation.xml" );
                File.Delete( $"{Folder}uEssentials.configuration.xml" );
            } ).Delay( 100 ).Go();

            Tasks.New( t => {
                CheckRocketCommands();
            } ).Delay( 500 ).Go();

            CommandWindow.ConsoleInput.onInputText += ReloadCallback;
        }

        protected override void Unload()
        {
            CommandWindow.ConsoleInput.onInputText -= ReloadCallback;
            Provider.onServerDisconnected -= PlayerDisconnectCallback;
            Provider.onServerConnected -= PlayerConnectCallback;

            TryRemoveComponent<Tasks>();

            var executingAssembly = GetType().Assembly;
            
            HookManager.UnloadAll();
            HookManager.UnregisterAll();
            CommandManager.UnregisterAll( executingAssembly );
            EventManager.UnregisterAll( executingAssembly ); 
            ModuleManager.UnloadAll();

            Tasks.CancelAll();
        }

        private static void ReloadCallback( string command )
        {
            if ( !command.StartsWith( "rocket reload", true, CultureInfo.InvariantCulture ) )
            {
                return;
            }

            Console.WriteLine();
            EssProvider.Logger.LogError( "Rocket reload cause many issues, consider restart the server" );
            EssProvider.Logger.LogError( "Or use '/essentials reload' to reload essentials correctly." );
            Console.WriteLine();
        }

        private static void PlayerConnectCallback( CSteamID id )
        {
            Instance.ConnectedPlayers.Add( new UPlayer( UnturnedPlayer.FromCSteamID( id ) ) );
        }

        private static void PlayerDisconnectCallback( CSteamID id )
        {
            Instance.ConnectedPlayers.RemoveWhere( connectedPlayer => connectedPlayer.CSteamId == id );
        }

        private static void CheckRocketCommands()
        {
            var rocketCommands = AccessorFactory.AccessField<List<IRocketCommand>>( R.Commands, "commands" );
            var logger = EssProvider.Logger;

            logger.LogWarning( "Searching commands that conflict with uEssentials commands." );

            var count = rocketCommands.Value.RemoveAll( cmd => {
                if ( EssProvider.CommandManager.GetByName( cmd.Name ) != null )
                {
                    logger.LogWarning( $"Found '{cmd.GetType()} ({cmd.Name})'. Disabling it..." );

                    return true;
                }
                return false;
            } );

            if ( count == 0 )
            {
                logger.LogWarning( "Nothing found!" );
                return;
            }

            logger.LogWarning( $"Disabled {count} commands from another's plugins." );
            logger.LogWarning( "if you prefer use another command instead of Essentials command, disable it in configuration." );
        }
    }
}
