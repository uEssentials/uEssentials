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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using Essentials.I18n;

namespace Essentials.InternalModules.Kit.Commands
{
    [CommandInfo(
        Name = "kit",
        Description = "Get an kit",
        Usage = "[kit_name] <player>"
    )]
    public class CommandKit : EssCommand
    {
        internal static Dictionary<ulong, Dictionary<string, DateTime>> Cooldowns = new Dictionary<ulong, Dictionary<string, DateTime>>();

        public override CommandResult OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.Length == 0 || ( parameters.Length == 1 && source.IsConsole ) )
            {
                return CommandResult.InvalidArgs( source.IsAdmin ? UsageMessage : "Use /kit [kit_name]" );
            }

            if ( parameters.Length == 1 )
            {
                var player = source.ToPlayer();

                if ( KitModule.Instance.KitManager.Contains( parameters[0].ToString() ) )
                {
                    var kitName = parameters[0].ToLowerString;
                    var requestedKit = KitModule.Instance.KitManager.GetByName(kitName);

                    if ( !requestedKit.CanUse( player ) )
                    {
                        return CommandResult.Lang( EssLang.KIT_NO_PERMISSION );
                    }

                    var steamPlayerId    = player.CSteamId.m_SteamID;
                    var kitCooldown      = requestedKit.Cooldown;

                    if ( !source.HasPermission("essentials.bypass.kitcooldown") )
                    {
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
                    KitModule.Instance.KitManager.GetByName( kitName ).GiveTo( player );
                }
                else
                {
                    return CommandResult.Lang( EssLang.KIT_NOT_EXIST, parameters[0] );
                }
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
                    EssLang.PLAYER_NOT_FOUND.SendTo( source, parameters[1] );
                }
                else if ( KitModule.Instance.KitManager.Contains( kitName ) )
                {
                    KitModule.Instance.KitManager.GetByName(kitName).GiveTo( target );
                    EssLang.KIT_GIVEN_SENDER.SendTo( source, kitName, target );
                }
                else
                {
                    return CommandResult.Lang( EssLang.KIT_NOT_EXIST, kitName );
                }
            }

            return CommandResult.Success();
        }
    }
}
