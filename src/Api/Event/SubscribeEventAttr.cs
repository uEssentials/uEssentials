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
using Essentials.Event;
using Rocket.Unturned;
using Rocket.Unturned.Events;

namespace Essentials.Api.Event {

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubscribeEvent : Attribute {

        /// <summary>
        /// Type of event event
        /// </summary>
        public EventType EventType;

        /// <summary>
        /// Instance or Type of target event
        /// </summary>
        public object DelegateOwner { get; }

        public string DelegateName { get; }

        public SubscribeEvent(EventType eventType) {
            EventType = eventType;

            switch (eventType) {
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
                    DelegateOwner = typeof(UnturnedPlayerEvents);
                    break;

                case EventType.PLAYER_CONNECTED:
                case EventType.PLAYER_DISCONNECTED:
                case EventType.SERVER_SHUTDOWN:
                    DelegateOwner = U.Events;
                    break;

                case EventType.ESSENTIALS_COMMAND_POS_EXECUTED:
                case EventType.ESSENTIALS_COMMAND_PRE_EXECUTED:
                    DelegateOwner = typeof(EssentialsEvents);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType),
                        eventType, null);
            }

            switch (EventType) {
                case EventType.PLAYER_UPDATE_BLEEDING:
                    DelegateName = "OnPlayerUpdateBleeding";
                    break;
                case EventType.PLAYER_UPDATE_BROKEN:
                    DelegateName = "OnPlayerUpdateBroken";
                    break;
                case EventType.PLAYER_UPDATE_POSITION:
                    DelegateName = "OnPlayerUpdatePosition";
                    break;
                case EventType.PLAYER_UPDATE_LIFE:
                    DelegateName = "OnPlayerUpdateLife";
                    break;
                case EventType.PLAYER_UPDATE_FOOD:
                    DelegateName = "OnPlayerUpdateFood";
                    break;
                case EventType.PLAYER_UPDATE_HEALTH:
                    DelegateName = "OnPlayerUpdateHealth";
                    break;
                case EventType.PLAYER_UPDATE_VIRUS:
                    DelegateName = "OnPlayerUpdateVirus";
                    break;
                case EventType.PLAYER_UPDATE_WATER:
                    DelegateName = "OnPlayerUpdateWater";
                    break;
                case EventType.PLAYER_UPDATE_STANCE:
                    DelegateName = "OnPlayerUpdateStance";
                    break;
                case EventType.PLAYER_UPDATE_STAT:
                    DelegateName = "OnPlayerUpdateStat";
                    break;
                case EventType.PLAYER_UPDATE_EXPERIENCE:
                    DelegateName = "OnPlayerUpdateExperience";
                    break;
                case EventType.PLAYER_UPDATE_STAMINA:
                    DelegateName = "OnPlayerUpdateStamina";
                    break;
                case EventType.PLAYER_UPDATE_GESTURE:
                    DelegateName = "OnPlayerUpdateGesture";
                    break;

                case EventType.PLAYER_INVENTORY_UPDATED:
                    DelegateName = "OnPlayerInventoryUpdated";
                    break;
                case EventType.PLAYER_INVENTORY_RESIZED:
                    DelegateName = "OnPlayerInventoryResized";
                    break;
                case EventType.PLAYER_INVENTORY_REMOVED:
                    DelegateName = "OnPlayerInventoryRemoved";
                    break;
                case EventType.PLAYER_INVENTORY_ADDED:
                    DelegateName = "OnPlayerInventoryAdded";
                    break;

                case EventType.PLAYER_DEATH:
                    DelegateName = "OnPlayerDeath";
                    break;
                case EventType.PLAYER_DEAD:
                    DelegateName = "OnPlayerDead";
                    break;
                case EventType.PLAYER_REVIVE:
                    DelegateName = "OnPlayerRevive";
                    break;
                case EventType.PLAYER_CHATTED:
                    DelegateName = "OnPlayerChatted";
                    break;
                case EventType.PLAYER_WEAR:
                    DelegateName = "OnPlayerWear";
                    break;

                case EventType.PLAYER_CONNECTED:
                    DelegateName = "OnPlayerConnected";
                    break;
                case EventType.PLAYER_DISCONNECTED:
                    DelegateName = "OnPlayerDisconnected";
                    break;
                case EventType.SERVER_SHUTDOWN:
                    DelegateName = "OnShutdown";
                    break;

                case EventType.ESSENTIALS_COMMAND_PRE_EXECUTED:
                    DelegateName = "OnCommandPreExecute";
                    break;
                case EventType.ESSENTIALS_COMMAND_POS_EXECUTED:
                    DelegateName = "OnCommandPosExecute";
                    break;

                default:
                    throw new Exception("No such TargetFieldName for " + EventType);
            }
        }

    }

    public enum EventType {

        /// <summary>
        /// Signature: (UnturnedPlayer player, bool bleeding)
        /// </summary>
        PLAYER_UPDATE_BLEEDING,

        /// <summary>
        /// Signature: (UnturnedPlayer player, bool broken)
        /// </summary>
        PLAYER_UPDATE_BROKEN,

        /// <summary>
        /// Signature: (UnturnedPlayer player, uint newSeq, Vector3 newPosition, byte newPitch, byte newYaw)
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

        /// <summary>
        /// Signature: (UnturnedPlayer, UnturnedPlayerEvents.PlayerGesture)
        /// </summary>
        PLAYER_UPDATE_GESTURE,

        /// <summary>
        /// Signature: (CommandPreExecutedEvent)
        /// </summary>
        ESSENTIALS_COMMAND_PRE_EXECUTED,

        /// <summary>
        /// Signature: (CommandPosExecutedEvent)
        /// </summary>
        ESSENTIALS_COMMAND_POS_EXECUTED

    }

}