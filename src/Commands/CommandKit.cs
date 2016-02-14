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

using Essentials.I18n;
using System;
using System.Collections.Generic;
using System.Text;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Event;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "kit",
        Description = "Get an kit",
        Usage = "[kit_name] <player>"
    )]
    public class CommandKit : EssCommand
    {
        private static readonly Func<uint, string> FormatSeconds = seconds =>
        {
            var msgDay       = EssLang.DAY.GetMessage();
            var msgDays      = EssLang.DAYS.GetMessage();
            var msgSecond    = EssLang.SECOND.GetMessage();
            var msgSeconds   = EssLang.SECONDS.GetMessage();
            var msgMinute    = EssLang.MINUTE.GetMessage();
            var msgMinutes   = EssLang.MINUTES.GetMessage();
            var msgHour      = EssLang.HOUR.GetMessage();
            var msgHours     = EssLang.HOURS.GetMessage();

            const uint MIN  = 60;
            const uint HOUR = MIN * MIN;
            const uint DAY  = HOUR * 24;

            uint days, hours, minutes;

            days = seconds / DAY;
            seconds -= days * DAY;

            hours = seconds / HOUR;
            seconds -= hours * HOUR;

            minutes = seconds / MIN;
            seconds -= minutes * MIN;

            var sb = new StringBuilder();

            if ( days > 0 )
                sb.Append( days )
                  .Append( " " )
                  .Append( days > 1 ? msgDays : msgDay )
                  .Append(", ");

            if ( hours > 0 )
                sb.Append( hours )
                  .Append( " " )
                  .Append( hours > 1 ? msgHours : msgHour )
                  .Append(", ");

            if ( minutes > 0 )
                sb.Append( minutes )
                  .Append( " " )
                  .Append( minutes > 1 ? msgMinutes : msgMinute )
                  .Append(", ");

            sb.Append( seconds )
                .Append( " " )
                .Append( seconds > 1 ? msgSeconds : msgSecond )
                .Append(", ");

            return sb.ToString().Substring( 0, sb.Length - 2 );
        };

        public static Dictionary<ulong, Dictionary<string, DateTime>> Cooldowns = 
            new Dictionary<ulong, Dictionary<string, DateTime>>();

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.Length == 0 || ( parameters.Length == 1 && source.IsConsole ) )
            {
                source.SendMessage( source.IsAdmin ? 
                    UsageMessage : "Use /kit [kit_name]" );
            }
            else if ( parameters.Length == 1 )
            {
                var player = source.ToPlayer();

                if ( EssProvider.KitManager.Contains( parameters[0].ToString() ) )
                {
                    var kitName = parameters[0].ToLowerString;
                    var requestedKit = EssProvider.KitManager.GetByName(kitName);

                    if ( !requestedKit.CanUse( player ) )
                    {
                        EssLang.KIT_NO_PERMISSION.SendTo( player );
                    }
                    else
                    {
                        var steamPlayerId    = player.CSteamId.m_SteamID;
                        var kitCooldown      = requestedKit.Cooldown;

                        if ( !source.HasPermission("essentials.kits.bypasscooldown") )
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
                                    EssLang.KIT_COOLDOWN.SendTo( source, FormatSeconds(
                                        (uint) (kitCooldown - remainingTime.TotalSeconds)
                                    ));
                                    return;
                                }
                            }
                            else
                            {
                                Cooldowns[steamPlayerId].Add( kitName, DateTime.Now );
                            }
                        }
                        EssProvider.KitManager.GetByName( kitName ).GiveTo( player );
                    }
                }
                else
                {
                    EssLang.KIT_NOT_EXIST.SendTo( player, parameters[0] );
                }
            }
            else if ( parameters.Length == 2 )
            {
                var kitName = parameters[0].ToString();

                if ( !source.HasPermission( $"essentials.kit.{kitName}.other" ) )
                {
                    return;
                }

                var target = parameters[1].ToPlayer;

                if ( target == null )
                {
                    EssLang.PLAYER_NOT_FOUND.SendTo( source, parameters[1] );
                }
                else if ( EssProvider.KitManager.Contains( kitName ) )
                {
                    EssProvider.KitManager.GetByName(kitName).GiveTo( target );
                    EssLang.KIT_GIVEN_SENDER.SendTo( source, kitName, target );
                }
                else
                {
                    EssLang.KIT_NOT_EXIST.SendTo( source, kitName );
                }
            }
        }

        [SubscribeEvent( EventType.PLAYER_DEATH )]
        internal void OnPlayerDeath( UnturnedPlayer player,EDeathCause cause, ELimb limb,
                                     CSteamID murderer )
        {
            if ( !Cooldowns.ContainsKey( player.CSteamID.m_SteamID ) ) return;

            var playerCooldowns = Cooldowns[player.CSteamID.m_SteamID];

            foreach ( var kitName in playerCooldowns.Keys )
            {
                var kit = EssProvider.KitManager.GetByName(kitName);

                if ( kit.ResetCooldownWhenDie )
                    playerCooldowns[kitName] = DateTime.Now.AddSeconds( -kit.Cooldown );
            }
        }
    }
}
