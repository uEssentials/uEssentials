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

using SDG.Unturned;
using Essentials.Api;

namespace Essentials.Components.Player
{
    public class ItemFeatures : PlayerComponent
    {
        public bool AutoRepair { get; set; }
        public bool AutoReload { get; set; }

        private ushort _lastArrowId;

        protected override void SafeFixedUpdate()
        {
            var equip = Player.Equipment;
            var itemInHandId = equip.itemID;

            if (itemInHandId == 0)
            {
                return; // Is not holding anything.
            }

            if (AutoReload && equip.state.Length >= 18)
            {
                DoAutoReload(equip, itemInHandId);
            }

            if (AutoRepair)
            {
                DoAutoRepair(equip);
            }
        }

        private void DoAutoRepair(PlayerEquipment equip)
        {
            if (equip.quality < UEssentials.Config.ItemFeatures.RepairPercentage)
            {
                equip.quality = 100;
                equip.sendUpdateQuality();
            }

            // This is for Barrel.
            if (equip.state.Length > 16 && equip.state[16] < UEssentials.Config.ItemFeatures.RepairPercentage)
            {
                equip.state[16] = 100;
                equip.sendUpdateState();
            }
        }

        private void DoAutoReload(PlayerEquipment equip, ushort itemId)
        {
            // NOTE:
            //  equip.state[8]  = magazine ID low bytes
            //  equip.state[9]  = magazine ID high bytes
            //  equip.state[10] = ammo
            var ammo = equip.state[10];
            var ammoId = (ushort) (equip.state[8] | equip.state[9] << 8);

            // Save the last arrow id used by this bow, so we can
            // use the same arrow later.
            if (ammo == 1 && IsBow(itemId))
            {
                _lastArrowId = ammoId;
            }

            if (HasSingleBullet(itemId))
            {
                if (ammo == 1)
                {
                    return;
                }

                if (IsBow(itemId))
                {
                    ammoId = _lastArrowId == 0 ? (ushort) 347 : _lastArrowId;
                    equip.state[17] = 100; // Arrow durability
                }
                else if (itemId == 519 || itemId == 3517)
                {
                    // Lancer & Rocket Launcher
                    ammoId = 520;
                }
                else if (itemId == 300)
                {
                    // Shadowstalker
                    ammoId = 301;
                }

                equip.state[8] = (byte) (ammoId);
                equip.state[9] = (byte) (ammoId >> 8);
                equip.state[10] = 1;
                equip.sendUpdateState();
                return;
            }

            var magazine = Assets.find(EAssetType.ITEM, ammoId) as ItemMagazineAsset;

            if (magazine == null)
            {
                return;
            }

            if (((float) ammo / magazine.amount) * 100 <= UEssentials.Config.ItemFeatures.ReloadPercentage)
            {
                equip.state[10] = magazine.amount;
                equip.sendUpdateState();
            }
        }

        private bool IsBow(ushort itemId) =>
            (itemId == 346 || itemId == 353 || itemId == 355 || itemId == 356 || itemId == 357);

        private bool HasSingleBullet(ushort itemId) =>
            IsBow(itemId) || itemId == 519 || itemId == 3517 || itemId == 300;
    }
}