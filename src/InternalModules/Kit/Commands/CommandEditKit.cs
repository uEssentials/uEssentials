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

using Essentials.InternalModules.Kit.Item;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using Essentials.Common.Util;
using SDG.Unturned;

// TODO: add translations

namespace Essentials.InternalModules.Kit.Commands
{
    [CommandInfo(
        Name = "editkit",
        Aliases = new [] {"ekit"},
        Description = "Edit an kit",
        Usage = "[kit] [see | additem | delitem | set]"
    )]
    public class CommandEditKit : EssCommand
    {
        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.Length < 2 )
            {
                goto usage;
            }
            
            var kitManager = KitModule.Instance.KitManager;
            var kitName = args[0].ToString();
            
            if ( !kitManager.Contains( kitName ) )
            {
                EssLang.KIT_NOT_EXIST.SendTo( src, kitName );
                return;
            }
            
            var kit = kitManager.GetByName( kitName );
            
            switch ( args[1].ToLowerString )
            {
                case "see":
                    if ( args.Length == 3 )
                    {
                        if ( !args[2].Is( "items" ) )
                        {
                            src.SendMessage( "Use '/ekit see items' to see items" );
                            return;
                        }
                        
                        var index = 0;
                        
                        kit.Items.ForEach( i => {
                            string message;
                            
                            if ( i is KitItemExperience )
                            {
                                 message = $"Xp: {((KitItemExperience) i).Amount}";
                            }
                            else if ( i is KitItemVehicle )
                            {
                                message = $"Vehicle: {((KitItemVehicle) i).Id}" ;
                            }
                            else
                            {
                                var kitItem = i as KitItem;
                                
                                if ( kitItem == null )
                                {
                                    return;
                                }
                                
                                message = $"Id: '{kitItem.Id}' Durability: '{kitItem.Durability}' Amount: '{kitItem.Amount}'";
                            }

                            src.SendMessage( message.Insert( 0, $"[{(index++) + 1}] " ) );
                        });
                    }
                    else
                    {
                        src.SendMessage( $"Name: {kit.Name}" );
                        src.SendMessage( $"Cooldown: {kit.Cooldown}" );
                        src.SendMessage( $"ResetCooldownWhenDie: {kit.ResetCooldownWhenDie}" );
                        src.SendMessage( string.Empty );
                        src.SendMessage( "Use '/ekit see items' to see items" );
                    }
                    return;
                
                // /ekit xp additem normal id amount durability
                case "additem":
                    if ( args.Length < 3 ) 
                    {
                        src.SendMessage( "Use /ekit [kit] additem [type] [id] [amount] [durability]" );
                        return;
                    }
                    
                    byte durability = 100;
                    byte amount = 1;
                    
                    if ( args.Length >= 5 )
                    {
                        if ( !args[4].IsInt )
                        {
                            EssLang.INVALID_NUMBER.SendTo( src, args[4] );
                            return;
                        }
                        
                        var argAsInt = args[4].ToInt;
                        
                        if ( argAsInt < 0 || argAsInt > 255 )
                        {
                            EssLang.NEGATIVE_OR_LARGE.SendTo( src );
                            return;
                        }
                        
                        amount = (byte) args[4].ToInt;
                    }
                    
                    if ( args.Length >= 6 )
                    {
                        if ( !args[5].IsInt )
                        {
                            EssLang.INVALID_NUMBER.SendTo( src, args[5] );
                            return;
                        }
                        
                        var argAsInt = args[5].ToInt;
                        
                        if ( argAsInt < 0 || argAsInt > 255 )
                        {
                            EssLang.NEGATIVE_OR_LARGE.SendTo( src );
                            return;
                        }
                        
                        durability = (byte) args[5].ToInt;
                    }
                    
                    switch (args[2].ToLowerString)
                    {
                        case "normal":
                            if ( args.Length < 4 )
                            {
                                src.SendMessage( "Use /ekit [kit] additem [type] <id> [amount] [durability]" );
                                return;
                            }
                            
                            var optAsset = ItemUtil.GetItem( args[3].ToString() );
                            
                            if ( optAsset.IsAbsent )
                            {
                                EssLang.INVALID_ITEM_ID_NAME.SendTo( src, args[3] );
                                return;
                            }
                            
                            kit.Items.Add( new KitItem( optAsset.Value.id, durability, amount ) );
                            src.SendMessage( $"Added Id: {optAsset.Value.id}, Amount: {amount}, Durability: {durability}" );
                            break;
                        
                        case "vehicle":
                            if ( args.Length != 4 )
                            {
                                src.SendMessage( "Use /ekit [kit] additem vehicle [id]" );
                            }
                            else
                            {
                                if ( !args[3].IsInt )
                                {
                                    EssLang.INVALID_NUMBER.SendTo( src, args[3] );
                                    return;
                                }
                                
                                var argAsInt = args[3].ToInt;
                                
                                if ( argAsInt < 0 || argAsInt > ushort.MaxValue )
                                {
                                    EssLang.NEGATIVE_OR_LARGE.SendTo( src );
                                    return;
                                }
                                
                                var vehicleAsset = Assets.find( EAssetType.VEHICLE, (ushort) argAsInt );
                                
                                if ( vehicleAsset == null )
                                {
                                    EssLang.INVALID_VEHICLE_ID.SendTo( src, argAsInt );
                                    return;
                                }
                                
                                kit.Items.Add( new KitItemVehicle( (ushort) argAsInt ) );
                                src.SendMessage( $"Added Vehicle item. Id: {argAsInt}" );
                            }
                            break;
                        
                        case "xp":
                            if ( args.Length != 4 )
                            {
                                src.SendMessage( "Use /ekit [kit] additem xp [amount]" );
                            }
                            else
                            {
                                if ( !args[3].IsInt )
                                {
                                    EssLang.INVALID_NUMBER.SendTo( src, args[3] );
                                    return;
                                }
                                
                                var argAsInt = args[3].ToInt;
                                
                                if ( argAsInt < 0 )
                                {
                                    EssLang.MUST_POSITIVE.SendTo( src );
                                    return;
                                }
                                
                                kit.Items.Add( new KitItemExperience( (uint) argAsInt ) );
                                src.SendMessage( $"Added Xp item. Amount: {amount}" );
                            }
                            break;
                        
                        default:
                            src.SendMessage( $"Invalid type '{args[2]}'. Valid types are: normal, vehicle or xp." );
                            return;
                    }
                    break;
                
                // /ekit kit delitem [itemindex]
                case "delitem":
                    if ( args.Length != 3 )
                    {
                        src.SendMessage( "Use /ekit [kit] delitem [itemIndex]" );
                        src.SendMessage( "Use /ekit [kit] see [items] to view valid indexes." );
                    }
                    else
                    {
                        if ( !args[2].IsInt )
                        {
                            EssLang.INVALID_NUMBER.SendTo( src, args[2] );
                            return;
                        }
                        
                        var argAsInt = args[2].ToInt;
                        
                        if ( argAsInt <= 0 )
                        {
                            EssLang.MUST_POSITIVE.SendTo( src );
                            return;
                        }
                        
                        /* 1 to kitItems.Count */
                        if ( (argAsInt - 1) > kit.Items.Count ) 
                        {
                            src.SendMessage( $"Invalid index, index must be between 1 and {kit.Items.Count}" );
                            src.SendMessage( "Use /ekit [kit] see [items] to view valid indexes." );
                        }
                        else
                        {
                            kit.Items.RemoveAt( argAsInt - 1 ); 
                            src.SendMessage( $"Removed item at index {argAsInt}" );
                        }
                    }
                    break;
                    
                case "set":
                    if ( args.Length != 3 )
                    {
                        src.SendMessage( "Use /ekit [kit] set [option] [value]" );
                        src.SendMessage( "nm  = Name" );
                        src.SendMessage( "cd  = Cooldown" );
                        src.SendMessage( "rwd = ResetCooldownWhenDie" );
                        return;
                    }
                    
                    switch ( args[2].ToLowerString )
                    {
                        case "name":
                        case "nm":
                            kit.Name = args[3].ToString();
                            src.SendMessage( "Name set to " + kit.Name );
                            break;
                        
                        case "cooldown":
                        case "cd":
                            if ( !args[3].IsInt )
                            {
                                EssLang.INVALID_NUMBER.SendTo( src, args[3] );
                            }
                            else if ( args[3].ToInt < 0 )
                            {
                                EssLang.MUST_POSITIVE.SendTo( src );
                            }
                            else
                            {
                                kit.Cooldown = args[3].ToUint;
                                src.SendMessage( "Cooldown set to " + kit.Cooldown );
                            }
                            break;
                        
                        case "resetcooldownwhendie":
                        case "rwd":
                            if ( !args[3].IsBool )
                            {
                                EssLang.INVALID_BOOLEAN.SendTo( src, args[3] );
                            }
                            else
                            {
                                kit.ResetCooldownWhenDie = args[3].ToBool;
                                src.SendMessage( "ResetCooldownWhenDie set to " + kit.ResetCooldownWhenDie );
                            }
                            break;
                        
                        default:
                            src.SendMessage( "nm  = Name" );
                            src.SendMessage( "cd  = Cooldown" );
                            src.SendMessage( "rwd = ResetCooldownWhenDie" );
                            return;
                    }
                    break;
                
                default:
                    goto usage;
            }
            
            kitManager.Save();
            kitManager.Load();
            
            return;
            usage:
            ShowUsage( src);
        }
    }
}
