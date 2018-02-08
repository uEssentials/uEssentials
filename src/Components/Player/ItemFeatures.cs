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

namespace Essentials.Components.Player {

    public class ItemFeatures : PlayerComponent {

        public bool AutoRepair { get; set; }
        public bool AutoReload { get; set; }

        private ushort _lastArrowId;

        protected override void SafeFixedUpdate() {
            var equip = Player.Equipment;
            var holdId = equip.itemID;

            if (holdId == 0) return;

            // Weapon feature (Auto reload)
            if (AutoReload && equip.state.Length >= 18) {
                // Save last arrow id
                if (equip.state[10] == 1 && (holdId == 346 || holdId == 353 || holdId == 355 ||
                                              holdId == 356 || holdId == 357)) {
                    _lastArrowId = (ushort) (equip.state[8] | equip.state[9] << 8);
                }

                if (equip.state[10] == 0) {
                    switch (holdId) {
                        case 3517: // Lancer
                        case 519: // Rocket Laucher
                            equip.state[8] = 8;
                            equip.state[9] = 2;
                            equip.state[10] = 1;
                            break;

                        case 300: // Shadowstalker
                            equip.state[8] = 45;
                            equip.state[9] = 1;
                            equip.state[10] = 1;
                            break;

                        case 346: // Crossbow
                        case 353: // Maple bow
                        case 355: // Birch bow
                        case 356: // Pine Bow
                        case 357: // Compound bow
                            equip.state[8] = (byte) (_lastArrowId == 0 ? 91 : _lastArrowId);
                            equip.state[9] = (byte) (_lastArrowId == 0 ? 1 : _lastArrowId >> 8);
                            equip.state[10] = 1;
                            equip.state[17] = 100; // Durability (for arrows)
                            break;

                        default:
                            var magazineId = (ushort) (equip.state[8] | equip.state[9] << 8);
                            var magazine = Assets.find(EAssetType.ITEM, magazineId) as ItemMagazineAsset;
                            if (magazine != null) {
                                equip.state[10] = magazine.amount;
                            }
                            break;
                    }
                    equip.sendUpdateState();
                }
            }

            // Item feature (Auto repair)
            if (AutoRepair) {
                if (equip.quality < 90) {
                    equip.quality = 100;
                    equip.sendUpdateQuality();
                }

                if (equip.state.Length > 16 && equip.state[16] < 10) {
                    equip.state[16] = 100;
                    equip.sendUpdateState();
                }
            }
        }

    }

}