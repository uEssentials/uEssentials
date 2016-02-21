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

using Essentials.Api.Command.Source;
using Essentials.Common;
using Newtonsoft.Json;
using UnityEngine;

namespace Essentials.Warp
{
    /// <summary>
    /// Author: Leonardosc
    /// </summary>
    [JsonObject]
    public sealed class Warp
    {
        /// <summary>
        /// Location in the world of warp
        /// </summary>
        [JsonIgnore]
        public Vector3 Location { get; set; }

        /// <summary>
        /// Rotation of warp
        /// </summary>
        [JsonProperty]
        public float Rotation { get; set; }

        /// <summary>
        /// Name of warp
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// Serializeble Vector3, used by JsonSerializer
        /// </summary>
        [JsonProperty( PropertyName = "Location" )]
        private SerializableVector3 SerializableLocation
        {
            // Used by jsonSerializer
            get { return new SerializableVector3( Location.x, 
                                                  Location.y, 
                                                  Location.z ); }

            set { Location = new Vector3( value.X, value.Y, value.Z ); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of warp</param>
        /// <param name="location">Location of warp</param>
        /// <param name="rotation">Rotation of warp</param>
        public Warp( string name, Vector3 location, float rotation )
        {
            Preconditions.NotNull( name, "Warp name connot be null" );

            Name = name;
            Rotation = rotation;
            SerializableLocation = new SerializableVector3( location.x, 
                                                            location.y, 
                                                            location.z );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source that you want to check if is authorized</param>
        /// <returns>If source has permission to use this warp</returns>
        public bool CanUse( ICommandSource source )
        {
            return source.HasPermission( $"essentials.warps.{Name}" );
        }

        public override string ToString()
        {
            return "Warp{Name= " + Name + ", " +
                   "Location= " + Location + ", " +
                   "Rotation= " + Rotation + "}";
        }
    }

    /// <summary>
    /// Serializeble Vector3, used by JsonSerializer
    /// </summary>
    [JsonObject]
    internal struct SerializableVector3
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public SerializableVector3( float x, float y, float z )
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
