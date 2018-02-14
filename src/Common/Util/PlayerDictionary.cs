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

using System;
using System.Collections.Generic;
using Essentials.Api.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace Essentials.Common.Util {

    // A wrapper for Dictionary that stores players (id) and can
    // automatically remove when the player disconnects/die (based on Options)
    public class PlayerDictionary<TValue> : Dictionary<ulong, TValue> {

        public PlayerDictionaryOptions Options { get; }
        private bool _registeredEventHandlers;
        private readonly Action<TValue> _removalCallback;

        public PlayerDictionary() : this(PlayerDictionaryOptions.REMOVE_ON_DISCONNECT, null) {}

        public PlayerDictionary(PlayerDictionaryOptions options, Action<TValue> removalCallback) {
            Options = options;
            _removalCallback = removalCallback;

            if ((Options & PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS) !=
                PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS)
                RegisterEventHandlers();
        }

        public TValue this[UnturnedPlayer player] {
            get { return this[player.CSteamID.m_SteamID]; }
            set { this[player.CSteamID.m_SteamID] = value; }
        }

        public TValue this[UPlayer player] {
            get { return this[player.CSteamId.m_SteamID]; }
            set { this[player.CSteamId.m_SteamID] = value; }
        }

        public new void Add(ulong key, TValue value) {
            if ((Options & PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS) != 0 && !_registeredEventHandlers)
                RegisterEventHandlers();
            base.Add(key, value);
        }

        public new bool Remove(ulong key) {
            if (_removalCallback != null && TryGetValue(key, out var val))
                _removalCallback(val);
            return base.Remove(key);
        }

        private void RegisterEventHandlers() {
            _registeredEventHandlers = true;

            if ((Options & PlayerDictionaryOptions.REMOVE_ON_DISCONNECT) != 0)
                Provider.onServerDisconnected += OnPlayerDisconnected;

            if ((Options & PlayerDictionaryOptions.REMOVE_ON_DEATH) != 0)
                UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
        }

        private void OnPlayerDisconnected(CSteamID steamId) {
            Remove(steamId.m_SteamID);
        }


        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            Remove(player.CSteamID.m_SteamID);
        }

    }

    [Flags]
    public enum PlayerDictionaryOptions {

        /// Remove when player dies.
        REMOVE_ON_DEATH = 1,

        /// Remove when player disconnects
        REMOVE_ON_DISCONNECT = 1 << 1,

        /// Event handlers will only be registered when something is added to the dictionary.
        LAZY_REGISTER_HANDLERS = 1 << 2

    }

}