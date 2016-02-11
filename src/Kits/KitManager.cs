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
using Essentials.Core.Storage;
using Rocket.Unturned.Items;

namespace Essentials.Kits
{
    public sealed class KitManager
    {
        private Dictionary<string, Kit> KitMap { get; set; }

        private IData<Dictionary<string, Kit>> KitData { get; set; }

        public int Count => KitMap.Count;

        public IEnumerable<Kit> Kits => KitMap.Values; 

        internal KitManager()
        {
            KitMap = new Dictionary<string, Kit>();
            KitData = new JsonKitData();
        }

        public bool Contains( string kitName )
        {
            return KitMap.ContainsKey( kitName.ToLower() );
        }

        public bool Contains( Kit kit )
        {
            return KitMap.ContainsValue( kit );
        }

        public void Load()
        {
            KitMap = KitData.Load();
        }

        public void Add( Kit kit )
        {
            KitMap.Add( kit.Name, kit );
            Save();
        }

        public Kit GetByName( string kitName )
        {
            return Contains( kitName ) 
                   ? KitMap[kitName.ToLower()] 
                   : null;
        }

        public void Save()
        {
            KitData.Save( KitMap );
        }
    }
}