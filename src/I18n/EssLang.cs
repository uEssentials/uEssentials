#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
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
#endregion

using Essentials.Api;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Essentials.I18n {
    
    public static class EssLang {

        internal const string KEY_NOT_FOUND_MESSAGE = "Lang: Key not found '{0}', report to an adminstrator.";
        private static readonly string[] LANGS = { "en", "pt-br", "es", "ru" };
        private static readonly Dictionary<string, object> _translations = new Dictionary<string, object>();

        private static void LoadDefault(string locale) {
            LoadDefault(locale, Path.Combine(UEssentials.TranslationFolder, $"lang_{locale}.json"));
        }

        public static void LoadDefault(string locale, string destPath) {
            if (File.Exists(destPath))
                File.WriteAllText(destPath, string.Empty);
            else
                File.Create(destPath).Close();

            var sw = new StreamWriter(destPath);
            var defaultLangStream = GetDefaultStream(locale);

            using (var sr = new StreamReader(defaultLangStream, Encoding.UTF8, true)) {
                for (string line; (line = sr.ReadLine()) != null;) {
                    sw.WriteLine(line);
                }
            }

            sw.Close();
        }

        public static void Load() {
            // Load defaults
            LANGS.ForEach(l => {
                var lpath = $"{UEssentials.TranslationFolder}lang_{l}.json";
                if (!File.Exists(lpath)) LoadDefault(l);
            });

            var locale = UEssentials.Config.Locale.ToLowerInvariant();
            var translationPath = $"{UEssentials.TranslationFolder}lang_{locale}.json";

            if (!File.Exists(translationPath)) {
                if (LANGS.Contains(locale)) {
                    LoadDefault(locale);
                } else {
                    UEssentials.Logger.LogError($"Invalid locale '{locale}', " +
                                                $"File not found '{translationPath}'");
                    UEssentials.Logger.LogError("Switching to default locale (en)...");
                    locale = "en";
                    translationPath = $"{UEssentials.TranslationFolder}lang_{locale}.json";
                }
            }

            JObject json;

            try {
                json = JObject.Parse(File.ReadAllText(translationPath));

                /*
                    Update translation
                */
                var defaultJson = JObject.Load(new JsonTextReader(new StreamReader(
                    GetDefaultStream(locale), Encoding.UTF8, true)));

                if (defaultJson.Count != json.Count) {
                    foreach (var key in  defaultJson) {
                        if (json.TryGetValue(key.Key, out var outVal)) {
                            defaultJson[key.Key] = outVal;
                        }
                    }

                    File.WriteAllText(translationPath, string.Empty);
                    JsonUtil.Serialize(translationPath, defaultJson);
                    json = defaultJson;
                }
            } catch (JsonReaderException ex) {
                UEssentials.Logger.LogError($"Invalid translation ({translationPath})");
                UEssentials.Logger.LogException(ex);

                // Load default
                json = JObject.Load(new JsonTextReader(new StreamReader(
                    GetDefaultStream(locale), Encoding.UTF8, true)));
            }

            _translations.Clear();
            foreach (var entry in json) {
                _translations.Add(entry.Key, entry.Value.Value<string>());
            }
        }

        public static string Translate(string key) {
            return GetEntry(key) as string;
        }

        public static string Translate(string key, params object[] args) {
            if (!(GetEntry(key) is string raw))
            {
                return null;
            }
            try {
                return string.Format(raw, args);
            } catch (FormatException) {
                UEssentials.Logger.LogError($"An error ocurred while translating the entry " +
                                            $"'{key}'. Arguments: {MiscUtil.ValuesToString(args)}");
                throw;
            }
        }

        public static bool HasEntry(string key) {
            return _translations.ContainsKey(key);
        }

        public static object GetEntry(string key) {
            return _translations.TryGetValue(key, out var val) ? val : null;
        }
        #region call me idiot
        public static void SendNoBuffer(ICommandSource target, string key, params object[] args)
        {
            var message = Translate(key, args);
            Color color;

            if (message == null)
            {
                color = Color.red;
                message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
            }
            else if (message.Length > 0)
            {
                color = ColorUtil.GetColorFromString(ref message);
            }
            else
            {
                return;  // Will not send if message is empty.
            }
            
            target.SendMessage(message, color);
        }
        public static void SendGlobal(string key, params object[] args)
        {
            var message = Translate(key, args);
            Color color;

            if (UEssentials.Config.OldFormatMessages)
            {
                if (message == null)
                {
                    color = Color.red;
                    message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
                }
                else if (message.Length > 0)
                {
                    color = ColorUtil.GetColorFromString(ref message);
                }
                else
                {
                    return;  // Will not send if message is empty.
                }
                UnturnedChat.Say(message, color);
            }
            else
            {
                if (message == null)
                {
                    color = Color.red;
                    message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
                }
                else if (message.Length > 0)
                {
                    color = Color.yellow;
                }
                else
                {
                    return;
                }
                ChatManager.serverSendMessage(message.ToString(), color, null, null, EChatMode.GLOBAL, "", true);
            }
        }
        public static void Send(ICommandSource target, string key, params object[] args) {
            var message = Translate(key, args);
            Color color;

            if (UEssentials.Config.OldFormatMessages)
            {
                if (message == null)
                {
                    color = Color.red;
                    message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
                }
                else if (message.Length > 0)
                {
                    color = ColorUtil.GetColorFromString(ref message);
                }
                else
                {
                    return;  // Will not send if message is empty.
                }
                target.SendMessage(message, color);
            }
            else
            {
                if (message == null)
                {
                    color = Color.red;
                    message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
                }
                else if (message.Length > 0)
                {
                    color = Color.yellow;
                }
                else
                {
                    return;
                }
                ChatManager.serverSendMessage(message.ToString(), color, null, target.ToPlayer().SteamPlayer);
            }

        }
        
        public static void BetterBroadcast(string keyicon, string key, params object[] args)
        {
            var message = Translate(key, args);
            var icon = Translate(keyicon);
            Color color;
            if (message == null)
            {
                color = Color.red;
                message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
            }
            else
            {
                color = ColorUtil.GetColorFromString(ref message);
            }
            if (UEssentials.Config.OldFormatMessages)
            {
                UnturnedChat.Say(message?.ToString() ?? "null", color);
            }
            else
            {
                ChatManager.serverSendMessage(message.ToString(), color, null, null, EChatMode.GLOBAL, icon, true);
            }
        }
        /*public static void BroadcastOld(object message, Color color)
        {
            UnturnedChat.Say(message?.ToString() ?? "null", color);
        }*/

        /*public static void BroadcastSecond(object message, object icon, Color color)
        {
            if (UEssentials.Config.OldFormatMessages)
            {
                UnturnedChat.Say(message?.ToString() ?? "null", color);
            }
            else
            {
                ChatManager.serverSendMessage(message?.ToString() ?? "null", color, null, null, EChatMode.GLOBAL, icon.ToString() ?? "", true);
            }
        }*/
        public static void Broadcast(string key, params object[] args) {
            var message = Translate(key, args);
            Color color;
            if (UEssentials.Config.OldFormatMessages)
            {
                if (message == null)
                {
                    color = Color.red;
                    message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
                }
                else
                {
                    color = ColorUtil.GetColorFromString(ref message);
                }
                BetterBroadcast(message, null, color);
            }
            else
            {
                if (message == null)
                {
                    color = Color.red;
                    message = string.Format(KEY_NOT_FOUND_MESSAGE, key);
                }
                else
                {
                    color = Color.yellow;
                }

                BetterBroadcast(message, null, color);
            }
        }
        #endregion
        private static Stream GetDefaultStream(string locale) {
            var path = $"Essentials.default.lang_{locale}.json";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        }

    }

}