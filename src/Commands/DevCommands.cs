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

#if DEV
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Essentials.Api.Task;

namespace Essentials.Commands {

   public class DevCommands {
        
        [CommandInfo(
           Name = "stoptasks"
        )]
        private CommandResult StopTasksCommand(ICommandSource src, ICommandArgs args) {
            UEssentials.TaskExecutor.DequeueAll();
            return CommandResult.Success();
        }

        [CommandInfo(
            Name = "cls"
        )]
        private CommandResult ClsCommand(ICommandSource src, ICommandArgs args) {
            Console.Clear();
            return CommandResult.Success();
        }

        private Assembly _lastEvalFileAssembly;
        private DateTime _evalFileLastChanged;
        private Task _evalFileWatchTask;

        [CommandInfo(
            Name = "evalfile",
            Aliases = new [] { "evalf" },
            Usage = "<watch|unwatch>"
        )]
        private CommandResult EvalFileCommand(ICommandSource src, ICommandArgs args) {
            var evalFile = Path.Combine(UEssentials.Folder, "EvalFile.cs");

            if (!File.Exists(evalFile)) {
                return CommandResult.Error("File \"{0}\" does not exist.", evalFile);
            }

            if (args.Length > 0) {
                var subCommand = args[0].ToLowerString;

                if (subCommand == "watch") {
                    UEssentials.Logger.LogInfo("Watching evalfile...");
                    _evalFileWatchTask = Task.Create()
                        .Async()
                        .Interval(100)
                        .Id("eval_file_watch")
                        .Action(() => {
                            var lastWrite = File.GetLastWriteTime(evalFile);

                            if (lastWrite > _evalFileLastChanged) {
                                UEssentials.Logger.LogInfo("EvalFile changed...");
                                _evalFileLastChanged = lastWrite;
                                EvalFile(evalFile);
                            }
                        })
                        .Submit();
                    return CommandResult.Success();
                }

                if (subCommand == "unwatch") {
                    if (_evalFileWatchTask == null) {
                        return CommandResult.Generic("not watching");
                    } else {
                        _evalFileWatchTask.Cancel();
                        return CommandResult.Success();
                    }
                }

                return CommandResult.ShowUsage();
            }

            return EvalFile(evalFile);
        }
        
        private CommandResult EvalFile(string file) {
            if (_lastEvalFileAssembly != null) {
                var unloadMethod = _lastEvalFileAssembly.GetTypes().FirstOrDefault().GetMethod("Unload");

                if (unloadMethod != null) {
                    unloadMethod.Invoke(null, null);
                }
            }

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            CompilerParameters compilerParams = new CompilerParameters();

            // Add all currently loaded assemblies as reference
            foreach (var asmRef in AppDomain.CurrentDomain.GetAssemblies()) {
                var asmPath = asmRef.EscapedCodeBase.Substring(8);
                if (!File.Exists(asmPath)) continue;
                compilerParams.ReferencedAssemblies.Add(asmPath);
            }

            compilerParams.GenerateInMemory = true;
            compilerParams.CompilerOptions = "/t:library";

            UEssentials.Logger.LogInfo("Compiling EvalFile.cs...");

            CompilerResults results = codeProvider.CompileAssemblyFromFile(compilerParams, file);

            if (results.Errors.Count > 0) {
                foreach (var error in results.Errors) {
                    var compilerError = error as CompilerError;

                    if (compilerError == null || !compilerError.IsWarning) {
                        UEssentials.Logger.LogError(error.ToString());
                    } else {
                        UEssentials.Logger.LogWarning(error.ToString());
                    }
                }
            }

            if (results.CompiledAssembly == null) {
                return CommandResult.Error("Failed to compile, see console.");
            }

            var asm = _lastEvalFileAssembly = results.CompiledAssembly;
            var firstType = asm.GetTypes().FirstOrDefault();
            var loadMethod = firstType.GetMethod("Load");

            if (loadMethod == null) {
                return CommandResult.Error("Failed to eval, method 'Load' not found!");
            }

            UEssentials.Logger.LogInfo("Calling 'Load' method");
            try {
                loadMethod.Invoke(null, null);
            } catch(Exception ex) {
                UEssentials.Logger.LogError(ex.ToString());
            }
            UEssentials.Logger.LogInfo("'Load' method called");

            return CommandResult.Success();
        }
    }
}
#endif
