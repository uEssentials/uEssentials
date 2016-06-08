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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Event;
using Essentials.Api.Events;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Commands;
using Essentials.Common;
using Essentials.Components.Player;
using Essentials.Core;
using Essentials.Economy;
using Essentials.I18n;
using Essentials.NativeModules.Kit;
using Essentials.NativeModules.Kit.Commands;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;

namespace Essentials.Event.Handling
{
    internal class EssentialsEventHandler
    {
        internal static readonly Dictionary<string, DateTime>                   LastChatted     = new Dictionary<string, DateTime>();
        internal static readonly Dictionary<string, Dictionary<USkill, byte>>   CachedSkills    = new Dictionary<string, Dictionary<USkill, byte>>();

        [SubscribeEvent( EventType.PLAYER_CHATTED )]
        void OnPlayerChatted( UnturnedPlayer player, ref Color color, string message,
                              EChatMode mode, ref bool cancel )
        {

            /* Rocket does not send "Command not found" if player is admin */
            if ( player.IsAdmin && message.StartsWith( "/" ) && 
                 UEssentials.Config.EnableUnknownMessage )
            {
                var command = message.Substring( 1 );

                if ( command.IndexOf( " ", StringComparison.Ordinal ) > -1 )
                {
                    command = command.Split( ' ' )[0];
                }

                if ( UEssentials.CommandManager.HasWithName( command ) )
                {
                    return;
                }
                
                UPlayer.TryGet( player, p => {
                    EssLang.UNKNOWN_COMMAND.SendTo( p, command );
                } );

                cancel = true;
                return;
            }

            if ( message.StartsWith( "/" ) || player.HasPermission( "essentials.bypass.antispam" ) ||
                 !UEssentials.Config.AntiSpam.Enabled ) return;

            var playerName = player.CharacterName;

            if ( !LastChatted.ContainsKey( playerName ) )
            {
                LastChatted.Add( playerName, DateTime.Now );
                return;
            }

            var interval = UEssentials.Config.AntiSpam.Interval;

            if ( (DateTime.Now - LastChatted[playerName]).TotalSeconds < interval )
            {
                EssLang.CHAT_ANTI_SPAM.SendTo( UPlayer.From( player ) );

                cancel = true;
                return;
            }

            LastChatted[playerName] = DateTime.Now;
        }

        [SubscribeEvent( EventType.PLAYER_DEATH )]
        void OnPlayerDeath( UnturnedPlayer player, EDeathCause cause,
                            ELimb limb, CSteamID murderer )
        {
            var uplayer = UPlayer.From( player );
            var displayName = uplayer.DisplayName;

            // Keep skill
            const string KEEP_SKILL_PERM = "essentials.keepskill.";

            var globalPercentage = -1;
            var playerPermissions = player.GetPermissions().Select( p => p.Name ).ToList();

            /*
                Search for 'global percentage' Ex: (essentials.keepskill.50)
            */
            
            foreach ( var p in playerPermissions.Where( p => p.StartsWith( KEEP_SKILL_PERM ) ) )
            {
                var rawAmount = p.Substring( KEEP_SKILL_PERM.Length );

                if ( rawAmount.Equals( "*" ) ) 
                {
                    globalPercentage = 100;
                    break;    
                }
                if ( int.TryParse( rawAmount, out globalPercentage ) )
                {
                    break;
                }
                globalPercentage = -1;
            }

            /*
                If player has global percentage he will keep all skills.
            */
            var hasGlobalPercentage = (globalPercentage != -1 || player.IsAdmin);
            var skillValues = new Dictionary<USkill, byte>();

            foreach ( var skill in USkill.Skills )
            {
                var currentLevel = uplayer.GetSkillLevel( skill );
                var newLevel = (byte?) null;
                
                if ( hasGlobalPercentage )
                {
                    newLevel = (byte) Math.Round((currentLevel * globalPercentage) / 100.0);
                    goto add;
                }

                /*
                    Search for single percentage.
                */
                var skillPermission = KEEP_SKILL_PERM + skill.Name.ToLowerInvariant() + ".";
                var skillPermission2 = KEEP_SKILL_PERM + skill.Name.ToLowerInvariant();
                var skillPercentage = -1;

                foreach ( var p in playerPermissions )
                {
                    if ( !p.StartsWith( skillPermission ) )
                    {
                        continue;
                    }

                    var rawAmount = p.Substring( skillPermission.Length );

                    if ( int.TryParse( rawAmount, out skillPercentage ) )
                    {
                        break;
                    }

                    skillPercentage = -1;
                }

                if ( skillPercentage != -1 )
                {
                    newLevel =  (byte) Math.Round( (currentLevel * skillPercentage) / 100.0 );
                }
                
                /*
                    Ccheck for 'essentials.keepskill.SKILL'
                */
                if ( !newLevel.HasValue && player.HasPermission( skillPermission2 ) )
                {
                    newLevel = currentLevel;
                }
                
                add:
                if ( newLevel.HasValue )
                {
                    skillValues.Add( skill, newLevel.Value );
                }
            }

            if ( skillValues.Count == 0 ) return;
            
            if ( CachedSkills.ContainsKey( displayName ) )
            {
                CachedSkills[displayName] = skillValues;
            }
            else
            {
                CachedSkills.Add(displayName, skillValues);
            }
        }

