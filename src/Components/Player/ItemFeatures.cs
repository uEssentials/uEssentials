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

using SDG.Unturned;

namespace Essentials.Components.Player {

    public class ItemFeatures : PlayerComponent {

        public bool AutoRepair { get; set; }
        public bool AutoReload { get; set; }

        private readonly PlayerEquipment _equip;
        private ushort _lastArrowId;

        public ItemFeatures() {
            _equip = Player.Equipment;
        }

        protected override void SafeFixedUpdate() {
            var holdId = _equip.HoldingItemID;
            if (holdId == 0) return;

            /*
                Weapon feature (Auto reload)
            */
            if (AutoReload && _equip.state.Length >= 18) {
                // Save last arrow id
                if (_equip.state[10] == 1 && (holdId == 346 || holdId == 353 || holdId == 355 || 
                                              holdId == 356 || holdId == 357)) {
                    _lastArrowId = (ushort) (_equip.state[8] | _equip.state[9] << 8);
                }

                if (_equip.state[10] == 0) {
                    switch (holdId) {
                        case 519: // Rocket Laucher
                            _equip.state[8] = 8;
                            _equip.state[9] = 2;
                            _equip.state[10] = 1;
                            break;

                        case 300: // Shadowstalker
                            _equip.state[8] = 45;
                            _equip.state[9] = 1;
                            _equip.state[10] = 1;
                            break;

                        case 346: // Crossbow
                        case 353: // Maple bow
                        case 355: // Birch bow
                        case 356: // Pine Bow
                        case 357: // Compound bow
                            _equip.state[8] = (byte) (_lastArrowId == 0 ? 91 : _lastArrowId);
                            _equip.state[9] = (byte) (_lastArrowId == 0 ? 1 : _lastArrowId >> 8);
                            _equip.state[10] = 1;
                            _equip.state[17] = 100; // Durability (for arrows)
                            break;

                        default:
                            var magazineId = (ushort) (_equip.state[8] | _equip.state[9] << 8);
                            var maga = Assets.find(EAssetType.ITEM, magazineId) as ItemMagazineAsset;
                            _equip.state[10] = maga?.amount ?? 0;
                            break;
                    }
                    _equip.sendUpdateState();
                }
            }

            /*
                Item feature (Auto repair)
            */
            if (AutoRepair) {
                if (_equip.quality < 90) {
                    _equip.quality = 100;
                    _equip.sendUpdateQuality();
                }

                if (_equip.state.Length > 16 && _equip.state[16] < 10) {
                    _equip.state[16] = 100;
                    _equip.sendUpdateState();
                }
            }
        }

    }

}