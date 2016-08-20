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

using System.IO;
using System.Reflection;
using Essentials.Api.Configuration;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Logging;

namespace Essentials.Api.Module {

    /// <summary>
    /// Author: Leonardosc
    /// </summary>
    public abstract class EssModule {

        /// <summary>
        /// The assembly that is running this module
        /// </summary>
        public Assembly Assembly { get; internal set; }

        /// <summary>
        /// Module Info
        /// </summary>
        public ModuleInfo Info { get; internal set; }

        /// <summary>
        /// Module logger
        /// </summary>
        public ConsoleLogger Logger { get; internal set; }

        /// <summary>
        /// Module folder
        /// </summary>
        public string Folder { get; internal set; }

        /// <summary>
        /// Constructor :D
        /// </summary>
        protected EssModule() {
            Info = ReflectionUtil.GetAttributeFrom<ModuleInfo>(this);
            Preconditions.NotNull(Info, "Module must have 'ModuleInfo' attribute");
            Preconditions.NotNull(Info.Name, "Module name cannot be null");

            Logger = new ConsoleLogger($"[{Info.Name}] ");
            Folder = UEssentials.ModulesFolder + Info.Name + "/";

            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }

        /// <summary>
        /// Called when module loads
        /// </summary>
        public virtual void OnLoad() {}

        /// <summary>
        /// Called when module Unloads
        /// </summary>
        public virtual void OnUnload() {}

        /// <summary>
        /// Load this module
        /// </summary>
        internal virtual void Load() {
            Preconditions.CheckState(Assembly != null, "module assembly null");

            OnLoad();

            if ((Info.Flags & LoadFlags.AUTO_REGISTER_COMMANDS) != 0) {
                UEssentials.CommandManager.RegisterAll(Assembly);
            }

            if ((Info.Flags & LoadFlags.AUTO_REGISTER_EVENTS) != 0) {
                UEssentials.EventManager.RegisterAll(Assembly);
            }
        }

        /// <summary>
        /// Unload this module
        /// </summary>
        internal virtual void Unload() {
            Preconditions.CheckState(Assembly != null, "module assembly null");

            OnUnload();

            UEssentials.CommandManager.UnregisterAll(Assembly);
            UEssentials.EventManager.UnregisterAll(Assembly);
        }

    }

    public abstract class EssModule<TConfigType> : EssModule where TConfigType : IConfig, new() {

        public TConfigType Configuration { get; private set; }

        internal override void Load() {
            Configuration = new TConfigType();
            Configuration.Load(Folder + Configuration.FileName);

            base.Load();
        }

        internal override void Unload() {
            Configuration?.Save(Folder + Configuration.FileName);
            base.Load();
        }

    }

}