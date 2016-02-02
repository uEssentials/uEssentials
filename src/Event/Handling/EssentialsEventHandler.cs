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
using Essentials.Api;
using Essentials.Api.Event;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.I18n;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;

using static Essentials.Commands.CommandSpy;
using static Essentials.Commands.CommandBack;
using static Essentials.Commands.CommandFreeze;
using static Essentials.Commands.CommandTell;

namespace Essentials.Event.Handling
{
     /* typedef */ internal class SkillCache : Dictionary<string, Dictionary<USkill, byte>> {}

    /// <summary>
    /// Misc Essentials eventhandlers
    /// 
    /// Author: leonardosc
    /// </summary>
    internal class EssentialsEventHandler 
    {
        internal static readonly Dictionary<string, DateTime>    LastChatted     = new Dictionary<string, DateTime>();
        internal static readonly SkillCache                      CachedSkills      = new SkillCache();

        [SubscribeEvent( EventType.ROCKET_PERMISSION_REQUESTED )]
        internal void OnPermissionRequested( UnturnedPlayer player, string permission, 
                                             ref bool granted )
        {
            var command = permission.Substring( 1 );

            if ( command.IndexOf( " ", StringComparison.Ordinal ) > -1 )
                command = command.Split( ' ' )[0];

            if ( !EssProvider.CommandManager.HasWithName( command ) ||
                 "ess".EqualsIgnoreCase( command ) ||
                 "essentials".EqualsIgnoreCase( command ) )
            {
                granted = true;
                return;
            }
    
            var reqCommand = EssProvider.CommandManager.GetByName( command );
            var uplayer = UPlayer.From( player );

            if ( reqCommand != null )
            {
                granted = player.HasPermission( reqCommand.Permission );
            }
            else
            {
                granted = EssProvider.CommandManager.HasWithName( command ) 
                            && player.HasPermission( command );
            }

            if ( !granted )
                EssLang.COMMAND_NO_PERMISSION.SendTo( uplayer );
        }

        [SubscribeEvent( EventType.ROCKET_PLAYER_CHATTED )]
        internal void OnPlayerChatted( UnturnedPlayer player,ref Color color, string message,
                                       EChatMode mode, ref bool cancel )
        {
            if ( message.StartsWith( "/" ) && EssProvider.Config.EnableUnknownMessage )
            {
                var command = message.Substring( 1 );

                if ( command.IndexOf( " ", StringComparison.Ordinal ) > -1 )
                    command = command.Split( ' ' )[0];

                if ( EssProvider.CommandManager.HasWithName( command ) )
                    return;
                
                UPlayer.TryGet( player, EssLang.UNKNOWN_COMMAND.SendTo );
                return;
            }

            if ( player.HasPermission( "essentials.antispam.bypass" ) ||
                 !EssProvider.Config.AntiSpam.Enabled ) return;

            var playerName = player.CharacterName;

            if ( !LastChatted.ContainsKey( playerName ) )
            {
                LastChatted.Add( playerName, DateTime.Now );
                return;
            }

            var interval = EssProvider.Config.AntiSpam.Interval;

            if ( (DateTime.Now - LastChatted[playerName]).TotalSeconds < interval )
            {
                EssLang.CHAT_ANTI_SPAM.SendTo( UPlayer.From( player ) );

                cancel = true;
                return;
            }

            LastChatted[playerName] = DateTime.Now;
        }

        [SubscribeEvent( EventType.ROCKET_PLAYER_DEATH )]
        internal void OnPlayerDeath( UnturnedPlayer player, EDeathCause cause,
                                     ELimb limb, CSteamID murderer )
        {
            var uplayer = UPlayer.From( player );
            var displayName = uplayer.DisplayName;
            var skillNameIdx = 0;

            string[] skillNames =
            {
                "OVERKILL",
                "SHARPSHOOTER",
                "DEXTERITY",
                "CARDIO",
                "EXERCISE", 
                "DIVING",
                "PARKOUR",
                "SNEAKYBEAKY",
                "VITALITY",
                "IMMUNITY", 
                "TOUGHNESS",
                "STRENGTH",
                "WARMBLOODED",
                "SURVIVAL",
                "HEALING", 
                "CRAFTING",
                "OUTDOORS",
                "COOKING",
                "FISHING",
                "AGRICULTURE",
                "MECHANIC",
                "ENGINEER"
            };
            
            var skillValues = (
                from skill 
                in USkill.Skills
                let permss = $"essentials.keepskill.{skillNames[skillNameIdx++]}"
                where uplayer.HasPermission( permss )
                select skill
            ).ToDictionary( skill => skill, skill => uplayer.GetSkillLevel( skill ) );

            if ( skillValues.Count == 0 ) return;
            
            if ( CachedSkills.ContainsKey( displayName) )
                CachedSkills[displayName] = skillValues;
            else
                CachedSkills.Add( displayName, skillValues ); 
        }

        [SubscribeEvent( EventType.ROCKET_PLAYER_REVIVE )]
        internal void OnPlayerRespawn( UnturnedPlayer player, Vector3 vect, byte angle )
        {
            if ( !CachedSkills.ContainsKey( player.CharacterName ) ) return;

            var uplayer = UPlayer.From( player );

            CachedSkills[ uplayer.DisplayName ].ForEach( pair => 
            {
                uplayer.SetSkillLevel( pair.Key, pair.Value );
            });
        }

        [SubscribeEvent( EventType.ROCKET_PLAYER_DISCONNECTED )]
        internal void OnPlayerDisconnect( UnturnedPlayer player )
        {
            var displayName = player.CharacterName;

            Spies           .Remove( displayName );
            CachedSkills    .Remove( displayName );
            BackDict        .Remove( displayName );
            AntiFlood       .Remove( displayName );
            Conversations   .Remove( displayName );
        }
    }
}