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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Event;
using Essentials.Api.Module;
using Essentials.Api.Task;
using Essentials.Common;
using Essentials.Compatibility;
using Essentials.Configuration;
using Essentials.Core;
using Essentials.Economy;
using Essentials.Logging;

namespace Essentials.Api {

    public static class UEssentials {

        /// <summary>
        /// Version of uEssentials
        /// </summary>
        public static string PLUGIN_VERSION => EssCore.PLUGIN_VERSION;

        /// <summary>
        /// Recommended version of Unturned
        /// </summary>
        public static string UNTURNED_VERSION => EssCore.UNTURNED_VERSION;

        /// <summary>
        /// Recommended version of Rocket
        /// </summary>
        public static string ROCKET_VERSION => EssCore.ROCKET_VERSION;

        /// <summary>
        /// Instance of CommandManager
        /// </summary>
        public static IEventManager EventManager => Core.EventManager;

        /// <summary>
        /// Instance of CommandManager
        /// </summary>
        public static ICommandManager CommandManager => Core.CommandManager;

        /// <summary>
        /// Instance of ModuleManager
        /// </summary>
        public static ModuleManager ModuleManager => Core.ModuleManager;

        /// <summary>
        /// Instance of HookManager
        /// </summary>
        public static HookManager HookManager => Core.HookManager;

        /// <summary>
        /// Instance of plugin configuration
        /// </summary>
        public static EssConfig Config => Core.Config;

        internal static ConsoleLogger Logger => Core.Logger;

        internal static ITaskExecutor TaskExecutor => Core.TaskExecutor;

        /// <summary>
        /// Current economy provider.
        /// </summary>
        public static Optional<IEconomyProvider> EconomyProvider => Core.EconomyProvider;

        /// <summary>
        /// Singleton instance of ConsoleSource
        /// </summary>
        public static ICommandSource ConsoleSource => Essentials.Core.Command.ConsoleSource.Instance;

        /// <summary>
        /// Plugin folder path
        /// </summary>
        public static string Folder => Core.Folder;

        /// <summary>
        /// Data folder path
        /// </summary>
        public static string DataFolder => Core.DataFolder;

        /// <summary>
        /// Translation folder path
        /// </summary>
        public static string TranslationFolder => Core.TranslationFolder;

        /// <summary>
        /// Translation folder path
        /// </summary>
        public static string ModulesFolder => Core.ModulesFolder;

        /// <summary>
        /// Singleton instance of Plugin
        /// </summary>
        public static EssCore Core {
            get {
                Preconditions.CheckState(EssCore.Instance != null, "Essentials isn't initialized");

                return EssCore.Instance;
            }
        }

    }

}