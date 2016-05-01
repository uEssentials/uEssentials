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
using System.Reflection;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Configuration;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core.Command;
using Newtonsoft.Json;

namespace Essentials.Configuration
{
    public class CommandsConfig : JsonConfig
    {
        public IDictionary<string, Command> Commands { get; private set; } = new Dictionary<string, Command>();

        public override void Save( string filePath )
        {
            File.WriteAllText( filePath, string.Empty );
            JsonUtil.Serialize( filePath, Commands );
        }

        public override void Load( string filePath )
        {
            if ( File.Exists( filePath ) )
            {
                JsonConvert.PopulateObject( File.ReadAllText( filePath ), Commands );

                var allCommands = FindAllCommands();

                /*
                    Add new commands, if necessary.
                */
                if ( Commands.Count != allCommands.Count )
                {
                    var keys = new List<string>( allCommands.Keys ); // Avoid out of sync.

                    keys.ForEach( k  => {
                        if ( Commands.ContainsKey( k ) )
                        {
                            allCommands[k] = Commands[k]; // Update existing command configurations.
                        }   
                    } );

                    Commands = allCommands;
                    Save( filePath );
                }
            }
            else
            {
                LoadDefaults();
                Save( filePath );
            }
        }

        public override void LoadDefaults()
        {
            Commands = FindAllCommands();
        }

        /*
            Search for all commands in uEssentials assembly.
        */
        private IDictionary<string, Command> FindAllCommands()
        {
            var asm = GetType().Assembly;
            var commands = new Dictionary<string, Command>();

            Predicate<Type> defaultPredicate = type => {
                return ( typeof( ICommand ).IsAssignableFrom( type ) 
                         && !type.IsAbstract && type != typeof( MethodCommand )
                         && type != typeof( TextCommand ) );
            };

            /*
                Search classes
            */
            (
                from type in asm.GetTypes()
                where defaultPredicate( type )
                select (ICommand) Activator.CreateInstance( type )
            ).ForEach( c => commands.Add( c.Name, new Command {
                Aliases = c.Aliases
            } ) );

            /*
                Search methods
            */
            Action<MethodInfo> invalidate = md => {
                UEssentials.Logger.LogError( $"Invalid method signature in '{md}'. " +
                                             "Expected 'CommandResult methodName(ICommandSource, ICommandArgs)'" );
            };

            foreach ( var type in asm.GetTypes() )
            {
                foreach ( var method in type.GetMethods( (BindingFlags) 0x3C ) )
                {
                    if ( ReflectionUtil.GetAttributeFrom<CommandInfo>( method ) == null )
                        continue;

                    var paramz = method.GetParameters();

                    if ( method.ReturnType != typeof( CommandResult ) )
                    {
                        invalidate( method );
                        continue;
                    }

                    if ( paramz.Length >= 2 &&
                        (paramz[0].ParameterType != typeof( ICommandSource ) ||
                        paramz[1].ParameterType != typeof( ICommandArgs )) )
                    {
                        if ( paramz.Length == 3 &&
                            (paramz[0].ParameterType != typeof( ICommandSource ) ||
                             paramz[1].ParameterType != typeof( ICommandArgs ) ||
                             paramz[2].ParameterType != typeof( ICommand )) )
                        {
                            invalidate( method );
                            continue;
                        }

                        invalidate( method );
                        continue;
                    }

                    var cmdInfo = ReflectionUtil.GetAttributeFrom<CommandInfo>( method );

                    commands.Add( cmdInfo.Name, new Command {
                        Aliases = cmdInfo.Aliases
                    } );
                }
            }

            return commands;
        }
    }

    [JsonObject]
    public struct Command
    {
        public string[] Aliases;
        public decimal Cost;
    }
}
