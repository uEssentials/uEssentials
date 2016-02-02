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
using Essentials.Api.Logging;
using Essentials.Common;
using Essentials.Common.Util;

namespace Essentials.Api.Module
{
    /// <summary>
    /// Author: Leonardosc
    /// </summary>
    public abstract class EssModule
    {
        /// <summary>
        /// The assembly that is running this module
        /// </summary>
        public Assembly Assembly { get; internal set; }

        /// <summary>
        /// Module Info
        /// </summary>
        public ModuleInfo Info { get; }

        /// <summary>
        /// Module logger
        /// </summary>
        public EssLogger Logger { get; }

        /// <summary>
        /// Module folder
        /// </summary>
        public string Folder { get; }

        /// <summary>
        /// Constructor :D
        /// </summary>
        protected EssModule()
        {
            Info = ReflectionUtil.GetAttributeFrom<ModuleInfo>( this );
            Preconditions.NotNull( Info, "Module must have 'ModuleInfo' attribute" );
            Preconditions.NotNull( Info.Name, "Module name cannot be null" );

            Logger = new EssLogger( $"[{Info.Name}] " );
            Folder = EssProvider.ModulesFolder + Info.Name + "/";

            if ( !Directory.Exists( Folder ) )
                Directory.CreateDirectory( Folder );
        }

        /// <summary>
        /// Called when module load
        /// </summary>
        public abstract void OnLoad();

        /// <summary>
        /// Called when module unload
        /// </summary>
        public abstract void OnUnload();

        /// <summary>
        /// Load this module
        /// </summary>
        internal virtual void Load()
        {
            Preconditions.CheckState( Assembly != null, "module assembly null" );

            OnLoad();

            EssProvider.CommandManager.RegisterAll( Assembly );
            EssProvider.EventManager.RegisterAll( Assembly );
        }

        /// <summary>
        /// Unload this module
        /// </summary>
        internal virtual void Unload()
        {
            Preconditions.CheckState( Assembly != null, "module assembly null" );

            OnUnload();

            EssProvider.CommandManager.UnregisterAll( Assembly );
            EssProvider.EventManager.UnregisterAll( Assembly );
        }
    }

    public abstract class EssModule<TConfigType> : EssModule where TConfigType : IConfig, new()
    {
        public TConfigType Configuration { get; private set; }

        internal override void Load()
        {
            Configuration = new TConfigType();
            Configuration.Load( Folder + Configuration.FileName );

            base.Load();
        }

        internal override void Unload()
        {
            Configuration?.Save( Folder + Configuration.FileName  );
            base.Load();
        }
    }
}