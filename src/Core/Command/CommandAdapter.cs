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

using System;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common.Util;
using Essentials.Event;
using Essentials.I18n;
using Rocket.API;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rocket.Unturned.Player;

namespace Essentials.Core.Command {

    internal class CommandAdapter : IRocketCommand {

        public List<string> Aliases { get; }
        public AllowedCaller AllowedCaller { get; }
        public string Help { get; }
        public string Name { get; private set; }
        public List<string> Permissions { get; }
        public string Syntax { get; }

        internal readonly ICommand Command;

        private readonly CommandInfo _info;

        internal CommandAdapter(ICommand command) {
            Command = command;
            Name = command.Name;
            Aliases = command.Aliases.ToList();
            Help = command.Description;
            Syntax = command.Usage;
            Permissions = new List<string>(1) { command.Permission };
            AllowedCaller = AllowedCaller.Both;

            if (command is EssCommand) {
                _info = ((EssCommand) command).Info;
            }
        }

        public void Execute(IRocketPlayer caller, string[] args) {
            var sw = (EssCore.DebugFlags & EssCore.kDebugCommands) != 0 ? Stopwatch.StartNew() : null;

            CommandResult result = null;
            var commandSource = caller is UnturnedPlayer
                    ? UPlayer.From((UnturnedPlayer) caller)
                    : UEssentials.ConsoleSource;

            try {
                if (commandSource.IsConsole && Command.AllowedSource == AllowedSource.PLAYER) {
                    EssLang.Send(commandSource, "CONSOLE_CANNOT_EXECUTE");
                } else if (!commandSource.IsConsole && Command.AllowedSource == AllowedSource.CONSOLE) {
                    EssLang.Send(commandSource, "PLAYER_CANNOT_EXECUTE");
                } else {
                    var cmdArgs = (ICommandArgs) new CommandArgs(args);
                    var preExec = EssentialsEvents.CallCommandPreExecute(Command, ref cmdArgs, ref commandSource);

                    if (preExec.Cancelled) {
                        return;
                    }

                    if (_info != null && (_info.MinArgs > cmdArgs.Length || cmdArgs.Length > _info.MaxArgs)) {
                        result = CommandResult.ShowUsage();
                    } else {
                        result = Command.OnExecute(commandSource, cmdArgs);
                    }

                    EssentialsEvents.CallCommandPosExecute(Command, ref cmdArgs, ref commandSource, ref result);

                    if (result != null) {
                        if (result.Type == CommandResult.ResultType.SHOW_USAGE) {
                            EssLang.Send(commandSource, "COMMAND_USAGE_TEMPLATE", Command.Name, Command.Usage);
                        } else if (result.Message != null) {
                            var message = result.Message;
                            var color = ColorUtil.GetColorFromString(ref message);
                            commandSource.SendMessage(message, color);
                        }
                    }
                }
            } catch (Exception e) {
                if (caller is UnturnedPlayer) {
                    UPlayer.TryGet((UnturnedPlayer) caller, p => {
                        EssLang.Send(p, p.IsAdmin ? "COMMAND_ERROR_OCURRED_ADMIN" : "COMMAND_ERROR_OCURRED");
                    });
                }
                UEssentials.Logger.LogError($"An error ocurred while executing command: '{Name} " +
                                             $"{string.Join(" ", args)}'");
                UEssentials.Logger.LogException(e);
            }

            if ((EssCore.DebugFlags & EssCore.kDebugCommands) != 0 && sw != null) {
                sw.Stop();
                UEssentials.Logger.LogDebug("Executed command {");
                UEssentials.Logger.LogDebug($"  Source: '{commandSource.GetType()}:{commandSource}'");
                UEssentials.Logger.LogDebug($"  Name: '{Command.Name}'");
                UEssentials.Logger.LogDebug($"  Args: [{string.Join(", ", args)}]");
                UEssentials.Logger.LogDebug($"  Type: '{Command.GetType()}'");
                UEssentials.Logger.LogDebug($"  Result: '{result?.ToString() ?? "null"}'");
                UEssentials.Logger.LogDebug($"  Took: '{sw.ElapsedTicks} ticks | {sw.ElapsedMilliseconds} ms'");
                UEssentials.Logger.LogDebug("}");
            }
        }

        internal class CommandAliasAdapter : CommandAdapter {
            internal CommandAliasAdapter(ICommand command, string alias) : base(command) {
                Name = alias;
            }
        }

    }

}