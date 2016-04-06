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
using System.Linq;
using System.Reflection;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Common.Reflect;
using Essentials.Common.Util;
using Rocket.API;
using Rocket.Core;
using SDG.Unturned;

namespace Essentials.Core.Command
{
    internal class CommandManager : ICommandManager
    {
        private Dictionary<string, ICommand> CommandMap { get; }
        private List<IRocketCommand> _rocketCommands;

        public IEnumerable<ICommand> Commands => CommandMap.Values; 

        internal CommandManager()
        {
            CommandMap = new Dictionary<string, ICommand>();
            _rocketCommands = AccessorFactory.AccessField<List<IRocketCommand>>( R.Commands, "commands" ).Value;
        }

        public ICommand GetByName( string name, bool includeAliases = true )
        {
            Preconditions.NotNull( name, "name cannot be null" );

            return GetWhere( command => 
            {
                if ( command is CommandAdapter && !includeAliases ) return false;

                return command.Name.EqualsIgnoreCase( name );
            } );
        }

        public ICommand GetByType( Type commandType )
        {
            return GetWhere( command => command.Command.GetType() == commandType );
        }

        public TCommandType GetByType<TCommandType>() where TCommandType : ICommand
        {
            return (TCommandType) GetByType( typeof (TCommandType) );
        }

        public void Register( ICommand command )
        {
            Preconditions.NotNull( command, "Command cannot be null" );
            Preconditions.NotNull( command.Name, "Command name cannot be null" );

            var name = command.Name.ToLowerInvariant();

            if ( CommandMap.ContainsKey( name ) )
            {
                EssProvider.Logger.LogError( $"Could not register '{command.GetType().Name}' because there is already a command called '{name}'" );
                return;
            }

            var configCommnads = EssCore.Instance.CommandsConfig.Commands;

            if ( configCommnads.ContainsKey( command.Name ) )
            {
                command.Aliases = configCommnads[command.Name].Aliases ?? new string[0];
            }

            var adapter = new CommandAdapter( command );

            _rocketCommands.Add( adapter );
            CommandMap.Add( name, command );
            
            if ( command is EssCommand )
            {
                AccessorFactory.AccessMethod<EssCommand>( command, "OnRegistered" ).Invoke();
            }
            
            var aliases = command.Aliases;

            if ( aliases == null || aliases.Length == 0 ) return;

            foreach ( var alias in aliases )
            {
                _rocketCommands.Add( new CommandAdapter.CommandAliasAdapter( command, alias ) );
            }
        }

        public void Register<TCommandType>() where TCommandType : ICommand
        {
            Register( Activator.CreateInstance<TCommandType>() );
        }

        public void Register( Func<ICommandSource, ICommandArgs, CommandResult> method )
        {
            Preconditions.NotNull( method, "method cannot be null" );

            Register( new MethodCommand( method ) );
        }

                public void Register( Func<ICommandSource, ICommandArgs, ICommand, CommandResult> method )
        {
            Preconditions.NotNull( method, "method cannot be null" );

            Register( new MethodCommand( method ) );
        }

        public void  RegisterAll( Assembly assembly )
        {
            Preconditions.NotNull( assembly, "Assembly cannot be null" );

            RegisterAllWhere( assembly, type => true );
        }

        public void RegisterAll( string targetNamespace )
        {
            RegisterAllWhere(
                GetType().Assembly,
                type => type.Namespace.EqualsIgnoreCase( targetNamespace )
            );
        }

        public void Unregister( Type commandType )
        {
            UnregisterWhere( command => command.Command.GetType() == commandType );
        }

        public bool HasWithName( string commandName )
        {
            Preconditions.NotNull( commandName, "commandName cannot be null" );
            
            return HasWith( command =>
            {
                var rocketCommand = command as IRocketCommand;

                /* Search in Rocket commands */
                if ( rocketCommand != null )
                {
                    return rocketCommand.Name.EqualsIgnoreCase( commandName );
                }

                var unturnedCommand = command as SDG.Unturned.Command;

                /* If not found, then, search in Unturned & Essentials commands */
                if ( unturnedCommand != null )
                {
                    return unturnedCommand.command.EqualsIgnoreCase( commandName );
                }

                return false;
            } );
        }

        public bool HasWithType<TCommandType>() where TCommandType : ICommand
        {
            return HasWith( command =>
            {
                var commandBridge = command as CommandAdapter;

                return commandBridge?.Command is TCommandType;
            } );
        }