        [SubscribeEvent( EventType.PLAYER_REVIVE )]
        void OnPlayerRespawn( UnturnedPlayer player, Vector3 vect, byte angle )
        {
            if ( !CachedSkills.ContainsKey( player.CharacterName ) ) return;

            var uplayer = UPlayer.From( player );

            CachedSkills[ uplayer.DisplayName ].ForEach( pair => {
                uplayer.SetSkillLevel( pair.Key, pair.Value );
            });
        }

        /*
            Generic CONNECTED
        */
        [SubscribeEvent(EventType.PLAYER_CONNECTED)]
        void OnPlayerConnected( UnturnedPlayer player )
        {
            Analytics.Metrics.ReportPlayer( player );
        }
        
        /*
            Generic DISCONNECTED
        */
        [SubscribeEvent( EventType.PLAYER_DISCONNECTED )]
        void OnPlayerDisconnect( UnturnedPlayer player )
        {
            var displayName = player.CharacterName;

            MiscCommands.Spies          .Remove( displayName );
            CommandBack.BackDict        .Remove( displayName );
            CommandTell.Conversations   .Remove( displayName );
            CachedSkills                .Remove( displayName );

            UEssentials.ModuleManager.GetModule<KitModule>().IfPresent( m => {
                if ( CommandKit.Cooldowns.Count == 0 ) return;
                if ( !CommandKit.Cooldowns.ContainsKey( player.CSteamID.m_SteamID ) ) return;

                var playerId = player.CSteamID.m_SteamID;
                var playerCooldowns = CommandKit.Cooldowns[playerId];
                var keys = new List<string> ( playerCooldowns.Keys );
                
                /*
                    Remove from list if cooldown has expired.
                    
                    Global and per kit
                */
                if ( CommandKit.GlobalCooldown.ContainsKey( playerId ) &&
                     CommandKit.GlobalCooldown[playerId].AddSeconds(
                         EssCore.Instance.Config.Kit.GlobalCooldown) < DateTime.Now ) 
                {
                    CommandKit.GlobalCooldown.Remove( playerId );
                }
                
                foreach ( var kitName in keys )
                {
                    var kit = m.KitManager.GetByName(kitName);

                    if ( playerCooldowns[kitName].AddSeconds( kit.Cooldown ) < DateTime.Now ) 
                    {
                        playerCooldowns.Remove( kitName );                        
                    }
                }
                
                if ( playerCooldowns.Count == 0 )
                {
                    CommandKit.Cooldowns.Remove( playerId );
                }
            } );
        }

