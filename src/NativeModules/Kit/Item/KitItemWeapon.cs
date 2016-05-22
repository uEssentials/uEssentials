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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rocket.Unturned.Items;
using SDG.Unturned;

namespace Essentials.NativeModules.Kit.Item
{
    public class KitItemWeapon : KitItem
    {
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public EFiremode? FireMode { get; set; }

        [JsonProperty]
        public byte? Ammo { get; set; }

        [JsonProperty]
        public Attachment Barrel { get; set; }

        [JsonProperty]
        public Attachment Sight { get; set; }

        [JsonProperty]
        public Attachment Grip { get; set; }

        [JsonProperty]
        public Attachment Tactical { get; set; }

        [JsonProperty]
        public Attachment Magazine { get; set; }

        /// <summary>
        /// Instance of SDG.Unturned.Item of this item
        /// </summary>
        /// <returns> Instance of SDG.Unturned.Item of this item </returns>>
        [JsonIgnore]
        public override SDG.Unturned.Item UnturnedItem
        {
            get
            {
                var item = base.UnturnedItem;

                if ( item.Metadata.Length != 18 )
                    return item;

                var metadata = item.Metadata;

                Action<int[], Attachment> assembleAttach = ( indexes, attach ) => {
                    if ( attach == null || attach.AttachmentId == 0 ) return;

                    var attachIdBytes = BitConverter.GetBytes( attach.AttachmentId );

                    metadata[indexes[0]] = attachIdBytes[0];
                    metadata[indexes[1]] = attachIdBytes[1];
                    metadata[indexes[2]] = attach.Durability;
                };

                assembleAttach( new[] { 0x0, 0x1, 0xD }, Sight );
                assembleAttach( new[] { 0x2, 0x3, 0xE }, Tactical );
                assembleAttach( new[] { 0x4, 0x5, 0xF }, Grip );
                assembleAttach( new[] { 0x6, 0x7, 0x10 }, Barrel );
                assembleAttach( new[] { 0x8, 0x9, 0x11 }, Magazine );

                if ( Ammo.HasValue )
                {
                    metadata[0xA] = Ammo.Value;
                }

                if ( FireMode.HasValue )
                {
                    metadata[0xB] = (byte) FireMode;
                }

                metadata[0xC] = 1;

                return item;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="durability"></param>
        /// <param name="amount"></param>
        /// <param name="ammo"></param>
        /// <param name="fireMode"></param>
        public KitItemWeapon( ushort id, byte durability,  byte amount, byte? ammo, 
                              EFiremode? fireMode = EFiremode.SAFETY )  : base( id, durability, amount )
        {
            FireMode = fireMode;
            Ammo = ammo;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Barrel: {Barrel}, Sight: {Sight}, " +
                   $"Grip: {Grip}, Tactical: {Tactical}, Magazine: {Magazine}, " +
                   $"FireMode: {FireMode}, Ammo: {Ammo}";
        }
    }
}
