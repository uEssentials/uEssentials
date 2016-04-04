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
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using Essentials.Compatibility.Hooks;
using Essentials.I18n;
using Essentials.Core;
using Essentials.Event.Handling;

namespace Essentials.NativeModules.Kit.Commands
{
    [CommandInfo(
        Name = "kit",
        Description = "Get an kit",
        Usage = "[kit_name] <player>"
    )]
    public class CommandKit : EssCommand
    {
        /*
            player_id -> [kit_name, time]
        */
        internal static Dictionary<ulong, Dictionary<string, DateTime>> Cooldowns = new Dictionary<ulong, Dictionary<string, DateTime>>();
        internal static Dictionary<ulong, DateTime> GlobalCooldown = new Dictionary<ulong, DateTime>();

        public override CommandResult OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.Length == 0 || ( parameters.Length == 1 && source.IsConsole ) )
            {
                return CommandResult.InvalidArgs( source.IsAdmin ? UsageMessage : "Use /kit [kit_name]" );
            }

            if ( parameters.Length == 1 )
            {
                var player = source.ToPlayer();

                if ( !KitModule.Instance.KitManager.Contains( parameters[0].ToString() ) )
                {
                    return CommandResult.Lang( EssLang.KIT_NOT_EXIST, parameters[0] );
                }
                
                var kitName = parameters[0].ToLowerString;
                var requestedKit = KitModule.Instance.KitManager.GetByName(kitName);

                if ( !requestedKit.CanUse( player ) )
                {
                    return CommandResult.Lang( EssLang.KIT_NO_PERMISSION );
                }

                var steamPlayerId    = player.CSteamId.m_SteamID;
                var kitCost          = requestedKit.Cost;

                if ( kitCost > 0 )
                {
                    var ecoHook = EssProvider.HookManager.GetActiveByType<UconomyHook>(); // TODO cache ??!

                    if ( ecoHook.IsPresent && (ecoHook.Value.GetBalance( steamPlayerId ) - kitCost) < 0 )
                    {
                        return CommandResult.Lang( EssLang.KIT_NO_MONEY, kitCost );
                    }
                } 

                if ( !source.HasPermission("essentials.bypass.kitcooldown") )
                {
                    var globalCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;
                    
                    if (  globalCooldown > 0 )
                    {
                        if ( GlobalCooldown.ContainsKey( steamPlayerId ) )
                        {
                            var remainingTime = DateTime.Now - GlobalCooldown[steamPlayerId];
                            
                            if ( (remainingTime.TotalSeconds + 1) > globalCooldown ) 
                            {
                                GlobalCooldown[steamPlayerId] = DateTime.Now;
                            }
                            else
                            {
                                return CommandResult.Lang( EssLang.KIT_GLOBAL_COOLDOWN, 
                                    TimeUtil.FormatSeconds( (uint) (globalCooldown - remainingTime.TotalSeconds)) );    
                            }
                        }
                        else
                        {
                            GlobalCooldown.Add( steamPlayerId, DateTime.Now );
                        }
                    }
                    else
                    {
                        var kitCooldown = requestedKit.Cooldown;
                        
                        if ( !Cooldowns.ContainsKey( steamPlayerId ) )
                        {
                            Cooldowns.Add( steamPlayerId, new Dictionary<string, DateTime>() );
                        }

                        if ( Cooldowns[steamPlayerId].ContainsKey( kitName ) )
                        {
                            var remainingTime = DateTime.Now - Cooldowns[steamPlayerId][kitName];

                            if ( (remainingTime.TotalSeconds + 1) > kitCooldown )
                            {
                                Cooldowns[steamPlayerId][kitName] = DateTime.Now;
                            }
                            else
                            {
                                return CommandResult.Lang( EssLang.KIT_COOLDOWN, TimeUtil.FormatSeconds(
                                    (uint) (kitCooldown - remainingTime.TotalSeconds) ) );
                            }
                        }
                        else
                        {
                            Cooldowns[steamPlayerId].Add( kitName, DateTime.Now );
                        }
                    }
                }

                EssProvider.HookManager.GetActiveByType<UconomyHook>().IfPresent( h => {
                    h.Withdraw( player.CSteamId.m_SteamID, requestedKit.Cost );

                    EssLang.KIT_PAID.SendTo( player, requestedKit.Cost );
                } );

                KitModule.Instance.KitManager.GetByName( kitName ).GiveTo( player );
            }
            else if ( parameters.Length == 2 )
            {
                var kitName = parameters[0].ToString();

                if ( !source.HasPermission( $"essentials.kit.{kitName}.other" ) )
                {
                    return CommandResult.Empty();
                }

                var target = parameters[1].ToPlayer;

                if ( target == null )
                {
                    return CommandResult.Lang( EssLang.PLAYER_NOT_FOUND, parameters[1] ) ;
                }
                 
                if ( KitModule.Instance.KitManager.Contains( kitName ) )
                {
                    KitModule.Instance.KitManager.GetByName( kitName ).GiveTo( target );
                    EssLang.KIT_GIVEN_SENDER.SendTo( source, kitName, target );
                }
                else
                {
                    return CommandResult.Lang( EssLang.KIT_NOT_EXIST, kitName );
                }
            }

            return CommandResult.Success();
        }
        
        protected override void OnUnregistered() 
            => EssProvider.EventManager.Unregister<EssentialsEventHandler>( "KitPlayerDeath" );
    }
}
