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

using Essentials.Api.Unturned;
using Essentials.Common.Util;
using Newtonsoft.Json;
using SDG.Unturned;

namespace Essentials.NativeModules.Kit.Item
{
    [JsonObject(Id = "Item")]
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
        public virtual SDG.Unturned.Item UnturnedItem => new SDG.Unturned.Item(Id, Amount, Durability, Metadata);

        public KitItem(ushort id, byte durability, byte amount)
        {
            Id = id;
            Durability = durability;
            Amount = amount;

            ItemUtil.GetItem(id).IfPresent(ass => Metadata = ass.getState());
        }

        /// <summary>
        /// Give this item to a specific player
        /// </summary>
        /// <param name="player"> player that will receive this item </param>
        /// <param name="dropIfInventoryFull"> determines whether this item should be dropped
        /// on ground if the player's inventory is full </param>
        /// <returns> true if item was sucessfully added to the player's inventory, otherwise false </returns>
        public override bool GiveTo(UPlayer player, bool dropIfInventoryFull = true)
        {
            return player.GiveItem(UnturnedItem, dropIfInventoryFull);
        }

        public override string ToString()
        {
            var itemName = ((ItemAsset) Assets.find(EAssetType.ITEM, Id))?.itemName;
            return $"Id: {Id}{(itemName == null ? "" : $" ({itemName})")}, Durability: {Durability}, Amount: {Amount}";
        }
    }
}