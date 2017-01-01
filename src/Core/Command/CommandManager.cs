#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.CodeAnalysis;
using Essentials.Common;
using Essentials.Common.Util;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Commands;
using SDG.Unturned;

namespace Essentials.Core.Command {

    internal class CommandManager : ICommandManager {

        private Dictionary<string, ICommand> CommandMap { get; }

        private readonly List<RocketCommandManager.RegisteredRocketCommand> _rocketCommands;
        private readonly MethodInfo _onRegisteredMethod = ReflectUtil.GetMethod<EssCommand>("OnRegistered");
        private readonly MethodInfo _onUnregisteredMethod = ReflectUtil.GetMethod<EssCommand>("OnUnregistered");

        public IEnumerable<ICommand> Commands => CommandMap.Values;

        internal CommandManager() {
            CommandMap = new Dictionary<string, ICommand>();
            var commandsField = ReflectUtil.GetField(R.Commands.GetType(), "commands");
            _rocketCommands = (List<RocketCommandManager.RegisteredRocketCommand>) commandsField.GetValue(R.Commands);
        }

        public ICommand GetByName([NotNull] string name, bool includeAliases = true) {
            ICommand command;
            if (CommandMap.TryGetValue(name.ToLowerInvariant(), out command)) {
                return command;
            }
            return GetWhere(cmd => cmd.Name.EqualsIgnoreCase(name));
        }

        public ICommand GetByType(Type commandType) {
            return GetWhere(command => command.Command.GetType() == commandType);
        }

        public TCommandType GetByType<TCommandType>() where TCommandType : ICommand {
            return (TCommandType) GetByType(typeof(TCommandType));
        }

        public void Register([NotNull] ICommand command) {
            Preconditions.NotNull(command.Name, "Command 'Name' cannot be null");

            var name = command.Name.ToLowerInvariant();

            if (CommandMap.ContainsKey(name)) {
                UEssentials.Logger.LogError(
                    $"Could not register '{command.GetType().Name}' because there is already a command called '{name}'");
                return;
            }

            var commandOptions = EssCore.Instance.CommandOptions.Commands;

            if (commandOptions.ContainsKey(command.Name)) {
                var cmdEntry = commandOptions[command.Name];

                if (cmdEntry.OverridedAliases != null) {
                    command.Aliases = cmdEntry.OverridedAliases;
                } else if (cmdEntry.CustomAliases != null) {
                    command.Aliases = command.Aliases.Concat(cmdEntry.CustomAliases).ToArray();
                }

                if (cmdEntry.Description != null) {
                    command.Description = cmdEntry.Description;
                }

                if (cmdEntry.Usage != null) {
                    command.Usage = cmdEntry.Usage;
                }
            }

            var adapter = new CommandAdapter(command);

            _rocketCommands.Add(new RocketCommandManager.RegisteredRocketCommand(name, adapter));
            CommandMap.Add(name.ToLowerInvariant(), command);

            Debug.WriteLine($"Registered '{command}'", "CommandManager");

            if (command is EssCommand) {
                _onRegisteredMethod?.Invoke(command, ReflectUtil.EMPTY_ARGS);
            }

            var aliases = command.Aliases;

            if (aliases == null || aliases.Length == 0) return;

            foreach (var alias in aliases) {
                _rocketCommands.Add(new RocketCommandManager.RegisteredRocketCommand(
                    alias.ToLowerInvariant(), new CommandAdapter.CommandAliasAdapter(command, alias)));
            }
        }

        public void Register<TCommandType>() where TCommandType : ICommand {
            Register((ICommand) EssCore.Instance.CommonInstancePool.GetOrCreate(typeof(TCommandType)));
        }

        public void Register([NotNull] Func<ICommandSource, ICommandArgs, CommandResult> method) {
            Register(new MethodCommand(method));
        }

        public void Register([NotNull] Func<ICommandSource, ICommandArgs, ICommand, CommandResult> method) {
            Register(new MethodCommand(method));
        }

        public void RegisterAll([NotNull] Assembly assembly) {
            RegisterAllWhere(assembly, type => true);
        }

        public void RegisterAll(string targetNamespace) {
            RegisterAllWhere(
                GetType().Assembly,
                type => type.Namespace.EqualsIgnoreCase(targetNamespace)
            );
        }

        public void Unregister(Type commandType) {
            UnregisterWhere(command => command.Command.GetType() == commandType);
        }

        public void UnregisterAll([NotNull] Assembly assembly) {
            UnregisterWhere(command => Equals(command.GetType().Assembly, assembly));
        }

        public void UnregisterAll(string targetNamespace) {
            UnregisterWhere(command =>
                command.GetType().Namespace.EqualsIgnoreCase(targetNamespace));
        }

