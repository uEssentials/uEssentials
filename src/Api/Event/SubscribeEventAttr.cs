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
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Permissions;

// ReSharper disable InconsistentNaming

namespace Essentials.Api.Event
{
    /// <summary>
    /// Author: Leonardosc
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class SubscribeEvent : Attribute
    {
        /// <summary>
        /// Name of rocket event delegate
        /// </summary>
        public string TargetFieldName
        {
            get
            {
                switch (EventType)
                {
                    case EventType.ROCKET_PLAYER_UPDATE_BLEEDING:
                        return "OnPlayerUpdateBleeding";
                    case EventType.ROCKET_PLAYER_UPDATE_BROKEN:
                        return "OnPlayerUpdateBroken";
                    case EventType.ROCKET_PLAYER_UPDATE_POSITION:
                        return "OnPlayerUpdatePosition";
                    case EventType.ROCKET_PLAYER_UPDATE_LIFE:
                        return "OnPlayerUpdateLife";
                    case EventType.ROCKET_PLAYER_UPDATE_FOOD:
                        return "OnPlayerUpdateFood";
                    case EventType.ROCKET_PLAYER_UPDATE_HEALTH:
                        return "OnPlayerUpdateHealth";
                    case EventType.ROCKET_PLAYER_UPDATE_VIRUS:
                        return "OnPlayerUpdateVirus";
                    case EventType.ROCKET_PLAYER_UPDATE_WATER:
                        return "OnPlayerUpdateWater";
                    case EventType.ROCKET_PLAYER_UPDATE_STANCE:
                        return "OnPlayerUpdateStance";
                    case EventType.ROCKET_PLAYER_UPDATE_STAT:
                        return "OnPlayerUpdateStat";
                    case EventType.ROCKET_PLAYER_UPDATE_EXPERIENCE:
                        return "OnPlayerUpdateExperience";
                    case EventType.ROCKET_PLAYER_UPDATE_STAMINA:
                        return "OnPlayerUpdateStamina";

                    case EventType.ROCKET_PLAYER_INVENTORY_UPDATED:
                        return "OnPlayerInventoryUpdated";
                    case EventType.ROCKET_PLAYER_INVENTORY_RESIZED:
                        return "OnPlayerInventoryResized";
                    case EventType.ROCKET_PLAYER_INVENTORY_REMOVED:
                        return "OnPlayerInventoryRemoved";
                    case EventType.ROCKET_PLAYER_INVENTORY_ADDED:
                        return "OnPlayerInventoryAdded";

                    case EventType.ROCKET_PLAYER_DEATH:
                        return "OnPlayerDeath";
                    case EventType.ROCKET_PLAYER_DEAD:
                        return "OnPlayerDead";
                    case EventType.ROCKET_PLAYER_REVIVE:
                        return "OnPlayerRevive";
                    case EventType.ROCKET_PLAYER_CHATTED:
                        return "OnPlayerChatted";
                    case EventType.ROCKET_PLAYER_WEAR:
                        return "OnPlayerWear";

                    case EventType.ROCKET_PLAYER_CONNECTED:
                        return "OnPlayerConnected";
                    case EventType.ROCKET_PLAYER_DISCONNECTED:
                        return "OnPlayerDisconnected";
                    case EventType.ROCKET_SERVER_SHUTDOWN:
                        return "OnShutdown";

                    case EventType.ROCKET_PERMISSION_REQUESTED:
                        return "OnPermissionRequested";

                    default:
                        return "Unknown";
                }
            }
        }

        /// <summary>
        /// Type of event event
        /// </summary>
        public EventType EventType;

        /// <summary>
        /// Instance or Type of target event
        /// </summary>
        public object TargetType;

        public SubscribeEvent( EventType eventType )
        {
            EventType = eventType;

            switch (eventType)
            {
                case EventType.ROCKET_PLAYER_UPDATE_BLEEDING:
                case EventType.ROCKET_PLAYER_UPDATE_BROKEN:
                case EventType.ROCKET_PLAYER_UPDATE_POSITION:
                case EventType.ROCKET_PLAYER_UPDATE_LIFE:
                case EventType.ROCKET_PLAYER_UPDATE_FOOD:
                case EventType.ROCKET_PLAYER_UPDATE_HEALTH:
                case EventType.ROCKET_PLAYER_UPDATE_VIRUS:
                case EventType.ROCKET_PLAYER_UPDATE_WATER:
                case EventType.ROCKET_PLAYER_UPDATE_STANCE:
                case EventType.ROCKET_PLAYER_UPDATE_STAT:
                case EventType.ROCKET_PLAYER_UPDATE_EXPERIENCE:
                case EventType.ROCKET_PLAYER_UPDATE_STAMINA:
                case EventType.ROCKET_PLAYER_INVENTORY_UPDATED:
                case EventType.ROCKET_PLAYER_INVENTORY_RESIZED:
                case EventType.ROCKET_PLAYER_INVENTORY_REMOVED:
                case EventType.ROCKET_PLAYER_INVENTORY_ADDED:
                case EventType.ROCKET_PLAYER_DEATH:
                case EventType.ROCKET_PLAYER_DEAD:
                case EventType.ROCKET_PLAYER_REVIVE:
                case EventType.ROCKET_PLAYER_CHATTED:
                case EventType.ROCKET_PLAYER_WEAR:
                    TargetType = typeof (UnturnedPlayerEvents);
                    break;

                case EventType.ROCKET_PLAYER_CONNECTED:
                case EventType.ROCKET_PLAYER_DISCONNECTED:
                case EventType.ROCKET_SERVER_SHUTDOWN:
                    TargetType = U.Events;
                    break;
                
                case EventType.ROCKET_PERMISSION_REQUESTED:
                    TargetType = typeof (UnturnedPermissions);
                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof( eventType ), 
                                                           eventType, null );
            }
        }
    }

    public enum EventType
    {
        /// <summary>
        /// Signature: (UnturnedPlayer player, bool bleeding)
        /// </summary>
        ROCKET_PLAYER_UPDATE_BLEEDING,

        /// <summary>
        /// Signature: (UnturnedPlayer player, bool broken)
        /// </summary>
        ROCKET_PLAYER_UPDATE_BROKEN,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Vector3 position)
        /// </summary>
        ROCKET_PLAYER_UPDATE_POSITION,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        ROCKET_PLAYER_UPDATE_LIFE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        ROCKET_PLAYER_UPDATE_FOOD,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        ROCKET_PLAYER_UPDATE_HEALTH,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        ROCKET_PLAYER_UPDATE_VIRUS,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        ROCKET_PLAYER_UPDATE_WATER,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte stance)
        /// </summary>
        ROCKET_PLAYER_UPDATE_STANCE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, EPlayerStat stat)
        /// </summary>
        ROCKET_PLAYER_UPDATE_STAT,

        /// <summary>
        /// Signature: (UnturnedPlayer player, uint experience)
        /// </summary>
        ROCKET_PLAYER_UPDATE_EXPERIENCE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        ROCKET_PLAYER_UPDATE_STAMINA,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        /// </summary>
        ROCKET_PLAYER_INVENTORY_UPDATED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte O, byte U)
        /// </summary>
        ROCKET_PLAYER_INVENTORY_RESIZED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        /// </summary>
        ROCKET_PLAYER_INVENTORY_REMOVED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        /// </summary>
        ROCKET_PLAYER_INVENTORY_ADDED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        /// </summary>
        ROCKET_PLAYER_DEATH,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Vector3 position)
        /// </summary>
        ROCKET_PLAYER_DEAD,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Vector3 position, byte angle)
        /// </summary>
        ROCKET_PLAYER_REVIVE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        /// </summary>
        ROCKET_PLAYER_CHATTED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Wearables wear, ushort id, byte? quality)
        /// </summary>
        ROCKET_PLAYER_WEAR,

        /// <summary>
        /// Signature: (UnturnedPlayer player)
        /// </summary>
        ROCKET_PLAYER_CONNECTED,

        /// <summary>
        /// Signature: (UnturnedPlayer player)
        /// </summary>
        ROCKET_PLAYER_DISCONNECTED,

        /// <summary>
        /// Signature: ()
        /// </summary>
        ROCKET_SERVER_SHUTDOWN,

        /// <summary>
        /// Signature: (UnturnedPlayer player, string permission, ref bool granted)
        /// </summary>
        ROCKET_PERMISSION_REQUESTED,
    }
}