        private DateTime lastUpdateCheck = DateTime.Now;

        [SubscribeEvent( EventType.PLAYER_CONNECTED )]
        void UpdateAlert( UnturnedPlayer player )
        { 
            if ( !player.IsAdmin || lastUpdateCheck > DateTime.Now ) return;

            var updater = EssCore.Instance.Updater;

            if ( !updater.IsUpdated() )
            {
                lastUpdateCheck = DateTime.Now.AddMinutes( 10 );

                Tasks.New( t => {
                    UPlayer.From( player ).SendMessage( "[uEssentials] New version avalaible " +
                                                        $"{updater.LastResult.LatestVersion}!", Color.cyan );
                } ).Delay( 1000 ).Go();

            }
        }

        [SubscribeEvent( EventType.PLAYER_CONNECTED )]
        void JoinMessage( UnturnedPlayer player )
        {
            EssLang.PLAYER_JOINED.Broadcast( player.CharacterName );
        }

        [SubscribeEvent( EventType.PLAYER_DISCONNECTED )]
        void LeaveMessage( UnturnedPlayer player )
        {
            EssLang.PLAYER_EXITED.Broadcast( player.CharacterName );
        }

        private static Optional<IEconomyProvider> _cachedEconomyProvider;
        private static IDictionary<string, Configuration.Command> _cachedCommands;

        /*
            TODO: Cache commands & cost ??
        */
        [SubscribeEvent( EventType.ESSENTIALS_COMMAND_PRE_EXECUTED )]
        void OnCommandPreExecuted( CommandPreExecuteEvent e )
        {
            if ( e.Source.IsConsole || e.Source.HasPermission( "essentials.bypass.commandcost" ) )
            {
                return;
            }
            
            if ( _cachedEconomyProvider == null )
            {
                _cachedEconomyProvider = UEssentials.EconomyProvider;
            }

            if ( _cachedEconomyProvider.IsAbsent )
            {
                /*
                    If economy hook is not present, this "handler" will be unregistered.
                */
                EssCore.Instance.EventManager.Unregister( GetType(), "OnCommandPreExecuted" );
                EssCore.Instance.EventManager.Unregister( GetType(), "OnCommandPosExecuted" );
            }

            if ( _cachedCommands == null )
            {
                _cachedCommands = EssCore.Instance.CommandsConfig.Commands;
            }

            if ( !_cachedCommands.ContainsKey( e.Command.Name ) )
            {
                return;
            }

            var cost = _cachedCommands[e.Command.Name].Cost;

            if ( cost <= 0 )
            {
                return;
            }

            /*
                Check if player has sufficient money to use this command.
            */
            if ( _cachedEconomyProvider.Value.Has( e.Source.ToPlayer(), cost ) )
            {
                return;
            }

            EssLang.COMMAND_NO_MONEY.SendTo( e.Source, cost );
            e.Cancelled = true;
        }

        [SubscribeEvent( EventType.ESSENTIALS_COMMAND_POS_EXECUTED )]
        void OnCommandPosExecuted( CommandPosExecuteEvent e )
        {
            if ( _cachedEconomyProvider == null || 
                 e.Source.IsConsole || e.Source.HasPermission( "essentials.bypass.commandcost" ) )
            {
                return;
            }

            /*
               He will not charge if command was not successfully executed.
            */
            if ( e.Result?.Type != CommandResult.ResultType.SUCCESS ||
                 !_cachedCommands.ContainsKey( e.Command.Name ) )
            {
                return;
            }

            var cost = _cachedCommands[e.Command.Name].Cost;

            if ( cost <= 0 )
            {
                return;
            }

            _cachedEconomyProvider.Value.Withdraw( e.Source.ToPlayer(), cost );
            EssLang.COMMAND_PAID.SendTo( e.Source, cost );
        }
        
