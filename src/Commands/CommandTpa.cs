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

using System.Collections.Generic;
using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Core;
using Essentials.Event.Handling;
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.Commands 
{
    [CommandInfo(
        Name = "tpa",
        Usage = "[player|accept|deny|cancel]",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandTpa : EssCommand
    {
        /*
            Key     -> sender
            Value   -> receiver
        */
        public static Dictionary<ulong, ulong> Requests = new Dictionary<ulong, ulong>();
        
        public override CommandResult OnExecute( ICommandSource src, ICommandArgs args )
        {
            if (args.Length < 1 ) 
            {
                return CommandResult.ShowUsage();
            }
            
            var player = src.ToPlayer();
            var senderId = player.CSteamId.m_SteamID;
                        
            switch ( args[0].ToLowerString ) 
            {
                case "accept":
                case "a":
                {
                    if ( !Requests.ContainsValue( senderId ) )
                    {
                        return CommandResult.Lang( EssLang.TPA_NONE );
                    }

                    var whoSentId = new List<ulong>( Requests.Keys ).FirstOrDefault( k => Requests[k] == senderId );
                    var whoSent = UPlayer.From( new Steamworks.CSteamID( whoSentId ) );
                    
                    /* Should never happen */
                    if ( whoSent == null )
                    {
                        return CommandResult.Lang( EssLang.TPA_NONE );
                    }
                    
                    if ( whoSent.Stance == EPlayerStance.DRIVING ||
                         whoSent.Stance == EPlayerStance.SITTING  ) 
                    {
                        EssLang.CANNOT_TELEPORT_DRIVING.SendTo( whoSent );
                        return CommandResult.Lang( EssLang.TPA_CANNOT_TELEPORT, whoSent.DisplayName );
                    }
                    
                    EssLang.TPA_ACCEPTED_SENDER.SendTo( src, whoSent.DisplayName );
                    EssLang.TPA_ACCEPTED.SendTo( whoSent, src.DisplayName );
                    Requests.Remove( whoSentId );
                    
                    var tpaSettings = EssCore.Instance.Config.Tpa;

                    if ( tpaSettings.TeleportDelay > 0 )
                    {
                        Tasks.New( t => {
                            if ( !whoSent.IsOnline || !player.IsOnline )
                            {
                                return;
                            }
                            whoSent.Teleport( player.Position );
                        }).Delay( tpaSettings.TeleportDelay * 1000 ).Go();
                    }
                    else
                    {
                        whoSent.Teleport( player.Position );
                    }
                    break;
                }
                
                case "deny":
                case "d":
                {
                    if ( !Requests.ContainsValue( senderId ) )
                    {
                        return CommandResult.Lang( EssLang.TPA_NONE ); 
                    }

                    var whoSentId = new List<ulong>( Requests.Keys ).FirstOrDefault( k => Requests[k] == senderId );
                    var whoSent = UPlayer.From( new Steamworks.CSteamID( whoSentId ) );

                    if ( whoSent != null )
                    {
                        EssLang.TPA_DENIED.SendTo( whoSent, src.DisplayName );
                    }
                    
                    EssLang.TPA_DENIED_SENDER.SendTo( src, whoSent == null ? "Unknown" : whoSent.DisplayName );
                    Requests.Remove( whoSentId );
                    break;
                }
                
                case "cancel":
                case "c":
                {
                    if ( !Requests.ContainsKey( senderId ) )
                    {
                        return CommandResult.Lang( EssLang.TPA_NONE );
                    }

                    Requests.Remove( senderId );
                    EssLang.TPA_CANCELLED.SendTo( src );
                    break;
                }
                
                default:
                {
                    if ( !args[0].IsValidPlayerName ) 
                    {
                        return CommandResult.Lang( EssLang.PLAYER_NOT_FOUND, args[0] );
                    }
                    
                    /*
                        Cancel current request
                    */
                    if ( Requests.ContainsKey( senderId ) )
                    {
                        Requests.Remove( senderId );
                    }
                    
                    var target = args[0].ToPlayer;
                    
                    if ( target == player )
                    {
                        return CommandResult.Lang( EssLang.TPA_YOURSELF );
                    }
                    
                    Requests.Add( senderId, target.CSteamId.m_SteamID );
                    EssLang.TPA_SENT_SENDER.SendTo( src, target.DisplayName );
                    EssLang.TPA_SENT.SendTo( target, src.DisplayName );

                    var tpaSettings = EssCore.Instance.Config.Tpa;

                    if ( tpaSettings.ExpireDelay > 0 )
                    {
                        Tasks.New( t => {
                            Requests.Remove( senderId );
                        }).Delay( tpaSettings.ExpireDelay * 1000 ).Go();
                    }
                    else
                    {
                        Requests.Remove( senderId );
                    }
                    break;
                }
            }
            
            return CommandResult.Success();
        }
        
        protected override void OnUnregistered()
            => EssProvider.EventManager.Unregister<EssentialsEventHandler>( "TpaPlayerDisconnect" );
    }
}
