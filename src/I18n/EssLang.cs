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

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Essentials.Api;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Newtonsoft.Json;

namespace Essentials.I18n
{
    public sealed class EssLang
    {
        private static readonly string[] LANGS = { "en", "pt-br", "es", "ru" };
        private const string KEY_NOT_FOUND_MESSAGE = "<red>Lang: Key not found '{0}', report to an adminstrator.";
        private readonly string _message;

        private EssLang( string message )
        {
            _message = message;
        }

        private static Stream GetDefaultStream( string locale )
        {
            var path = $"Essentials.default.lang_{locale}.json";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream( path );
        }

        public static void LoadDefault( string locale )
        {
            LoadDefault( locale, $"{UEssentials.TranslationFolder}lang_{locale}.json" );
        }

        public static void LoadDefault( string locale, string translationPath )
        {
            if ( File.Exists( translationPath ) )
                File.WriteAllText( translationPath, "" );
            else
                File.Create( translationPath ).Close();

            var sw = new StreamWriter( translationPath );
            var defaultLangStream = GetDefaultStream( locale );

            using (var sr = new StreamReader( defaultLangStream, Encoding.UTF8, true ))
            {
                for ( string line; (line = sr.ReadLine()) != null; )
                {
                    sw.WriteLine( line );
                }
            }

            sw.Close();
        }

        public static void Load()
        {
            // Load defaults
            LANGS.ForEach( l => {
                var lpath = $"{UEssentials.TranslationFolder}lang_{l}.json";
                if ( !File.Exists( lpath ) ) LoadDefault( l );
            } );

            var locale = UEssentials.Config.Locale.ToLowerInvariant();
            var translationPath = $"{UEssentials.TranslationFolder}lang_{locale}.json";

            if ( !File.Exists( translationPath ) )
            {
                if ( LANGS.Contains( locale ) )
                {
                    LoadDefault( locale );
                }
                else
                {
                    throw new ArgumentException( $"Invalid locale '{locale}', " +
                                                 $"File not found '{translationPath}'" );
                }
            }

            JObject json;
            
            try
            {
                json = JObject.Parse( File.ReadAllText( translationPath ) );

                /* 
                    Update translation
                */
                var defaultJson = JObject.Load( new JsonTextReader( new StreamReader( 
                    GetDefaultStream( locale ), Encoding.UTF8, true ) ) );

                if ( defaultJson.Count != json.Count )
                {
                    foreach ( var key in  defaultJson )
                    {
                        JToken outVal;
                        if ( json.TryGetValue( key.Key, out outVal ) )
                        {
                            defaultJson[key.Key] = outVal;
                        }
                    }

                    File.WriteAllText( translationPath, string.Empty );
                    JsonUtil.Serialize( translationPath, defaultJson );
                    json = defaultJson;
                }
            }
            catch (JsonReaderException ex)
            {
                UEssentials.Logger.LogError( $"Invalid translation ({translationPath})" );
                UEssentials.Logger.LogError( ex.Message );

                // Load default
                json = JObject.Load( new JsonTextReader( new StreamReader( 
                    GetDefaultStream( locale ), Encoding.UTF8, true ) ) );
            }
            
            Func<string, EssLang> loadFromJson = key => {
                return new EssLang( json[key]?.ToString() 
                                    ?? string.Format( KEY_NOT_FOUND_MESSAGE, key ) );
            };

            var props = typeof(EssLang).GetProperties( BindingFlags.Public | 
                                                        BindingFlags.Static );

            foreach ( var field in props )
            {
                field.SetValue( null, loadFromJson( field.Name ), null );
            }
        }

        public void SendTo( ICommandSource source, params object[] replacers )
        {
            var message = _message.Clone() as string;
            var color = ColorUtil.GetColorFromString( ref message );

            source.SendMessage( replacers == null ? message :
                string.Format( message, replacers ), color );
        }

        public void SendTo( ICommandSource source )
        {
            SendTo( source, null );
        }

        public string GetMessage( params object[] replacers )
        {
            return replacers == null ? _message :
                string.Format( _message, replacers );
        }

        public string GetMessage()
        {
            return _message;
        }

        public void Broadcast( params object[] replacers )
        {
            var message = _message.Clone() as string;
            var color = ColorUtil.GetColorFromString( ref message );

            UServer.Broadcast( replacers == null 
                               ? message 
                               : string.Format( message, replacers ), color );
        }

        public void Broadcast()
        {
            Broadcast( null );
        }

