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
using System.Collections.Generic;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Core;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

// ReSharper disable ForCanBeConvertedToForeach

namespace Essentials.Api.Unturned
{
    //TODO improve factory methods.
    //TODO: Remove totally Rocketplayer dependency.

    public class UPlayer : ICommandSource
    {   
        public UnturnedPlayer       RocketPlayer    { get; }
        public bool                 IsConsole       => false;
        public string               DisplayName     => CharacterName;
        public string               Id              => CSteamId.m_SteamID.ToString();
        public string               CharacterName   => RocketPlayer.CharacterName;
        public string               SteamName       => SteamPlayer.SteamPlayerID.SteamName;
        public float                Rotation        => RocketPlayer.Rotation;
        public bool                 IsOnGround      => UnturnedPlayer.movement.isGrounded;
        public byte                 Stamina         => RocketPlayer.Stamina;
        public bool                 IsDead          => RocketPlayer.Dead;
        public bool                 IsPro           => RocketPlayer.IsPro;
        public bool                 IsInVehicle     => CurrentVehicle != null;
        public bool                 IsFreezing      => RocketPlayer.Freezing;
        public uint                 Ping            => (uint) (RocketPlayer.Player.SteamChannel.SteamPlayer.ping*1000);
        public SteamChannel         Channel         => UnturnedPlayer.SteamChannel;
        public List<string>         Permissions     => R.Permissions.GetPermissions( RocketPlayer ).Select( p => p.Name ).ToList();
        public Player               UnturnedPlayer  => RocketPlayer.Player;
        public SteamPlayer          SteamPlayer     => RocketPlayer.Player.SteamChannel.SteamPlayer;
        public Vector3              Position        => RocketPlayer.Position;
        public PlayerInventory      Inventory       => RocketPlayer.Inventory;
        public InteractableVehicle  CurrentVehicle  => UnturnedPlayer.movement.getVehicle();
        public CSteamID             CSteamId        => RocketPlayer.CSteamID;
        public PlayerMovement       Movement        => UnturnedPlayer.movement;
        public PlayerLook           Look            => UnturnedPlayer.look;
        public PlayerClothing       Clothing        => UnturnedPlayer.clothing;
        public PlayerLife           Life            => UnturnedPlayer.life;
        public PlayerEquipment      Equipment       => UnturnedPlayer.equipment;

        internal UPlayer( UnturnedPlayer player )
        {
            RocketPlayer = player;
        }

        public byte Health
        {
            get
            {
                return RocketPlayer.Health;
            }
            set
            {
                var playerHealth = RocketPlayer.Health;

                if ( playerHealth > value )
                {
                    RocketPlayer.Damage(
                        (byte) (playerHealth - value),
                        Vector3.zero,
                        EDeathCause.BLEEDING,
                        ELimb.SPINE,
                        CSteamId
                    );
                }
                else
                {
                    RocketPlayer.Heal( (byte) (playerHealth - value) );
                }

                RocketPlayer.Bleeding = false;
            }
        }

        public byte Hunger
        {
            get
            {
                return RocketPlayer.Hunger;
            }
            set
            {
                RocketPlayer.Hunger = (byte) (100 - value);
            }
        }

        public byte Thirst
        {
            get
            {
                return RocketPlayer.Thirst;
            }
            set
            {
                RocketPlayer.Thirst = (byte) (100 - value);
            }
        }

        public byte Infection
        {
            get
            {
                return RocketPlayer.Infection;
            }
            set
            {
                RocketPlayer.Infection = (byte) (100 - value);
            }
        }

        public bool IsBleeding
        {
            get
            {
                return RocketPlayer.Bleeding;
            }
            set
            {
                RocketPlayer.Bleeding = value;
            }
        }

        public bool IsBroken
        {
            get
            {
                return RocketPlayer.Broken;
            }
            set
            {
                RocketPlayer.Broken = value;
            }
        }

        public bool IsAdmin
        {
            get
            {
                return RocketPlayer.IsAdmin;
            }
            set
            {
                RocketPlayer.Admin( value );
            }
        }

        public void Teleport( Vector3 pos )
        {
            Teleport( pos, RocketPlayer.Rotation );
        }

        public void Teleport( Vector3 pos, float rotation )
        {
            RocketPlayer.Teleport( pos, rotation );
        }

        public bool HasPermission( string permission )
        {
            return IsAdmin || RocketPlayer.HasPermission( permission );
        }

        public void SendMessage( object message, Color color )
        {
            UnturnedChat.Say( RocketPlayer, message?.ToString() ?? "null", color );
        }

        public void SendMessage( object message )
        {
            UnturnedChat.Say( RocketPlayer, message?.ToString() ?? "null" );
        }

        public bool GiveItem( Item item, ushort amount, bool dropIfInventoryIsFull = false )
        {
            var added = false;

            for ( var i = 0; i < amount; i++ )
            {
                var clone = new Item( item.ItemID, item.Amount, item.Durability, item.Metadata );

                added = UnturnedPlayer.inventory.tryAddItem( clone, true );

                if ( !added && dropIfInventoryIsFull )
                {
                    ItemManager.dropItem( clone, Position, true, Dedicator.isDedicated, true );
                }
            }

            return added;
        }

        public bool GiveItem( Item item, bool dropIfInventoryIsFull = false )
        {
            return GiveItem( item, item.Amount, dropIfInventoryIsFull );
        }

        public void DispatchCommand( string command )
        {
            if ( string.IsNullOrEmpty( command ) ) return;

            if ( command.StartsWith( "/" ) )
                command = command.Substring( 1 );

            Commander.execute( CSteamId, command );
        }

        public void Kill()
        {
            EPlayerKill outKill;

            UnturnedPlayer.PlayerLife.askDamage(
                100,
                Position.normalized,
                EDeathCause.KILL,
                ELimb.SKULL,
                CSteamID.Nil,
                out outKill
            );
        }

