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
using Steamworks;

namespace Essentials.Misc
{
    public class SimpleCooldown
    {
        private readonly Dictionary<ulong, DateTime> Cooldowns = new Dictionary<ulong, DateTime>();

        public bool HasEntry( CSteamID playerId )
        {
            if ( GetRemainingTime( playerId) < 0 )
            {
                RemoveEntry( playerId );
                return false;
            }
            return Cooldowns.ContainsKey( playerId.m_SteamID );
        }

        public void AddEntry( CSteamID playerId, int seconds )
        {
            AddEntry( playerId, TimeSpan.FromSeconds( seconds ) );
        }

        public void AddEntry( CSteamID playerId, TimeSpan cooldown )
        {
            Cooldowns.Add( playerId.m_SteamID, DateTime.Now.Add( cooldown ) );
        }

        public bool RemoveEntry( CSteamID playerId )
        {
            return Cooldowns.Remove( playerId.m_SteamID );
        }

        public double GetRemainingTime( CSteamID playerId )
        {
            DateTime val;
            if ( Cooldowns.TryGetValue( playerId.m_SteamID, out val ) )
            {
                return (val - DateTime.Now).TotalSeconds;
            }
            return 0;
        }

        public void RemovedIfExpired( CSteamID playerId )
        {
            if ( GetRemainingTime( playerId) < 0 )
                RemoveEntry( playerId );
        }
    }
}