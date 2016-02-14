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
using System.Linq;
using System.Reflection;
using Essentials.Common;

namespace Essentials.Compatibility
{
    public class HookManager
    {
        private readonly Dictionary<string, Hook> _activeHooks = new Dictionary<string, Hook>();

        public IEnumerable<Hook> Hooks => _activeHooks.Values;

        internal HookManager() {}

        public void LoadAll()
        {
            ( from hook in Hooks where hook.CanBeLoaded() select hook ).ForEach( h => h.OnLoad() );
        }

        public void UnloadAll()
        {
            Hooks.ForEach( h => h.OnUnload() );
        }

        public void RegisterAll()
        {
            RegisterAll( GetType().Assembly );
        }

        public void RegisterAll( Assembly asm )
        {
            ( 
                from type in asm.GetTypes()
                where typeof(Hook).IsAssignableFrom( type )
                where !type.IsAbstract 
                select type
             ).ForEach( RegisterHook );
        }

        public void UnregisterAll()
        {
            _activeHooks.Clear();
        }

        public void RegisterHook( Type hookType )
        {
            Preconditions.IsTrue( hookType.IsAbstract, $"Cannot register {hookType} because it is abstract." );

            var hook = (Hook) Activator.CreateInstance( hookType );

            _activeHooks.Add( hook.Name.ToLowerInvariant(), hook );
        }

        public void RegisterHook<T>() where T : Hook
        {
            RegisterHook( typeof (T) );
        }

        public Hook GetByName( string hookName )
        {
            return _activeHooks[hookName.ToLowerInvariant()];
        }
    }
}