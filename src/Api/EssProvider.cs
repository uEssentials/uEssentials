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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Event;
using Essentials.Api.Logging;
using Essentials.Api.Module;
using Essentials.Common;
using Essentials.Compatibility;
using Essentials.Configuration;
using Essentials.Core;

namespace Essentials.Api
{
    public static class EssProvider
    {
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
        /// <returns> Instance of CommandManager </returns>
        /// </summary>
        public static IEventManager EventManager => Core.EventManager;

        /// <summary>
        /// <returns> Instance of CommandManager </returns>
        /// </summary>
        public static ICommandManager CommandManager => Core.CommandManager;

        /// <summary>
        /// <returns> Instance of ModuleManager </returns>
        /// </summary>
        public static ModuleManager ModuleManager => Core.ModuleManager;

        /// <summary>
        /// <returns> Instance of HookManager </returns>
        /// </summary>
        public static HookManager HookManager => Core.HookManager;

        /// <summary>
        /// <returns> Instance of plugin configuration </returns>
        /// </summary>
        public static EssConfig Config => Core.Config;

        /// <summary>
        /// <returns> Instance of EssLogger </returns>
        /// </summary>
        public static EssLogger Logger => Core.Logger;

        /// <summary>
        /// Singleton instance of ConsoleSource
        /// </summary>
        public static ICommandSource ConsoleSource => Essentials.Core.Command.ConsoleSource.Instance;

        /// <summary>
        /// <returns> Plugin folder path </returns>
        /// </summary>
        public static string PluginFolder => Core.Folder;

        /// <summary>
        /// <returns> Data folder path </returns>
        /// </summary>
        public static string DataFolder => Core.DataFolder;

        /// <summary>
        /// <returns> Translation folder path </returns>
        /// </summary>
        public static string TranslationFolder => Core.TranslationFolder;
        
        /// <summary>
        /// <returns> Translation folder path </returns>
        /// </summary>
        public static string ModulesFolder => Core.ModulesFolder;

        /// <summary>
        /// <returns> Singleton instance of Plugin </returns>
        /// </summary>
        public static EssCore Core
        {
            get
            {
                Preconditions.CheckState( EssCore.Instance != null, "Essentials isn't initialized" );

                return EssCore.Instance;
            }
        }
    }
}
