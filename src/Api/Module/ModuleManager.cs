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
using Essentials.Common;
using Essentials.Core;

namespace Essentials.Api.Module {

    /// <summary>
    /// Author: leonardosc
    /// </summary>
    public sealed class ModuleManager {

        /// <summary>
        /// List of running modules
        /// </summary>
        public List<EssModule> RunningModules { get; set; }

        /// <summary>
        /// Internal constructor
        /// </summary>
        internal ModuleManager() {
            RunningModules = new List<EssModule>();
        }

        /// <summary>
        /// Load module from an assembly
        /// </summary>
        /// <param name="moduleAssembly">Assembly that contains module that you want to load</param>
        public EssModule LoadModule(Assembly moduleAssembly) {
            Preconditions.NotNull(moduleAssembly, "ModuleAssembly cannot be null");

            // Prevents to load uEssentials dll accidently
            if (moduleAssembly.Equals(typeof(EssCore).Assembly))
                return null;

            EssModule moduleInstance = null;

            foreach (var type in moduleAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(EssModule)))) {
                moduleInstance = (EssModule) Activator.CreateInstance(type);
                moduleInstance.Assembly = moduleAssembly;

                if (moduleInstance.Info.Version.EqualsIgnoreCase("$asmVersion")) {
                    moduleInstance.Info.Version = moduleAssembly.GetCustomAttributes(false)
                        .Cast<AssemblyFileVersionAttribute>()
                        .Select(c => c.Version)
                        .FirstOrDefault();
                }
            }

            Preconditions.NotNull(moduleInstance, "Could load module from assembly " +
                                                  moduleAssembly.FullName +
                                                  $" because it does not contains any class that extends '{typeof(EssModule)}");

            LoadModule(moduleInstance);

            return moduleInstance;
        }

        /// <summary>
        /// Load an module
        /// </summary>
        /// <param name="module">Module that you want to load</param>
        public void LoadModule(EssModule module) {
            Preconditions.NotNull(module, "Module cannot be null");
            Preconditions.IsTrue(RunningModules.Contains(module), "This module already loaded");

            module.Load();
            RunningModules.Add(module);
        }

        /// <summary>
        /// Unload ab module
        /// </summary>
        /// <param name="module">Module that you want to unload</param>
        public void UnloadModule(EssModule module) {
            Preconditions.NotNull(module, "Module cannot be null");
            Preconditions.IsFalse(RunningModules.Contains(module), "This module isn't running");

            module.Unload();
            RunningModules.Remove(module);
        }

        ///<summary>
        /// Load all modules from an folder
        ///</summary>
        /// <param name="directory"> Directory that you want to load modules</param>>
        public void LoadAll(string directory) {
            if (!Directory.Exists(directory)) return;

            var moduleFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);

            if (moduleFiles.Length == 0) return;

            foreach (var file in moduleFiles) {
                Load(file);
            }
        }

        ///<summary>
        /// Load modules from given assemblyPath
        ///</summary>
        /// <param name="assemblyPath">Assembly file path</param>>
        public void Load(string assemblyPath) {
            if (!File.Exists(assemblyPath)) {
                throw new FileNotFoundException($"File not found '{assemblyPath}'");
            }

            var fileExtension = Path.GetExtension(assemblyPath);

            if ("dll".Equals(fileExtension)) {
                throw new ArgumentException($"Invalid file '{assemblyPath}', file must be '.dll'");
            }

            var rawAssembly = File.ReadAllBytes(assemblyPath);
            var moduleAssembly = Assembly.Load(rawAssembly);

            if (moduleAssembly == null) {
                throw new Exception($"Could not read data from assembly '{assemblyPath}'");
            }

            try {
                LoadModule(moduleAssembly);
            } catch (Exception ex) {
                var fileName = (assemblyPath.Substring(assemblyPath.LastIndexOf("/", StringComparison.Ordinal) + 1));
                UEssentials.Logger.LogError("An error occurred attempting to load " + fileName + ", ignoring...");
                UEssentials.Logger.LogError(ex.ToString());
            }
        }

        public Optional<T> GetModule<T>() where T : EssModule {
            return Optional<T>.OfNullable(RunningModules.FirstOrDefault(m => m.GetType() == typeof(T)) as T);
        }

        public Optional<EssModule> GetModule(Type type) {
            return Optional<EssModule>.OfNullable(RunningModules.FirstOrDefault(m => m.GetType() == type));
        }

        ///<summary>
        /// Unload all modules
        ///</summary>
        public void UnloadAll() {
            RunningModules.ForEach(module => module.Unload());
            RunningModules.Clear();
        }

    }

}