        [SubscribeEvent( EventType.PLAYER_DEATH )]
        void DeathMessages( UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID killer )
        {
            switch (cause)
            {
                case EDeathCause.BLEEDING:
                    EssLang.DEATH_BLEEDING.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.BONES:
                    EssLang.DEATH_BONES.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.FREEZING:
                    EssLang.DEATH_FREEZING.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.BURNING:
                    EssLang.DEATH_BURNING.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.FOOD:
                    EssLang.DEATH_FOOD.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.WATER:
                    EssLang.DEATH_WATER.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.GUN:
                    var pKiller = UPlayer.From( killer )?.CharacterName ?? "?";
                    EssLang.DEATH_GUN.Broadcast( player.CharacterName, TranslateLimb( limb ), pKiller );
                    break;

                case EDeathCause.MELEE:
                    pKiller = UPlayer.From( killer )?.CharacterName ?? "?";
                    EssLang.DEATH_MELEE.Broadcast( player.CharacterName, TranslateLimb( limb ), pKiller );
                    break;

                case EDeathCause.ZOMBIE:
                    EssLang.DEATH_ZOMBIE.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.ANIMAL:
                    EssLang.DEATH_ANIMAL.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.SUICIDE:
                    EssLang.DEATH_SUICIDE.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.KILL:
                    EssLang.DEATH_KILL.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.INFECTION:
                    EssLang.DEATH_INFECTION.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.PUNCH:
                    pKiller = UPlayer.From( killer )?.CharacterName ?? "?";
                    EssLang.DEATH_PUNCH.Broadcast( player.CharacterName, TranslateLimb( limb ), pKiller );
                    break;

                case EDeathCause.BREATH:
                    EssLang.DEATH_BREATH.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.ROADKILL:
                    pKiller = UPlayer.From( killer )?.CharacterName ?? "?";
                    EssLang.DEATH_ROADKILL.Broadcast( pKiller, player.CharacterName );
                    break;

                case EDeathCause.VEHICLE:
                    EssLang.DEATH_VEHICLE.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.GRENADE:
                    EssLang.DEATH_GRENADE.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.SHRED:
                    EssLang.DEATH_SHRED.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.LANDMINE:
                    EssLang.DEATH_LANDMINE.Broadcast( player.CharacterName );
                    break;

                case EDeathCause.ARENA:
                    EssLang.DEATH_ARENA.Broadcast( player.CharacterName );
                    break;
                
                default:
                    break;
            }
        }

        /* Commands eventhandlers */

        [SubscribeEvent( EventType.PLAYER_UPDATE_POSITION )]
        void HomePlayerMove( UnturnedPlayer player, Vector3 newPosition )
        {
            if ( !UEssentials.Config.HomeCommand.CancelWhenMove ||
                 !Commands.CommandHome.Delay.ContainsKey( player.CSteamID.m_SteamID ) )
            {
                return;
            }

            Commands.CommandHome.Delay[player.CSteamID.m_SteamID].Cancel();
            Commands.CommandHome.Delay.Remove( player.CSteamID.m_SteamID );
            Commands.CommandHome.Cooldown.Remove( player.CSteamID );

            UPlayer.TryGet( player, EssLang.TELEPORT_CANCELLED_MOVED.SendTo );
        }

        [SubscribeEvent( EventType.PLAYER_DEATH )]
        void BackPlayerDeath( UnturnedPlayer player, EDeathCause cause, ELimb limb,  CSteamID murderer )
        {
            if ( !player.HasPermission( "essentials.command.back" ) )
                return;

            var displayName = player.DisplayName;

            if ( Commands.CommandBack.BackDict.ContainsKey( displayName ) )
            {
                Commands.CommandBack.BackDict.Remove( displayName );
            }

            Commands.CommandBack.BackDict.Add( displayName, player.Position );
        }


        /* Freeze Command */
        private static HashSet<ulong> DisconnectedFrozen = new HashSet<ulong>();

