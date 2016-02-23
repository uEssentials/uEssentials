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

using System.Collections.Generic;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.I18n;
using Essentials.InternalModules.Kit.Item;
using Newtonsoft.Json;

namespace Essentials.InternalModules.Kit
{
    /// <summary>
    /// Author: leonardosc
    /// </summary>
    [JsonObject]
    public class Kit
    {

        /// <summary>
        /// Name of kit (obviously)
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// List of items
        /// </summary>
        [JsonProperty]
        public List<AbstractKitItem> Items { get; set; }

        /// <summary>
        /// Cooldown in seconds
        /// </summary>
        [JsonProperty]
        public uint Cooldown { get; set; }

        /// <summary>
        /// Flag that determines if cooldown will be reseted when player die
        /// </summary>
        [JsonProperty]
        public bool ResetCooldownWhenDie { get; set; }

        /// <summary>
        /// <param name="name"> name of kit </param>
        /// <param name="cooldown"> Cooldown in seconds of kit </param>
        /// <param name="resetCooldownWhenDie"> 
        ///     Flag that determines if cooldown will be reseted when player die
        /// </param>
        /// </summary>
        public Kit(string name, uint cooldown, bool resetCooldownWhenDie)
        {
            Preconditions.NotNull( name, "Kit Name cannot be null" );

            Name = name;
            Cooldown = cooldown;
            ResetCooldownWhenDie = resetCooldownWhenDie;
            Items = new List<AbstractKitItem>();
        }

        /// <summary>
        /// <returns> If determinated player has permission for this kit </returns>
        /// </summary>
        public bool CanUse( ICommandSource player )
        {
            return player.HasPermission( $"essentials.kit.{Name}" );
        }

        /// <summary>
        /// Give this kit to player
        /// </summary>
        public void GiveTo( UPlayer player )
        {
            var onetime = false;

            foreach ( var kitItem in Items )
            {
                var added = kitItem.GiveTo( player );

                if ( added || onetime ) continue;

                EssLang.INVENTORY_FULL.SendTo( player );
                onetime = true;
            }

            EssLang.KIT_GIVEN_RECEIVER.SendTo( player, Name );
        }
    }
}