        public override String ToString() 
        {
            return GetMessage();
        }

        public static EssLang PLAYER_JOINED { get; private set; }
        public static EssLang PLAYER_EXITED { get; private set; }
        public static EssLang PLAYER_CANNOT_EXECUTE { get; private set; }
        public static EssLang CONSOLE_CANNOT_EXECUTE { get; private set; }
        public static EssLang COMMAND_ERROR_OCURRED { get; private set; }
        public static EssLang ONLY_NUMBERS { get; private set; }
        public static EssLang ONLINE_PLAYERS { get; private set; }
        public static EssLang WARP_LIST { get; private set; }
        public static EssLang WARP_ALREADY_EXISTS { get; private set; }
        public static EssLang WARP_SET { get; private set; }
        public static EssLang WARP_REMOVED { get; private set; }
        public static EssLang WARP_NOT_EXIST { get; private set; }
        public static EssLang WARP_TELEPORTED { get; private set; }
        public static EssLang WARP_NONE { get; private set; }
        public static EssLang WARP_COOLDOWN { get; private set; }
        public static EssLang WARP_NO_PERMISSION { get; private set; }
        public static EssLang PING { get; private set; }
        public static EssLang PING_OTHER { get; private set; }
        public static EssLang INVALID_STEAMID { get; private set; }
        public static EssLang PLAYER_NOT_FOUND { get; private set; }
        public static EssLang KICK_NO_SPECIFIED_REASON { get; private set; }
        public static EssLang PLAYER_RESET_KICK { get; private set; }
        public static EssLang PLAYER_RESET { get; private set; }
        public static EssLang KILL_PLAYER { get; private set; }
        public static EssLang POSITION { get; private set; }
        public static EssLang POSITION_OTHER { get; private set; }
        public static EssLang NOT_FROZEN { get; private set; }
        public static EssLang ALREADY_FROZEN { get; private set; }
        public static EssLang FROZEN_SENDER { get; private set; }
        public static EssLang UNFROZEN_SENDER { get; private set; }
        public static EssLang FROZEN_PLAYER { get; private set; }
        public static EssLang UNFROZEN_PLAYER { get; private set; }
        public static EssLang FROZEN_ALL { get; private set; }
        public static EssLang UNFROZEN_ALL { get; private set; }
        public static EssLang NONE_FOR_REPLY { get; private set; }
        public static EssLang NOT_DIED_YET { get; private set; }
        public static EssLang RETURNED { get; private set; }
        public static EssLang SUDO_EXECUTED { get; private set; }
        public static EssLang ALL_REPAIRED { get; private set; }
        public static EssLang HAND_REPAIRED { get; private set; }
        public static EssLang MAX_SKILLS { get; private set; }
        public static EssLang MAX_SKILLS_TARGET { get; private set; }
        public static EssLang EXPERIENCE_GIVEN { get; private set; }
        public static EssLang EXPERIENCE_RECEIVED { get; private set; }
        public static EssLang EXPERIENCE_TAKE { get; private set; }
        public static EssLang EXPERIENCE_LOST { get; private set; }
        public static EssLang NEGATIVE_OR_LARGE { get; private set; }
        public static EssLang KILLED_ZOMBIES { get; private set; }
        public static EssLang TELEPORTED_ALL_YOU { get; private set; }
        public static EssLang TELEPORTED_ALL_COORDS { get; private set; }
        public static EssLang TELEPORTED_ALL_PLAYER { get; private set; }
        public static EssLang NO_PLAYERS_FOR_TELEPORT { get; private set; }
        public static EssLang NO_PLAYERS_FOR_KICK { get; private set; }
        public static EssLang KICKED_ALL { get; private set; }
        public static EssLang INVALID_COORDS { get; private set; }
        public static EssLang KIT_NOT_EXIST { get; private set; }
        public static EssLang KIT_GIVEN_SENDER { get; private set; }
        public static EssLang KIT_GIVEN_SENDER_ALL { get; private set; }
        public static EssLang KIT_GIVEN_RECEIVER { get; private set; }
        public static EssLang KIT_LIST { get; private set; }
        public static EssLang KIT_NO_PERMISSION { get; private set; }
        public static EssLang KIT_NONE { get; private set; }
        public static EssLang INVENTORY_FULL { get; private set; }
        public static EssLang DAY { get; private set; }
        public static EssLang SECOND { get; private set; }
        public static EssLang MINUTE { get; private set; }
        public static EssLang HOUR { get; private set; }
        public static EssLang DAYS { get; private set; }
        public static EssLang SECONDS { get; private set; }
        public static EssLang MINUTES { get; private set; }
        public static EssLang HOURS { get; private set; }
        public static EssLang KIT_COOLDOWN { get; private set; }
        public static EssLang COMMAND_NO_PERMISSION { get; private set; }
        public static EssLang NOT_IN_VEHICLE { get; private set; }
        public static EssLang VEHICLE_REFUELED { get; private set; }
        public static EssLang VEHICLE_REFUELED_ALL { get; private set; }
        public static EssLang VEHICLE_REPAIRED { get; private set; }
        public static EssLang VEHICLE_REPAIRED_ALL { get; private set; }
        public static EssLang POLL_NAME_IN_USE { get; private set; }
        public static EssLang POLL_STARTED { get; private set; }
        public static EssLang POLL_RUNNING { get; private set; }
        public static EssLang POLL_STOPPED { get; private set; }
        public static EssLang INVALID_NUMBER { get; private set; }
        public static EssLang INVALID_BOOLEAN { get; private set; }
        public static EssLang POLL_LIST { get; private set; }
        public static EssLang POLL_LIST_ENTRY { get; private set; }
        public static EssLang POLL_NOT_EXIST { get; private set; }
        public static EssLang POLL_VOTED_YES { get; private set; }
        public static EssLang POLL_VOTED_NO { get; private set; }
        public static EssLang POLL_ALREADY_VOTED { get; private set; }
        public static EssLang POLL_INFO { get; private set; }
        public static EssLang POLL_NONE { get; private set; }
        public static EssLang UNKNOWN_COMMAND { get; private set; }
        public static EssLang SPAWNED_ITEM { get; private set; }
        public static EssLang SPAWNED_ITEM_AT { get; private set; }
        public static EssLang SPAWNED_VEHICLE_AT_POSITION { get; private set; }
        public static EssLang SPAWNED_VEHICLE_AT_PLAYER { get; private set; }
        public static EssLang INVALID_ITEM_ID { get; private set; }
        public static EssLang INVALID_VEHICLE_ID { get; private set; }
        public static EssLang CHAT_ANTI_SPAM { get; private set; }
        public static EssLang NO_LONGER_ONLINE { get; private set; }
        public static EssLang SPY_MODE_OFF { get; private set; }
        public static EssLang SPY_MODE_ON { get; private set; }
        public static EssLang RESPAWNED_ZOMBIES { get; private set; }
        public static EssLang RESPAWNED_ANIMALS { get; private set; }
        public static EssLang INVENTORY_CLEAN { get; private set; }
        public static EssLang AUTO_RELOAD_ENABLED { get; private set; }
        public static EssLang AUTO_REFUEL_ENABLED { get; private set; }
        public static EssLang AUTO_REFUEL_DISABLED { get; private set; }
        public static EssLang AUTO_RELOAD_DISABLED { get; private set; }
        public static EssLang AUTO_REPAIR_ENABLED { get; private set; }
        public static EssLang AUTO_REPAIR_DISABLED { get; private set; }
        public static EssLang JUMP_NO_POSITION { get; private set; }
        public static EssLang JUMPED { get; private set; }
        public static EssLang TELEPORTED { get; private set; }
        public static EssLang TELEPORTED_SENDER { get; private set; }
        public static EssLang FAILED_FIND_PLACE_OR_PLAYER { get; private set; }
        public static EssLang EMPTY_HANDS { get; private set; }
        public static EssLang MUST_POSITIVE { get; private set; }
        public static EssLang CLEAR_VEHICLES { get; private set; }
        public static EssLang CLEAR_ITEMS { get; private set; }
        public static EssLang RECEIVED_ITEM { get; private set; }
        public static EssLang GIVEN_ITEM { get; private set; }
        public static EssLang GIVEN_ITEM_ALL { get; private set; }
        public static EssLang INVALID_ITEM_ID_NAME { get; private set; }
        public static EssLang CREATED_KIT { get; private set; }
        public static EssLang ITEM_NOT_FOUND { get; private set; }
        public static EssLang WITHOUT_BED { get; private set; }
        public static EssLang CANNOT_TELEPORT_DRIVING { get; private set; }
        public static EssLang TELEPORT_DELAY { get; private set; }
        public static EssLang USE_COOLDOWN { get; private set; }
        public static EssLang TELEPORTED_BED { get; private set; }
        public static EssLang TELEPORT_CANCELLED_LOSE_HEALTH { get; private set; }
        public static EssLang TELEPORT_CANCELLED_MOVED { get; private set; }
        public static EssLang NOBODY_TO_REPLY { get; private set; }
        public static EssLang KIT_ALREADY_EXIST { get; private set; }
        public static EssLang DELETED_KIT { get; private set; }
        public static EssLang RESPAWNED_ITEMS { get; private set; }
        public static EssLang RESPAWNED_VEHICLES { get; private set; }
        public static EssLang DROPKIT_SENDER { get; private set; }
        public static EssLang DROPKIT_PLAYER { get; private set; }
        public static EssLang DROPKIT_LOCATION { get; private set; }
        public static EssLang RECEIVED_VEHICLE { get; private set; }
        public static EssLang GIVEN_VEHICLE { get; private set; }
        public static EssLang GIVEN_VEHICLE_ALL { get; private set; }
        public static EssLang KILL_ALL { get; private set; }
        public static EssLang KIT_NO_MONEY { get; private set; }
        public static EssLang KIT_PAID { get; private set; }
        public static EssLang COMMAND_NO_MONEY { get; private set; }
        public static EssLang COMMAND_PAID { get; private set; }
        public static EssLang NUMBER_BETWEEN { get; private set; }
        public static EssLang SKILL_SET { get; private set; }
        public static EssLang SKILL_SET_PLAYER { get; private set; }
        public static EssLang SKILL_SET_ALL { get; private set; }
        public static EssLang INVALID_SKILL { get; private set; }
        public static EssLang ANYONE_ONLINE { get; private set; }
        public static EssLang KILLED_ANIMALS { get; private set; }
        public static EssLang DESCENDED { get; private set; }
        public static EssLang ASCENDED { get; private set; }
        public static EssLang CLEAR_EMPTY_VEHICLES { get; private set; }
        public static EssLang KIT_GLOBAL_COOLDOWN { get; private set; }  
        public static EssLang TPA_YOURSELF { get; private set; }
        public static EssLang TPA_SENT_SENDER { get; private set; }
        public static EssLang TPA_SENT { get; private set; }
        public static EssLang TPA_NONE { get; private set; }
        public static EssLang TPA_ACCEPTED { get; private set; }
        public static EssLang TPA_ACCEPTED_SENDER { get; private set; }
        public static EssLang TPA_DENIED { get; private set; }
        public static EssLang TPA_DENIED_SENDER { get; private set; }
        public static EssLang TPA_CANCELLED { get; private set; }
        public static EssLang TPA_CANNOT_TELEPORT { get; private set; }
        public static EssLang PVP_DISABLED { get; private set; }
        public static EssLang PVP_ENABLED { get; private set; }
        public static EssLang BLACKLISTED_ITEM { get; private set; }
        public static EssLang BLACKLISTED_VEHICLE { get; private set; }
        public static EssLang DEATH_BLEEDING { get; private set; }
        public static EssLang DEATH_BONES { get; private set; }
        public static EssLang DEATH_FREEZING { get; private set; }
        public static EssLang DEATH_BURNING { get; private set; }
        public static EssLang DEATH_FOOD { get; private set; }
        public static EssLang DEATH_WATER { get; private set; }
        public static EssLang DEATH_GUN { get; private set; }
        public static EssLang DEATH_MELEE { get; private set; }
        public static EssLang DEATH_ZOMBIE { get; private set; }
        public static EssLang DEATH_ANIMAL { get; private set; }
        public static EssLang DEATH_SUICIDE { get; private set; }
        public static EssLang DEATH_KILL { get; private set; }
        public static EssLang DEATH_INFECTION { get; private set; }
        public static EssLang DEATH_PUNCH { get; private set; }
        public static EssLang DEATH_BREATH { get; private set; }
        public static EssLang DEATH_ROADKILL { get; private set; }
        public static EssLang DEATH_VEHICLE { get; private set; }
        public static EssLang DEATH_GRENADE { get; private set; }
        public static EssLang DEATH_SHRED { get; private set; }
        public static EssLang DEATH_LANDMINE { get; private set; }
        public static EssLang DEATH_ARENA { get; private set; }
        public static EssLang LIMB_HEAD { get; private set; }
        public static EssLang LIMB_LEG { get; private set; }
        public static EssLang LIMB_TORSO { get; private set; }
        public static EssLang LIMB_ARM { get; private set; }
        public static EssLang ITEM_LIMIT { get; private set; }
        public static EssLang EVERYONE { get; private set; }
    }
}