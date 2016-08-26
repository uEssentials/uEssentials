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
using System.Reflection;
using Essentials.Common;

namespace Essentials.Compatibility {

    public class HookManager {

        private readonly Dictionary<string, Hook> _hooks = new Dictionary<string, Hook>();

        public IEnumerable<Hook> Hooks => _hooks.Values;

        internal HookManager() {}

        public void LoadAll() {
            Hooks.ForEach(h => h.Load());
        }

        public void UnloadAll() {
            Hooks.ForEach(h => h.Unload());
        }

        public void RegisterAll() {
            RegisterAll(GetType().Assembly);
        }

        public void RegisterAll(Assembly asm) {
            (
                from type in asm.GetTypes()
                where typeof(Hook).IsAssignableFrom(type)
                where !type.IsAbstract
                select type
            ).ForEach(RegisterHook);
        }

        public void UnregisterAll() {
            _hooks.Clear();
        }

        public void RegisterHook(Type hookType) {
            Preconditions.IsTrue(hookType.IsAbstract, $"Cannot register {hookType} because it is abstract.");

            var hook = (Hook) Activator.CreateInstance(hookType);

            _hooks.Add(hook.Name.ToLowerInvariant(), hook);
        }

        public void RegisterHook<T>() where T : Hook {
            RegisterHook(typeof(T));
        }

        public Optional<Hook> GetActiveByName(string hookName) {
            var hook = GetByName(hookName);

            if (hook.IsPresent && hook.Value.IsLoaded) {
                return hook;
            }

            return Optional<Hook>.Empty();
        }

        public Optional<THookType> GetActiveByType<THookType>() where THookType : Hook {
            var hook = Optional<THookType>.OfNullable(
                (THookType) _hooks.FirstOrDefault(h => h.Value is THookType &&
                                                       h.Value.IsLoaded).Value
                );

            return hook;
        }

        public Optional<Hook> GetByName(string hookName) {
            hookName = hookName.ToLowerInvariant();

            if (_hooks.ContainsKey(hookName)) {
                return Optional<Hook>.Of(_hooks[hookName]);
            }

            return Optional<Hook>.Empty();
        }

        public Optional<THookType> GetByType<THookType>() where THookType : Hook {
            return Optional<THookType>.OfNullable((THookType) _hooks
                .FirstOrDefault(h => h.Value is THookType).Value);
        }

    }

}