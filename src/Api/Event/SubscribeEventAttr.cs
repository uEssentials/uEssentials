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

// ReSharper disable InconsistentNaming

//TODO: Fix PERMISION_REQUESTED, fr34ky broke it in Rocket 4.9.0.0

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
                    case EventType.PLAYER_UPDATE_BLEEDING:
                        return "OnPlayerUpdateBleeding";
                    case EventType.PLAYER_UPDATE_BROKEN:
                        return "OnPlayerUpdateBroken";
                    case EventType.PLAYER_UPDATE_POSITION:
                        return "OnPlayerUpdatePosition";
                    case EventType.PLAYER_UPDATE_LIFE:
                        return "OnPlayerUpdateLife";
                    case EventType.PLAYER_UPDATE_FOOD:
                        return "OnPlayerUpdateFood";
                    case EventType.PLAYER_UPDATE_HEALTH:
                        return "OnPlayerUpdateHealth";
                    case EventType.PLAYER_UPDATE_VIRUS:
                        return "OnPlayerUpdateVirus";
                    case EventType.PLAYER_UPDATE_WATER:
                        return "OnPlayerUpdateWater";
                    case EventType.PLAYER_UPDATE_STANCE:
                        return "OnPlayerUpdateStance";
                    case EventType.PLAYER_UPDATE_STAT:
                        return "OnPlayerUpdateStat";
                    case EventType.PLAYER_UPDATE_EXPERIENCE:
                        return "OnPlayerUpdateExperience";
                    case EventType.PLAYER_UPDATE_STAMINA:
                        return "OnPlayerUpdateStamina";
                    case EventType.PLAYER_UPDATE_GESTURE:
                        return "OnPlayerUpdateGesture";

                    case EventType.PLAYER_INVENTORY_UPDATED:
                        return "OnPlayerInventoryUpdated";
                    case EventType.PLAYER_INVENTORY_RESIZED:
                        return "OnPlayerInventoryResized";
                    case EventType.PLAYER_INVENTORY_REMOVED:
                        return "OnPlayerInventoryRemoved";
                    case EventType.PLAYER_INVENTORY_ADDED:
                        return "OnPlayerInventoryAdded";

                    case EventType.PLAYER_DEATH:
                        return "OnPlayerDeath";
                    case EventType.PLAYER_DEAD:
                        return "OnPlayerDead";
                    case EventType.PLAYER_REVIVE:
                        return "OnPlayerRevive";
                    case EventType.PLAYER_CHATTED:
                        return "OnPlayerChatted";
                    case EventType.PLAYER_WEAR:
                        return "OnPlayerWear";

                    case EventType.PLAYER_CONNECTED:
                        return "OnPlayerConnected";
                    case EventType.PLAYER_DISCONNECTED:
                        return "OnPlayerDisconnected";
                    case EventType.SERVER_SHUTDOWN:
                        return "OnShutdown";

//                    case EventType.PERMISSION_REQUESTED:
//                        return "OnPermissionRequested";

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
                case EventType.PLAYER_UPDATE_BLEEDING:
                case EventType.PLAYER_UPDATE_BROKEN:
                case EventType.PLAYER_UPDATE_POSITION:
                case EventType.PLAYER_UPDATE_LIFE:
                case EventType.PLAYER_UPDATE_FOOD:
                case EventType.PLAYER_UPDATE_HEALTH:
                case EventType.PLAYER_UPDATE_VIRUS:
                case EventType.PLAYER_UPDATE_WATER:
                case EventType.PLAYER_UPDATE_STANCE:
                case EventType.PLAYER_UPDATE_STAT:
                case EventType.PLAYER_UPDATE_EXPERIENCE:
                case EventType.PLAYER_UPDATE_STAMINA:
                case EventType.PLAYER_UPDATE_GESTURE:
                case EventType.PLAYER_INVENTORY_UPDATED:
                case EventType.PLAYER_INVENTORY_RESIZED:
                case EventType.PLAYER_INVENTORY_REMOVED:
                case EventType.PLAYER_INVENTORY_ADDED:
                case EventType.PLAYER_DEATH:
                case EventType.PLAYER_DEAD:
                case EventType.PLAYER_REVIVE:
                case EventType.PLAYER_CHATTED:
                case EventType.PLAYER_WEAR:
                    TargetType = typeof (UnturnedPlayerEvents);
                    break;

                case EventType.PLAYER_CONNECTED:
                case EventType.PLAYER_DISCONNECTED:
                case EventType.SERVER_SHUTDOWN:
                    TargetType = U.Events;
                    break;
                
//                case EventType.PERMISSION_REQUESTED:
//                    TargetType = typeof (UnturnedPermissions);
//                    break;

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
        PLAYER_UPDATE_BLEEDING,

        /// <summary>
        /// Signature: (UnturnedPlayer player, bool broken)
        /// </summary>
        PLAYER_UPDATE_BROKEN,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Vector3 position)
        /// </summary>
        PLAYER_UPDATE_POSITION,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        PLAYER_UPDATE_LIFE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        PLAYER_UPDATE_FOOD,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        PLAYER_UPDATE_HEALTH,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        PLAYER_UPDATE_VIRUS,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        PLAYER_UPDATE_WATER,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte stance)
        /// </summary>
        PLAYER_UPDATE_STANCE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, EPlayerStat stat)
        /// </summary>
        PLAYER_UPDATE_STAT,

        /// <summary>
        /// Signature: (UnturnedPlayer player, uint experience)
        /// </summary>
        PLAYER_UPDATE_EXPERIENCE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, byte amount)
        /// </summary>
        PLAYER_UPDATE_STAMINA,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        /// </summary>
        PLAYER_INVENTORY_UPDATED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte O, byte U)
        /// </summary>
        PLAYER_INVENTORY_RESIZED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        /// </summary>
        PLAYER_INVENTORY_REMOVED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        /// </summary>
        PLAYER_INVENTORY_ADDED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        /// </summary>
        PLAYER_DEATH,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Vector3 position)
        /// </summary>
        PLAYER_DEAD,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Vector3 position, byte angle)
        /// </summary>
        PLAYER_REVIVE,

        /// <summary>
        /// Signature: (UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        /// </summary>
        PLAYER_CHATTED,

        /// <summary>
        /// Signature: (UnturnedPlayer player, Wearables wear, ushort id, byte? quality)
        /// </summary>
        PLAYER_WEAR,

        /// <summary>
        /// Signature: (UnturnedPlayer player)
        /// </summary>
        PLAYER_CONNECTED,

        /// <summary>
        /// Signature: (UnturnedPlayer player)
        /// </summary>
        PLAYER_DISCONNECTED,

        /// <summary>
        /// Signature: ()
        /// </summary>
        SERVER_SHUTDOWN,

 /*       /// <summary>
        /// Signature: (UnturnedPlayer player, string permission, ref bool granted)
        /// </summary>
        PERMISSION_REQUESTED, */

        /// <summary>
        /// Signature: (UnturnedPlayer, UnturnedPlayerEvents.PlayerGesture)
        /// </summary>
        PLAYER_UPDATE_GESTURE
    }
}
