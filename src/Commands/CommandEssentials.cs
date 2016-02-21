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
using System.Linq;
using System.Text;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Configuration;
using Essentials.Core;
using Essentials.Core.Command;
using Essentials.I18n;
using Essentials.Kit;
using Rocket.Core;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "essentials",
        Description = "Plugin commands",
        Aliases = new[] { "ess", "?", "uessentials" }
    )]
    public class CommandEssentials : EssCommand
    {
        private string _cachedCommands;

        private static void ReloadKits()
        {
            var core = EssCore.Instance;
            core.KitManager = new KitManager();
            core.KitManager.Load();
        }

        private static void ReloadLang()
        {
            EssLang.Load();
        }

        private static void ReloadConfig()
        {
            var core = EssCore.Instance;
            var configPath = $"{core.Folder}config.json";

            core.Config = new EssConfig();
            core.Config.Load( configPath );
        }

        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.IsEmpty )
            {
                src.SendMessage( "Use /ess <commands/help/info/reload>" );
                return;
            }

            switch ( args[0].ToLowerString )
            {
                case "reload":
                    if ( !src.HasPermission( "essentials.reload" ) )
                    {
                        EssLang.COMMAND_NO_PERMISSION.SendTo( src );
                        return;
                    }
                    if ( args.Length == 1 )
                    {
                        src.SendMessage( "Reloading all..." );
                        ReloadConfig();
                        ReloadKits();
                        ReloadLang();
                        R.Permissions.Reload();
                        src.SendMessage( "Reload finished..." );
                    }
                    else
                    {
                        switch ( args[1].ToLowerString )
                        {
                            case "kits":
                            case "kit":
                                src.SendMessage( "Reloading kits..." );
                                ReloadKits();
                                src.SendMessage( "Reload finished..." );
                                break;
                            
                            case "config":
                                src.SendMessage( "Reloading config..." );
                                ReloadConfig();
                                src.SendMessage( "Reload finished..." );
                                break;

                            case "lang":
                                src.SendMessage( "Reloading translations..." );
                                ReloadLang();
                                src.SendMessage( "Reload finished..." );
                                break;
                            
                            default:
                                src.SendMessage( "Use /ess reload <kits/config/lang>" );
                                break;
                        }
                    }
                    break;
                
                case "commands":
                    if ( src.IsConsole )
                    {
                        if ( _cachedCommands == null )
                        {
                            var builder = new StringBuilder( "Commands: \n" );

                            Func<ICommand, bool> isEssentialsCommand = cmd =>
                            {
                                var asm = cmd.GetType().Assembly;

                                if ( cmd.GetType() == typeof (MethodCommand) )
                                {
                                    asm = ((MethodCommand) cmd).Owner.Assembly;
                                }

                                return asm.Equals( typeof(EssCore).Assembly );
                            };

                            EssProvider.CommandManager.Commands
                                .Where( isEssentialsCommand )
                                .ForEach( command => {
                                    builder.Append( "  /" )
                                            .Append( command.Name.ToLower() )
                                            .Append( command.Usage == "" ? "" : " " + command.Usage )
                                            .Append( " - " ).Append( command.Description )
                                            .AppendLine();
                                } );

                            _cachedCommands = builder.ToString();
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine( _cachedCommands );
                        Console.WriteLine( "Use /ess help <command> to view help page." );
                    }
                    else
                    {
                        EssLang.PLAYER_CANNOT_EXECUTE.SendTo( src );
                    }
                    break;

                case "info":
                    if ( src.IsConsole )
                        Console.ForegroundColor = ConsoleColor.Green;

                    src.SendMessage( "Author:  leonardosc", Color.yellow );
                    src.SendMessage( "Skype:   devleeo", Color.yellow );
                    src.SendMessage( "Github:  github.com/leonardosnt", Color.yellow );
                    src.SendMessage( "Version: " + EssCore.PLUGIN_VERSION, Color.yellow );
                    break;

                case "help":
                    if ( src.IsConsole )
                        Console.ForegroundColor = ConsoleColor.Green;

                    if ( args.Length == 1 )
                    {
                        src.SendMessage( "Use /ess help <command>" );
                    }
                    else
                    {
                        var command = EssProvider.CommandManager.GetByName( args[1].ToString() );

                        if ( command == null )
                        {
                            src.SendMessage( $"Command {args[1]} does not exists", Color.red );
                        }
                        else
                        {
                            src.SendMessage( "Command: " + command.Name , Color.cyan);
                            src.SendMessage( "  Arguments: <optional> [required]", Color.cyan );
                            src.SendMessage( "  Help: " + command.Description, Color.cyan );
                            src.SendMessage( "  Usage: /" + command.Name + " " + command.Usage, Color.cyan );
                            if ( command.Aliases.Any() )
                                src.SendMessage( "  Aliases: " + string.Join( ", ", command.Aliases ), Color.cyan );
                        }
                    }
                    break;

                default:
                    src.SendMessage( "Use /ess <commands/help/info>" );
                    break;
            }
        }
    }
}