        public void Unregister<TCommandType>() where TCommandType : ICommand {
            UnregisterWhere(command => command.Command is TCommandType);
        }

        public void Unregister(ICommand targetCommand) {
            UnregisterWhere(command => command.Command == targetCommand);
        }

        public bool HasWithName([NotNull] string commandName) {
            return HasWith(command => {
                var rocketCommand = command as IRocketCommand;

                /* Search in Rocket commands */
                if (rocketCommand != null) {
                    return rocketCommand.Name.EqualsIgnoreCase(commandName);
                }

                var unturnedCommand = command as SDG.Unturned.Command;

                /* If not found, then, search in Unturned & Essentials commands */
                if (unturnedCommand != null) {
                    return unturnedCommand.command.EqualsIgnoreCase(commandName);
                }

                return false;
            });
        }

        public bool HasWithType<TCommandType>() where TCommandType : ICommand {
            return HasWith(command => {
                var commandBridge = command as CommandAdapter;

                return commandBridge?.Command is TCommandType;
            });
        }

        /*
            Utility methods
        */

        private static bool HasWith(Func<object, bool> predicate) {
            return
                R.Commands.Commands.ToList().Any(command => predicate(command)) ||
                Commander.commands.Any(command => predicate(command));
        }

        private void UnregisterWhere(Func<CommandAdapter, bool> predicate) {
            _rocketCommands.RemoveAll(cmd => {
                var cmdAdapter = cmd.Command as CommandAdapter;
                if (cmdAdapter != null && predicate(cmdAdapter)) {
                    var command = cmdAdapter.Command;
                    if (command is EssCommand) {
                        _onUnregisteredMethod?.Invoke(command, ReflectUtil.EMPTY_ARGS);
                    }
                    CommandMap.Remove(command.Name.ToLowerInvariant());
                    return true;
                }
                return false;
            });
        }

        private void RegisterAllWhere(Assembly asm, Predicate<Type> filter) {
            Predicate<Type> defaultPredicate = type => {
                return (typeof(ICommand).IsAssignableFrom(type) && !type.IsAbstract && type != typeof(MethodCommand));
            };

            var commonInstPoll = EssCore.Instance.CommonInstancePool;

            /*
                Register classes
            */

            (
                from type in asm.GetTypes()
                where defaultPredicate(type)
                where filter(type)
                select (ICommand) commonInstPoll.GetOrCreate(type)
            ).ForEach(Register);

            /*
                Register methods
            */
            Func<Type, object, MethodInfo, Delegate> createDelegate = (type, obj, method) => {
                return obj == null
                    ? Delegate.CreateDelegate(type, method)
                    : Delegate.CreateDelegate(type, obj, method.Name);
            };

            foreach (var type in asm.GetTypes().Where(filter.Invoke)) {
                foreach (var method in type.GetMethods((BindingFlags) 0x3C)) {
                    if (ReflectUtil.GetAttributeFrom<CommandInfo>(method) == null)
                        continue;

                    var inst = method.IsStatic ? null : EssCore.Instance.CommonInstancePool.GetOrCreate(type);
                    var paramz = method.GetParameters();

                    if (method.ReturnType != typeof(CommandResult)) {
                        UEssentials.Logger.LogError($"Invalid method signature in '{method}'. " +
                                                    "Expected 'CommandResult methodName(ICommandSource, ICommandArgs)'");
                        continue;
                    }

                    if (paramz.Length == 2 &&
                        paramz[0].ParameterType == typeof(ICommandSource) &&
                        paramz[1].ParameterType == typeof(ICommandArgs)) {
                        Register((Func<ICommandSource, ICommandArgs, CommandResult>) createDelegate(
                            typeof(Func<ICommandSource, ICommandArgs, CommandResult>), inst, method));
                    } else if (paramz.Length == 3 &&
                               paramz[0].ParameterType == typeof(ICommandSource) &&
                               paramz[1].ParameterType == typeof(ICommandArgs) &&
                               paramz[2].ParameterType == typeof(ICommand)) {
                        Register((Func<ICommandSource, ICommandArgs, ICommand, CommandResult>) createDelegate(
                            typeof(Func<ICommandSource, ICommandArgs, ICommand, CommandResult>), inst, method));
                    } else {
                        UEssentials.Logger.LogError($"Invalid method signature in '{method}'. " +
                                                    "Expected 'CommandResult methodName(ICommandSource, ICommandArgs)'");
                    }
                }
            }
        }

        private ICommand GetWhere(Func<CommandAdapter, bool> predicate) {
            return (
                from command in _rocketCommands
                where command.Command is CommandAdapter
                let cmdAdapter = command.Command as CommandAdapter
                where predicate(cmdAdapter)
                select cmdAdapter.Command
            ).FirstOrDefault();
        }

    }

}