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
using System.Globalization;
using System.Linq;
using Rocket.Unturned.Items;
using SDG.Unturned;

namespace Essentials.Common.Util {

    public static class ItemUtil {

        private static IOrderedEnumerable<ItemAsset> _cachedAssets;

        public static Optional<ItemAsset> GetItem(ushort id) {
            return Optional<ItemAsset>.OfNullable((ItemAsset) Assets.find(EAssetType.ITEM, id));
        }

        public static Optional<ItemAsset> GetItem(string name) {
            if (name == null) {
                return Optional<ItemAsset>.Empty();
            }

            ushort id;

            if (ushort.TryParse(name, out id)) {
                return GetItem(id);
            }

            if (_cachedAssets == null) {
                _cachedAssets = Assets.find(EAssetType.ITEM)
                    .Cast<ItemAsset>()
                    .Where(i => i.Name != null)
                    .OrderBy(i => i.Id);
            }

            var lastAsset = null as ItemAsset;
            var lastPriority = 0;

            foreach (var asset in _cachedAssets) {
                var itemPriority = 0;
                var itemName = asset.Name;

                if (itemName.EqualsIgnoreCase(name)) {
                    lastAsset = asset;
                    break;
                }

                if (itemName.StartsWith(name, true, CultureInfo.InvariantCulture)) {
                    itemPriority = 3;
                } else if (itemName.ContainsIgnoreCase(name)) {
                    itemPriority = 2;
                } else if (name.IndexOf(' ') > 0 && name.Split(' ').All(p => itemName.ContainsIgnoreCase(p))) {
                    itemPriority = 1;
                }

                if (itemPriority > lastPriority) {
                    lastAsset = asset;
                    lastPriority = itemPriority;
                }
            }

            return Optional<ItemAsset>.OfNullable(lastAsset);
        }

        public static Optional<T> GetItemAs<T>(string name) where T : ItemAsset {
            var optItem = GetItem(name);

            return optItem.IsPresent ? Optional<T>.Of(optItem.Value as T) : Optional<T>.Empty();
        }

        public static Optional<T> GetItemAs<T>(ushort id) where T : ItemAsset {
            var optItem = GetItem(id);

            return optItem.IsPresent ? Optional<T>.Of(optItem.Value as T) : Optional<T>.Empty();
        }

        public static Optional<Attachment> GetWeaponAttachment(byte[] metadata, AttachmentType type) {
            if (metadata.Length < 18) {
                return Optional<Attachment>.Empty();
            }

            int[] indexes;

            switch (type) {
                case AttachmentType.SIGHT:
                    indexes = new[] { 0x0, 0x1, 0xD };
                    break;

                case AttachmentType.TACTICAL:
                    indexes = new[] { 0x2, 0x3, 0xE };
                    break;

                case AttachmentType.GRIP:
                    indexes = new[] { 0x4, 0x5, 0xF };
                    break;

                case AttachmentType.BARREL:
                    indexes = new[] { 0x6, 0x7, 0x10 };
                    break;

                case AttachmentType.MAGAZINE:
                    indexes = new[] { 0x8, 0x9, 0x11 };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var attachDurability = metadata[indexes[2]];
            var attachId = BitConverter.ToUInt16(new[] {
                metadata[indexes[0]],
                metadata[indexes[1]]
            }, 0);

            return Optional<Attachment>.Of(new Attachment(attachId, attachDurability));
        }

        public static Optional<EFiremode> GetWeaponFiremode(byte[] metadata) {
            if (metadata.Length < 18) {
                return Optional<EFiremode>.Empty();
            }

            return Optional<EFiremode>.OfNullable((EFiremode) metadata[0xB]);
        }

        public static Optional<byte> GetWeaponAmmo(byte[] metadata) {
            if (metadata.Length < 18) {
                return Optional<byte>.Empty();
            }

            return Optional<byte>.Of(metadata[0xA]);
        }


        public static Optional<Attachment> GetWeaponAttachment(Item weaponItem, AttachmentType type) {
            return GetWeaponAttachment(weaponItem.Metadata, type);
        }

        public static Optional<EFiremode> GetWeaponFiremode(Item weaponItem) {
            return GetWeaponFiremode(weaponItem.Metadata);
        }

        public static Optional<byte> GetWeaponAmmo(Item weaponItem) {
            return GetWeaponAmmo(weaponItem.Metadata);
        }

       /*
            Unturned weapon state structure.

            state[0] = sight id byte 1
            state[1] = sight id byte 2

            state[2] = tactical id byte 1
            state[3] = tactical id byte 2
            
            state[4] = grip id byte 1
            state[5] = grip id byte 2

            state[6] = barrel id byte 1
            state[7] = barrel id byte 2

            state[8] = magazine id byte 1
            state[9] = magazine id byte 2

            state[10] = ammo
            state[11] = firemode
            state[12] = ??

            state[13] = sight durability
            state[14] = tactical durability
            state[15] = grip durability
            state[16] = barrel durability
            state[17] = magazine durability
        */
        public static void SetWeaponAttachment(Item weaponItem, AttachmentType type, Attachment attach) {
            if (weaponItem.Metadata.Length < 18) {
                return;
            }

            switch (type) {
                case AttachmentType.SIGHT:
                    AssembleAttach(weaponItem, 0, 1, 13, attach);
                    break;

                case AttachmentType.TACTICAL:
                    AssembleAttach(weaponItem, 2, 3, 14, attach);
                    break;

                case AttachmentType.GRIP:
                    AssembleAttach(weaponItem, 4, 5, 15, attach);
                    break;

                case AttachmentType.BARREL:
                    AssembleAttach(weaponItem, 6, 7, 16, attach);
                    break;

                case AttachmentType.MAGAZINE:
                    AssembleAttach(weaponItem, 8, 9, 17, attach);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static void SetWeaponFiremode(Item weaponItem, EFiremode firemode) {
            if (weaponItem.Metadata.Length < 18) {
                return;
            }

            weaponItem.Metadata[11] = (byte) firemode;
        }

        public static void SetWeaponAmmo(Item weaponItem, byte ammo) {
            if (weaponItem.Metadata.Length < 18) {
                return;
            }

            weaponItem.Metadata[10] = ammo;
        }

        
        private static void AssembleAttach(Item item, int idx0, int idx1, int idx2, Attachment attach) {
            if (attach == null || attach.AttachmentId == 0) return;

            var attachIdBytes = BitConverter.GetBytes(attach.AttachmentId);

            // 2 bytes for id (uint16)
            item.Metadata[idx0] = attachIdBytes[0];
            item.Metadata[idx1] = attachIdBytes[1];

            // 1 byte for durability (uint8)
            item.Metadata[idx2] = attach.Durability;
        }


        public enum AttachmentType {
            SIGHT,
            BARREL,
            GRIP,
            TACTICAL,
            MAGAZINE
        }

    }

}