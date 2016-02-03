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

﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rocket.Unturned.Items;
using SDG.Unturned;

namespace Essentials.Kits
{
    /// <summary>
    /// Author: leonardosc
    /// </summary>
    public class KitItemWeapon : KitItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public EFiremode FireMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public byte Ammo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public Attachment Barrel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public Attachment Sight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public Attachment Grip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public Attachment Tatical { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public Attachment Magazine { get; set; }

        /// <summary>
        /// Instance of SDG.Unturned.Item of this item
        /// </summary>
        /// <returns> Instance of SDG.Unturned.Item of this item </returns>>
        [JsonIgnore]
        public override Item UnturnedItem => UnturnedItems.AssembleItem(
            Id,
            Ammo,
            Sight,
            Tatical,
            Grip,
            Barrel,
            Magazine,
            FireMode,
            Amount,
            Durability
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="durability"></param>
        /// <param name="amount"></param>
        /// <param name="ammo"></param>
        /// <param name="fireMode"></param>
        public KitItemWeapon( ushort id, byte durability,  byte amount, byte ammo, 
                              EFiremode fireMode = EFiremode.AUTO )  : base( id, durability, amount )
        {
            FireMode = fireMode;
            Ammo = ammo;
            Type = ItemType.WEAPON;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Barrel: {Barrel}, Sight: {Sight}, " +
                   $"Grip: {Grip}, Tatical: {Tatical}, Magazine: {Magazine}, " +
                   $"FireMode: {FireMode}, Ammo: {Ammo}";
        }
    }
}
