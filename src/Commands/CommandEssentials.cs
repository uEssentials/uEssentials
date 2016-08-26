#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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
using System.Linq;
using System.Text;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core;
using Essentials.Core.Command;
using Essentials.I18n;
using Essentials.NativeModules.Kit;
using Essentials.NativeModules.Warp;
using Rocket.Core;
using UnityEngine;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "essentials",
        Description = "Plugin commands",
        Aliases = new[] { "ess", "?", "uessentials" },
        Usage = "<commands/help/info/reload/savedata>"
    )]
    public class CommandEssentials : EssCommand {

        private readonly LazyInitVar<string> _cachedCommands = LazyInitVar<string>.Of(() => {
            var builder = new StringBuilder("Commands: \n");

            UEssentials.CommandManager.Commands
                .Where(IsEssentialsCommand)
                .ForEach(command => {
                    builder.Append("  /")
                        .Append(command.Name.ToLower())
                        .Append(command.Usage == "" ? "" : " " + command.Usage)
                        .Append(" - ").Append(command.Description)
                        .AppendLine();
                });

            return builder.ToString();
        });

        private readonly LazyInitVar<List<List<string>>> _ingameCommandPages = LazyInitVar<List<List<string>>>.Of(() => {
            const int PAGE_SIZE = 16;

            var ret = new List<List<string>>(50) {
                new List<string>(PAGE_SIZE)
            };

            var builder = new StringBuilder("Commands: \n");
            var count = 0;
            var page = 0;

            UEssentials.CommandManager.Commands
                .Where(IsEssentialsCommand)
                .ForEach(command => {
                    if (count >= (PAGE_SIZE - 1)) {
                        ret[page++].Add("Use /ess help <command> to view help page.");
                        ret.Add(new List<string>(PAGE_SIZE));
                        count = 0;
                    }

                    builder.Append("  /")
                        .Append(command.Name.ToLower())
                        .Append(command.Usage == "" ? "" : " " + command.Usage)
                        .Append(" - ").Append(command.Description)
                        .AppendLine();

                    ret[page].Add(builder.ToString());
                    builder.Length = 0;
                    count++;
                });

            return ret;
        });

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty) {
                return CommandResult.ShowUsage();
            }

            switch (args[0].ToLowerString) {
                case "savedata":
                     if (!src.HasPermission($"{Permission}.savedata")) {
                        return CommandResult.NoPermission($"{Permission}.savedata");
                    }
                    UEssentials.ModuleManager.GetModule<KitModule>().IfPresent(m => {
                        m.KitManager.CooldownData.Save();
                    });
                    UEssentials.ModuleManager.GetModule<WarpModule>().IfPresent(m => {
                        m.WarpManager.Save();
                    });
                    break;

                case "reload":
                    if (!src.HasPermission($"{Permission}.reload")) {
                        return CommandResult.NoPermission($"{Permission}.reload");
                    }
                    if (args.Length == 1) {
                        src.SendMessage("Reloading all...");
                        ReloadConfig();
                        ReloadKits();
                        ReloadLang();
                        R.Permissions.Reload();
                        src.SendMessage("Reload finished...");
                    } else {
                        switch (args[1].ToLowerString) {
                            case "kits":
                            case "kit":
                                src.SendMessage("Reloading kits...");
                                ReloadKits();
                                src.SendMessage("Reload finished...");
                                break;

                            case "config":
                                src.SendMessage("Reloading config...");
                                ReloadConfig();
                                src.SendMessage("Reload finished...");
                                break;

                            case "lang":
                                src.SendMessage("Reloading translations...");
                                ReloadLang();
                                src.SendMessage("Reload finished...");
                                break;

                            default:
                                return CommandResult.InvalidArgs("Use /ess reload <kits/config/lang>");
                        }
                    }
                    break;

                case "commands":
                    if (src.IsConsole) {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(_cachedCommands.Value);
                        Console.WriteLine("Use /ess help <command> to view help page.");
                    } else if (args.Length != 2 || !args[1].IsInt) {
                        src.SendMessage("Use /ess commands [page]");
                    } else {
                        var pages = _ingameCommandPages.Value;
                        var pageArg = args[1].ToInt;

                        if (pageArg < 1 || pageArg > pages.Count - 1) {
                            src.SendMessage($"Page must be between 1 and {pages.Count - 1}", Color.red);
                        } else {
                            pages[pageArg - 1].ForEach(s => {
                                src.SendMessage(s, Color.cyan);
                            });
                        }
                    }
                    break;

                case "info":
                    if (src.IsConsole) {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    src.SendMessage("Author:  leonardosnt", Color.yellow);
                    src.SendMessage("Github:  github.com/leonardosnt", Color.yellow);
                    src.SendMessage("uEssentials Github:  github.com/uEssentials", Color.yellow);
                    src.SendMessage("Version: " + EssCore.PLUGIN_VERSION, Color.yellow);
                    break;

                case "help":
                    if (src.IsConsole)
                        Console.ForegroundColor = ConsoleColor.Green;

                    if (args.Length == 1) {
                        src.SendMessage("Use /ess help <command>");
                    } else {
                        var command = UEssentials.CommandManager.GetByName(args[1].ToString());

                        if (command == null) {
                            src.SendMessage($"Command {args[1]} does not exists", Color.red);
                        } else {
                            src.SendMessage("Command: " + command.Name, Color.cyan);
                            src.SendMessage("  Usage Syntax: ", Color.cyan);
                            src.SendMessage("    - [arg] = required argument.", Color.cyan);
                            src.SendMessage("    - <arg> = optional argument.", Color.cyan);
                            src.SendMessage("    - | or / = means 'Or'.", Color.cyan);
                            src.SendMessage("  Description: " + command.Description, Color.cyan);
                            src.SendMessage("  Usage: /" + command.Name + " " + command.Usage, Color.cyan);
                            if (command.Aliases.Any())
                                src.SendMessage("  Aliases: " + string.Join(", ", command.Aliases), Color.cyan);
                        }
                    }
                    break;

                case "debug":
                case "dbg":
                    if (!src.HasPermission($"{Permission}.debug")) {
                        return CommandResult.NoPermission($"{Permission}.debug");
                    }

                    if (args.Length < 3) {
                        return CommandResult.InvalidArgs("Use /essentials debug [commands/tasks] [true/false]");
                    }

                    if (!args[2].IsBool) {
                        return CommandResult.Lang("INVALID_BOOLEAN", args[2]);
                    }

                    var flag = args[2].ToBool;
                    byte mask;
                    switch (args[1].RawValue.ToLowerInvariant()) {
                        case "commands":
                            mask = EssCore.kDebugCommands;
                            src.SendMessage($"DebugCommands set to {flag}");
                            break;

                        case "tasks":
                            mask = EssCore.kDebugTasks;
                            src.SendMessage($"DebugTasks set to {flag}");
                            break;

                        default:
                            return CommandResult.InvalidArgs($"Invalid option '{args[1]}'");
                    }
                    if (flag) {
                        EssCore.DebugFlags |= mask;
                    } else {
                        EssCore.DebugFlags &= (byte) ~mask; //O'Rly c#?
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

        private static bool IsEssentialsCommand(ICommand cmd) {
            var asm = cmd.GetType().Assembly;

            if (cmd is CommandTest) {
                return false;
            }

            if (cmd.GetType() == typeof(MethodCommand)) {
                asm = ((MethodCommand) cmd).Owner.Assembly;
            }

            return asm.Equals(typeof(EssCore).Assembly);
        }

        private static void ReloadKits() {
            UEssentials.ModuleManager.GetModule<KitModule>().IfPresent(m => {
                m.KitManager = new KitManager();
                m.KitManager.LoadKits();
            });
        }

        private static void ReloadLang() {
            EssLang.Load();
        }

        private static void ReloadConfig() {
            EssCore.Instance.LoadConfig();
        }
    }
}