        public void UnregisterAll( Assembly assembly )
        {
            Preconditions.NotNull( assembly, "Assembly cannot be null" );

            UnregisterWhere( command => Equals( command.GetType().Assembly, assembly ) ); 
        }

        public void UnregisterAll( string targetNamespace )
        {
            UnregisterWhere( command => 
                command.GetType().Namespace.EqualsIgnoreCase( targetNamespace ) );
        }

        public void Unregister<TCommandType>() where TCommandType : ICommand
        {
            UnregisterWhere( command => command.Command is TCommandType );
        }

        public void Unregister( ICommand targetCommand )
        {
            UnregisterWhere( command => command.Command == targetCommand );
        }

        /* 
            Utility methods 
        */

        private static bool HasWith( Func<object, bool> predicate )
        {
            return 
                R.Commands.Commands.ToList().Any( command => predicate( command ) ) ||
                Commander.commands.Any( command => predicate( command ) );
        }

        private void UnregisterWhere( Func<CommandAdapter, bool> predicate )
        {
            _rocketCommands.RemoveAll( cmd => {
               if ( cmd is CommandAdapter && predicate( (CommandAdapter) cmd ) )
               {
                    var command = ((CommandAdapter) cmd).Command;
                    if ( command  is EssCommand )
                    {
                        AccessorFactory.AccessMethod<EssCommand>( command, "OnUnregistered" ).Invoke();
                    }
                    return true;
                }
                return false; 
            });
        }

        private void RegisterAllWhere( Assembly asm, Predicate<Type> filter )
        {
            Predicate<Type> defaultPredicate = type => {
                return (typeof (ICommand).IsAssignableFrom( type ) && !type.IsAbstract && type != typeof (MethodCommand));
            };

            /*
                Register classes
            */

            ( 
                from type in asm.GetTypes()
                where defaultPredicate(type)
                where filter(type)
                select (ICommand) Activator.CreateInstance( type )
            ).ForEach( Register );

            /*
                Register methods
            */
            Func<Type, object> createInstance = type => {
                if ( !type.IsClass || type.IsAbstract ) 
                {
                    throw new Exception($"{type} isn't an class or is abstract.");
                }
                  
                return Activator.CreateInstance( type );
            };

            Func<Type, object, MethodInfo, Delegate> createDelegate = (type, obj, method) => {
                return obj == null
                        ? Delegate.CreateDelegate( type, method )
                        : Delegate.CreateDelegate( type, obj, method.Name );
            };

            foreach ( var type in asm.GetTypes().Where( filter.Invoke ) )
            {
                foreach ( var method in type.GetMethods( (BindingFlags) 0x3C ) )
                {
                    if ( ReflectionUtil.GetAttributeFrom<CommandInfo>( method ) == null )
                        continue;

                    var inst = method.IsStatic ? null : createInstance( type );
                    var paramz = method.GetParameters();

                    if ( method.ReturnType != typeof (CommandResult) )
                    {
                        EssProvider.Logger.LogError( $"Invalid method signature in '{method}'. " +
                                                      "Expected 'CommandResult methodName(ICommandSource, ICommandArgs)'");
                        continue;
                    }

                    if ( paramz.Length == 2 &&
                         paramz[0].ParameterType == typeof (ICommandSource) &&
                         paramz[1].ParameterType == typeof (ICommandArgs) )
                    {
                        Register( (Func<ICommandSource, ICommandArgs, CommandResult>) createDelegate(
                            typeof (Func<ICommandSource, ICommandArgs, CommandResult>), inst, method ) );
                    }
                    else if ( paramz.Length == 3 &&
                              paramz[0].ParameterType == typeof (ICommandSource) &&
                              paramz[1].ParameterType == typeof (ICommandArgs) &&
                              paramz[2].ParameterType == typeof (ICommand) )
                    {
                        Register( (Func<ICommandSource, ICommandArgs, ICommand, CommandResult>) createDelegate(
                            typeof (Func<ICommandSource, ICommandArgs, ICommand, CommandResult>), inst, method ) );
                    }
                    else
                    {
                        EssProvider.Logger.LogError( $"Invalid method signature in '{method}'. " +
                                                      "Expected 'CommandResult methodName(ICommandSource, ICommandArgs)'");
                    }
                }
            }
        }

        private ICommand GetWhere( Func<CommandAdapter, bool> predicate )
        {
            return (
                from command in _rocketCommands
                where command is CommandAdapter
                let cmdAdapter = command as CommandAdapter
                where predicate( cmdAdapter )
                select cmdAdapter.Command
            ).FirstOrDefault();
        }
    }
}