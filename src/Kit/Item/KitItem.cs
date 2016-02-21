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

using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.I18n;
using Newtonsoft.Json;
using SDG.Unturned;

// ReSharper disable InconsistentNaming

namespace Essentials.Kit.Item
{
    /// <summary>
    /// Author: leonardosc
    /// </summary>
    [JsonObject( Id = "Item" )]
    public class KitItem : AbstractKitItem
    {
        /// <summary>
        /// Id of item
        /// </summary>
        [JsonProperty]
        public ushort Id { get; set; }

        /// <summary>
        /// Durability of item
        /// </summary>
        [JsonProperty]
        public byte Durability { get; set; }

        /// <summary>
        /// Amount of item
        /// </summary>
        [JsonProperty]
        public byte Amount { get; set; }

        /// <summary>
        /// Metadata of item
        /// </summary>
        [JsonIgnore] 
        public byte[] Metadata { get; set; }

        /// <summary>
        /// Instance of SDG.Unturned.Item of this item
        /// </summary>
        /// <returns> Instance of SDG.Unturned.Item of this item </returns>>
        [JsonIgnore]
        public virtual SDG.Unturned.Item UnturnedItem => new SDG.Unturned.Item( Id, Amount, Durability, Metadata );

        public KitItem( ushort id, byte durability, byte amount )
        {
            Preconditions.CheckState(
                Assets.find(EAssetType.ITEM, id) != null,
                EssLang.INVALID_ITEM_ID.GetMessage( id )
            );

            Id = id;
            Durability = durability;
            Amount = amount;

            ItemUtil.GetItem( id ).IfPresent( ass => Metadata = ass.getState() );
        }

        /// <summary>
        /// Give this item to an specified player
        /// </summary>
        /// <param name="player"> player that you should give this item </param>
        /// <param name="dropIfInventoryFull"> determine if this item should be dropped 
        /// on ground if inventory is full </param>
        /// <returns> False if could not be added(full inventory) otherwise true </returns>
        public override bool GiveTo( UPlayer player, bool dropIfInventoryFull = true )
        {
            return player.GiveItem( UnturnedItem, dropIfInventoryFull );
        }

        public override string ToString()
        {
            return $"Id: {Id}, Durability: {Durability}, Amount: {Amount}";
        }
    }
}
