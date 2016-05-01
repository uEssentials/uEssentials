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
using SDG.Unturned;

namespace Essentials.Components.Player
{
    public class ItemFeatures : PlayerComponent
    {
        private readonly PlayerEquipment _equip;
        
        /*
            Used by autoreload for bows.
        */
        private byte _lastArrowId1, _lastArrowId2;

        public bool AutoRepair { get; set; }
        public bool AutoReload { get; set; }

        public ItemFeatures()
        {
            _equip = Player.Equipment;
        }

        private void FixedUpdate()
        {
            if ( _equip.HoldingItemID == 0 ) return;

            /*
                Weapon feature (Auto reload)
            */
            if ( AutoReload && _equip.state.Length >= 18 )
            {
                if ( _equip.state[10] == 1 )
                {
                    switch ( _equip.HoldingItemID )
                    {
                        /*
                            Bows
                        */
                        case 346:
                        case 353:
                        case 355:
                        case 356:
                        case 357:
                            _lastArrowId1 = _equip.state[8];
                            _lastArrowId2 = _equip.state[9];
                            break;
                    }
                }
                
                if ( _equip.state[10] == 0 )
                {
                    var id = BitConverter.ToUInt16(
                        new[] { _equip.state[8], _equip.state[9] },
                        0
                    );

                    var holdId = _equip.HoldingItemID;

                    switch ( holdId )
                    {
                        case 519:
                            _equip.state[8] = 8;
                            _equip.state[9] = 2;
                            _equip.state[10] = 1;
                            goto update;

                        case 300:
                            _equip.state[8] = 45;
                            _equip.state[9] = 1;
                            _equip.state[10] = 1;
                            goto update;
                        
                        case 346:
                        case 353:
                        case 355:
                        case 356:
                        case 357:
                            _equip.state[8] = (byte) (_lastArrowId1 == 0 ? 91 : _lastArrowId1);
                            _equip.state[9] = (byte) (_lastArrowId2 == 0 ? 1 : _lastArrowId2);
                            _equip.state[10] = 1;
                            _equip.state[17] = 100; // Durability (for arrows)
                            goto update;
                    }
                    
                    var maga = Assets.find( EAssetType.ITEM, id ) as ItemMagazineAsset;
                    
                    _equip.state[10] = maga?.amount ?? 0;
                    
                    update:
                    _equip.sendUpdateState();   
                }
            }

            /*
                Item feature (Auto repair)
            */
            if ( AutoRepair )
            {
                if ( _equip.quality < 90 )
                {
                    _equip.quality = 100;
                    _equip.sendUpdateQuality();
                }

                if ( _equip.state.Length > 16 && _equip.state[16] < 10 )
                {
                    _equip.state[16] = 100;
                    _equip.sendUpdateState();
                }
            }
        }
    }
}