        public void Suicide()
        {
            RocketPlayer.Suicide();
        }

        public void Kick( string reason )
        {
            Provider.kick( CSteamId, reason );
        }

        public void Kick()
        {
            Kick( "Undefined" );
        }

        public void SetSkillLevel( USkill uSkill, byte value )
        {
            GetSkill( uSkill ).level = value;
            UnturnedPlayer.skills.askSkills( CSteamId );
        }

        public byte GetSkillLevel( USkill uSkill )
        {
            return GetSkill( uSkill ).level;
        }

        public Skill GetSkill( USkill uSkill )
        {
            var skills = UnturnedPlayer.skills;
            return skills.skills[uSkill.SpecialityIndex][uSkill.SkillIndex];
        }

        public Vector3? GetEyePosition( float distance, int masks )
        {
            RaycastHit raycast;
            Physics.Raycast( Look.aim.position, Look.aim.forward, out raycast, distance, masks );

            if ( raycast.transform == null )
                return null;

            return raycast.point;
        }

        public Vector3? GetEyePosition( float distance )
        {
            return GetEyePosition( distance, RayMasks.BLOCK_COLLISION & ~(1 << 0x15) );
        }

        public bool HasComponent<T>() where T : Component
        {
            return GetComponent<T>() != null;
        }

        public bool HasComponent( Type componentType )
        {
            return GetComponent( componentType ) != null;
        }

        public void RemoveComponent<T>() where T : Component
        {
            UnityEngine.Object.Destroy( GetComponent<T>(  ) );
        }

        public void RemoveComponent( Type componentType )
        {
            UnityEngine.Object.Destroy( GetComponent( componentType ) );
        }

        public T AddComponent<T>() where T : Component
        {
            return UnturnedPlayer.gameObject.AddComponent<T>();
        }

        public Component AddComponent( Type componentType )
        {
            return UnturnedPlayer.gameObject.AddComponent( componentType );
        }

        public T GetComponent<T>() where T : Component
        {
            return UnturnedPlayer.gameObject.GetComponent<T>();
        }

        public Component GetComponent( Type componentType )
        {
            return UnturnedPlayer.gameObject.GetComponent( componentType );
        }

        public static UPlayer From( UnturnedPlayer player )
        {
            return player == null
                ? null
                : From( player.Player );
        }

        public static UPlayer From( Player player )
        {
            return player == null
                ? null
                : From( player.SteamChannel.SteamPlayer.SteamPlayerID.CharacterName );
        }

        public static UPlayer From( string name )
        {
            Player player;

            if ( name == null || (player = PlayerTool.getPlayer( name.ToLower() )) == null )
                return null;

            foreach ( var connectedPlayer in EssCore.Instance.ConnectedPlayers )
            {
                if ( connectedPlayer.UnturnedPlayer == player )
                {
                    return connectedPlayer;
                }
            }

            var ret = new UPlayer( Rocket.Unturned.Player.UnturnedPlayer.FromPlayer( player ) );
            EssCore.Instance.ConnectedPlayers.Add( ret );

            return ret;
        }

        public static UPlayer From( CSteamID csteamId )
        {
            return csteamId == CSteamID.Nil
                ? null
                : From( PlayerTool.getPlayer( csteamId ) );
        }

        public static UPlayer From( SteamPlayer player )
        {
            return player == null ? null : From( player.SteamPlayerID.CSteamID );
        }

        /// <summary>
        /// Tries to find a player, if found, it will execute the <paramref name="consumer"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="consumer"></param>
        public static bool TryGet( CSteamID id, Action<UPlayer> consumer )
        {
            var player = From( id );

            if ( player != null )
            {
                consumer( player );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found, it will execute the <paramref name="consumer"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="consumer"></param>
        public static bool TryGet( string name, Action<UPlayer> consumer )
        {
            var player = From( name );

            if ( player != null )
            {
                consumer( player );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found, it will execute the <paramref name="consumer"/>
        /// </summary>
        /// <param name="rocketPlayer"></param>
        /// <param name="consumer"></param>
        public static bool TryGet( UnturnedPlayer rocketPlayer, Action<UPlayer> consumer )
        {
            var player = From( rocketPlayer );

            if ( player != null )
            {
                consumer( player );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found, it will execute the <paramref name="consumer"/>
        /// </summary>
        /// <param name="unturnedPlayer"></param>
        /// <param name="consumer"></param>
        public static bool TryGet( Player unturnedPlayer, Action<UPlayer> consumer )
        {
            var player = From( unturnedPlayer );

            if ( player != null )
            {
                consumer( player );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found, it will execute the <paramref name="consumer"/>
        /// </summary>
        /// <param name="cmdArg"></param>
        /// <param name="consumer"></param>
        public static bool TryGet( ICommandArgument cmdArg, Action<UPlayer> consumer )
        {
            var player = From( cmdArg.ToString() );

            if ( player != null )
            {
                consumer( player );
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public static bool operator ==( UPlayer left, ICommandSource right )
        {
            if ( Equals( left, right ) )
                return true;

            return left?.Id == right?.Id;
        }

        public static bool operator !=( UPlayer left, ICommandSource right )
        {
            return !(left == right);
        }

        protected bool Equals( UPlayer other )
        {
            return Equals( this, other );
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) ) return false;
            if ( ReferenceEquals( this, obj ) ) return true;
            if ( obj.GetType() != GetType() ) return false;

            /*
                TODO; Improve
            */
            return RocketPlayer.Equals( ((UPlayer) obj).RocketPlayer );
        }

        public override int GetHashCode()
        {
            return RocketPlayer.GetHashCode();
        }
    }
}