        [SubscribeEvent( EventType.PLAYER_DEATH )]
        void FreezePlayerDeath( UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer )
        {
            if ( UEssentials.Config.UnfreezeOnDeath &&
                 player.GetComponent<FrozenPlayer>() != null )
            {
                UnityEngine.Object.Destroy( player.GetComponent<FrozenPlayer>() );
            }
        }

        [SubscribeEvent( EventType.PLAYER_DISCONNECTED )]
        void FreezePlayerDisconnect( UnturnedPlayer player )
        {
            if ( !UEssentials.Config.UnfreezeOnQuit &&
                 player.GetComponent<FrozenPlayer>() != null )
            {
                DisconnectedFrozen.Add( player.CSteamID.m_SteamID );
            }
        }
        
        [SubscribeEvent( EventType.PLAYER_CONNECTED )]
        void FreezePlayerConnected( UnturnedPlayer player ) 
        {
            if ( !UEssentials.Config.UnfreezeOnQuit && 
                 DisconnectedFrozen.Contains( player.CSteamID.m_SteamID ) ) 
            {
                UPlayer.From( player ).AddComponent<FrozenPlayer>();
                DisconnectedFrozen.Remove( player.CSteamID.m_SteamID );
            }
        } 


        [SubscribeEvent( EventType.PLAYER_DEATH )]
        void KitPlayerDeath( UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer )
        {
            var globalKitCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;

            if ( CommandKit.GlobalCooldown.ContainsKey( player.CSteamID.m_SteamID ) &&
                 EssCore.Instance.Config.Kit.ResetGlobalCooldownWhenDie ) 
            {
                CommandKit.GlobalCooldown[player.CSteamID.m_SteamID] =
                        DateTime.Now.AddSeconds( -globalKitCooldown );
            }
            
            if ( !CommandKit.Cooldowns.ContainsKey( player.CSteamID.m_SteamID ) ) return;

            var playerCooldowns = CommandKit.Cooldowns[player.CSteamID.m_SteamID];
            var keys = new List<string> ( playerCooldowns.Keys );

            foreach ( var kitName in keys )
            {
                var kit = KitModule.Instance.KitManager.GetByName(kitName);

                if ( kit == null )
                {
                    playerCooldowns.Remove( kitName );
                    continue;
                }

                if ( kit.ResetCooldownWhenDie )
                {
                    playerCooldowns[kitName] = DateTime.Now.AddSeconds( -kit.Cooldown );
                }
            }
        }

        [SubscribeEvent( EventType.PLAYER_DISCONNECTED )]
        void TpaPlayerDisconnect( UnturnedPlayer player )
        {
            var playerId = player.CSteamID.m_SteamID;
            var requests = CommandTpa.Requests;
            
            if ( requests.ContainsKey( playerId ) )
            {
                requests.Remove( playerId );
            }
            else if ( requests.ContainsValue(playerId) )
            {
                var val = requests.Keys.FirstOrDefault(k => requests[k] == playerId);
                
                if ( val != default(ulong) )
                {
                    requests.Remove( val );
                }
            }
        }

        private string TranslateLimb( ELimb limb )
        {
            switch (limb)
            {
                case ELimb.SKULL: 
                    return EssLang.LIMB_HEAD.GetMessage();

                case ELimb.LEFT_HAND:
                case ELimb.RIGHT_HAND:
                case ELimb.LEFT_ARM:
                case ELimb.RIGHT_ARM:
                    return EssLang.LIMB_ARM.GetMessage();

                case ELimb.LEFT_FOOT:
                case ELimb.RIGHT_FOOT:
                case ELimb.LEFT_LEG:
                case ELimb.RIGHT_LEG:
                    return EssLang.LIMB_LEG.GetMessage(); 

                case ELimb.SPINE:
                    return EssLang.LIMB_TORSO.GetMessage();

                default:
                    return "?";
            }
        }
    }
}