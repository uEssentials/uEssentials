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

using System.Collections.Generic;

namespace Essentials.Api.Metadata {

    public class MetadataStore : IMetadataStore {

        private readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();

        public object this[string key] {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        public bool Has(string key) {
            return _metadata.ContainsKey(key);
        }

        public void Set(string key, object value) {
            this[key] = value;
        }

        public object Get(string key) {
            return this[key];
        }

        public T GetOrDefault<T>(string key, T defaultValue)
        {
            if (_metadata.TryGetValue(key, out var val)) {
                return (T) val;
            }
            return defaultValue;
        }

        public T Get<T>(string key) {
            return (T) Get(key);
        }

        public bool Remove(string key) {
            return _metadata.Remove(key);
        }

    }

}