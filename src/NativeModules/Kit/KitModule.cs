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

using Essentials.Api.Module;
using static Essentials.Api.UEssentials;

namespace Essentials.NativeModules.Kit
{
    [ModuleInfo( Name = "Kits" )]
    public class KitModule : NativeModule
    {
        public KitManager       KitManager  { get; internal set; }
        public static KitModule Instance    { get; internal set; }

        public override void OnLoad()
        {
            Instance = this;

            KitManager = new KitManager();
            KitManager.Load();

            Logger.LogInfo( $"Loaded {KitManager.Count} kits" );

            CommandManager.RegisterAll( "Essentials.NativeModules.Kit.Commands" );
        }

        public override void OnUnload()
        {
            CommandManager.UnregisterAll( "Essentials.NativeModules.Kit.Commands" );
            KitManager.Save();
        }
    }
}
