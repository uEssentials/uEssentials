#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Event;
using Essentials.Api.Module;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Commands;
using Essentials.Common;
using Essentials.Configuration;
using Essentials.Core.Command;
using Essentials.Core.Event;
using Essentials.I18n;
using Essentials.Common.Reflect;
using Essentials.Compatibility;
using Essentials.Core.Permission;
using Essentials.Event.Handling;
using Essentials.NativeModules;
using Essentials.Updater;
using Essentials.Compatibility.Hooks;
using Essentials.Economy;
using Essentials.Logging;
using Essentials.Misc;
using Newtonsoft.Json.Linq;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Core.Commands;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace Essentials.Core {

    public sealed class EssCore : RocketPlugin {

        internal const string ROCKET_VERSION = "4.9.3.0";
        internal const string UNTURNED_VERSION = "3.17.1.0";
        internal const string PLUGIN_VERSION = "1.3.0.0";

#if EXPERIMENTAL
        internal const string BUILD_INFO = " experimental (commit: $COMMIT_HASH$)";
#else
        internal const string BUILD_INFO = "";
#endif

        internal static EssCore Instance;

        internal static byte DebugFlags;
        internal const byte kDebugTasks = 0x01;
        internal const byte kDebugCommands = 0x02;

        internal Optional<IEconomyProvider> EconomyProvider;
        internal ModuleManager ModuleManager { get; set; }
        internal ICommandManager CommandManager { get; set; }
        internal IEventManager EventManager { get; set; }
        internal HookManager HookManager { get; set; }
        internal IUpdater Updater { get; set; }

        internal CommandOptions CommandOptions { get; set; }
        internal TextCommands TextCommands { get; set; }
        internal EssConfig Config { get; set; }
        internal ConsoleLogger Logger { get; set; }
        internal ITaskExecutor TaskExecutor { get; set; }
        internal WebResources WebResources { get; set; }

        private string _folder;
        private string _translationFolder;
        private string _dataFolder;
        private string _modulesFolder;

        internal string Folder => MkDirIfNotExists(_folder);
        internal string TranslationFolder => MkDirIfNotExists(_translationFolder);
        internal string DataFolder => MkDirIfNotExists(_dataFolder);
        internal string ModulesFolder => MkDirIfNotExists(_modulesFolder);

        internal Dictionary<ulong, UPlayer> ConnectedPlayers { get; set; }
        internal InstancePool CommonInstancePool { get; } = new InstancePool();

        protected override void Load() {
            try {
                var stopwatch = Stopwatch.StartNew();

                Instance = this;

                try {
                    var essPermProvider = new EssentialsPermissionsProvider();
                    R.Permissions = essPermProvider;
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                }

                TaskExecutor = new EssentialsTaskExecutor();

                Logger = new ConsoleLogger("[uEssentials] ");
                ConnectedPlayers = new Dictionary<ulong, UPlayer>();
                Debug.Listeners.Add(new EssentialsConsoleTraceListener());

                Provider.onServerDisconnected += PlayerDisconnectCallback;
                Provider.onServerConnected += PlayerConnectCallback;

                Logger.LogInfo("Enabling uEssentials...");

                if (Provider.clients.Count > 0) {
                    Provider.clients.ForEach(p => {
                        ConnectedPlayers.Add(p.playerID.steamID.m_SteamID,
                            new UPlayer(UnturnedPlayer.FromSteamPlayer(p)));
                    });
                }

                _folder = Rocket.Core.Environment.PluginsDirectory + "/uEssentials/";
                _translationFolder = Folder + "translations/";
                _dataFolder = Folder + "data/";
                _modulesFolder = Folder + "modules/";

                WebResources = new WebResources();
                Config = new EssConfig();

                var webRscPath = Path.Combine(Folder, WebResources.FileName);
                var configPath = Path.Combine(Folder, Config.FileName);

                WebResources.Load(webRscPath);

                // TODO: Remove
                // Load old webkit/webconfig
                try {
                    if (File.Exists(configPath) && !WebResources.Enabled) {
                        var json = JObject.Parse(File.ReadAllText(configPath));
                        var save = false;

                        foreach (var opt in new[] { "Config", "Kits" }) {
                            JToken val;
                            if (json.TryGetValue($"Web{opt}", out val)) {
                                if (val.Value<bool>("Enabled")) {
                                    WebResources.Enabled = true;
                                    WebResources.URLs[opt] = val.Value<string>("Url");
                                    save = true;
                                }
                            }
                        }

                        if (save) {
                            WebResources.Save(webRscPath);
                            WebResources.Load(webRscPath);
                        }
                    }
                } catch (Exception ex) {
                    Debug.Print(ex.ToString());
                }

                 // Sync web config with local config.json
                if (WebResources.Loaded.ContainsKey("Config")) {
                    File.WriteAllText(configPath, WebResources.Loaded["Config"]);
                }

                Config.Load(configPath);

                CommandOptions = new CommandOptions();
                CommandOptions.Load(Path.Combine(Folder, CommandOptions.FileName));

                Updater = new GithubUpdater();
                EventManager = new EventManager();
                CommandManager = new CommandManager();
                ModuleManager = new ModuleManager();
                HookManager = new HookManager();

                EssLang.Load();

                new [] {
                    "Plugin version: ~white~" + PLUGIN_VERSION + BUILD_INFO,
                    "Recommended Rocket version: ~white~" + ROCKET_VERSION,
                    "Recommended Unturned version: ~white~" + UNTURNED_VERSION,
                    "Author: ~white~leonardosnt",
                    "Wiki: ~white~uessentials.github.io",
                }.ForEach(text => Logger.LogInfo(text, true));

                EventManager.RegisterAll(GetType().Assembly);

                if (!Config.EnableJoinLeaveMessage) {
                    EventManager.Unregister<EssentialsEventHandler>("JoinMessage");
                    EventManager.Unregister<EssentialsEventHandler>("LeaveMessage");
                }

                CommandManager.RegisterAll("Essentials.Commands");

                HookManager.RegisterAll();
                HookManager.LoadAll();

                if (Config.Economy.UseXp) {
                    EconomyProvider = Optional<IEconomyProvider>.Of(new ExpEconomyProvider());
                } else if (HookManager.GetActiveByType<UconomyHook>().IsPresent) {
                    EconomyProvider = Optional<IEconomyProvider>.Of(HookManager.GetActiveByType<UconomyHook>().Value);
                } else {
                    EconomyProvider = Optional<IEconomyProvider>.Empty();
                }

                /*
                    Load native modules
                */
                Assembly.GetTypes()
                    .Where(t => typeof(NativeModule).IsAssignableFrom(t))
                    .WhereNot(t => t.IsAbstract)
                    .Where(t => {
                        var moduleInfo = (ModuleInfo) t.GetCustomAttributes(typeof(ModuleInfo), false)[0];
                        return Config.EnabledSystems.Any(s => s.Equals(moduleInfo.Name, StringComparison.OrdinalIgnoreCase));
                    })
                    .ForEach(t => {
                        ModuleManager.LoadModule((NativeModule) Activator.CreateInstance(t));
                    });

                Logger.LogInfo($"Loaded {CommandManager.Commands.Count()} commands");

                Logger.LogInfo("Loading modules...");
                ModuleManager.LoadAll(ModulesFolder);
                Logger.LogInfo($"Loaded {ModuleManager.RunningModules.Count(t => !(t is NativeModule))} modules");

                if (Config.AutoAnnouncer.Enabled) {
                    Config.AutoAnnouncer.Start();
                }

                if (Config.AutoCommands.Enabled) {
                    Config.AutoCommands.Start();
                }

                if (!Config.Updater.AlertOnJoin) {
                    EventManager.Unregister<EssentialsEventHandler>("UpdateAlert");
                }

                if (Config.ServerFrameRate != -1) {
                    var frameRate = Config.ServerFrameRate;

                    if (Config.ServerFrameRate < -1) {
                        frameRate = -1; // Set to default
                    }

                    UnityEngine.Application.targetFrameRate = frameRate;
                }

                if (Config.DisabledCommands.Count != 0) {
                    Config.DisabledCommands.ForEach(cmdName => {
                        var command = CommandManager.GetByName(cmdName);

                        if (command == null || command is CommandEssentials) {
                            Logger.LogWarning($"There is no command named '{cmdName}' to disable.");
                        } else {
                            CommandManager.Unregister(command);
                            Logger.LogInfo($"Disabled command: '{command.Name}'");
                        }
                    });
                }

                if (Config.EnableTextCommands) {
                    TextCommands = new TextCommands();

                    var textCommandsFile = Path.Combine(Folder, TextCommands.FileName);

                    TextCommands.Load(textCommandsFile);

                    TextCommands.Commands.ForEach(txtCommand => {
                        CommandManager.Register(new TextCommand(txtCommand));
                    });
                }

                if (!Config.EnableDeathMessages) {
                    EventManager.Unregister<EssentialsEventHandler>("DeathMessages");
                }

#if EXPERIMENTAL
                Logger.LogWarning("THIS IS AN EXPERIMENTAL BUILD, CAN BE BUGGY.");
                Logger.LogWarning("THIS IS AN EXPERIMENTAL BUILD, CAN BE BUGGY.");
#endif

                Task.Create()
                    .Id("Delete Xml Files")
                    .Delay(TimeSpan.FromSeconds(1))
                    .Async()
                    .Action(() => {
                        File.Delete($"{Folder}uEssentials.en.translation.xml");
                        File.Delete($"{Folder}uEssentials.configuration.xml");
                    })
                    .Submit();

                Task.Create()
                    .Id("Unregister Rocket Commands")
                    .Delay(TimeSpan.FromSeconds(3))
                    .Action(() => UnregisterRocketCommands(true)) // Second check, silently.
                    .Submit();

                CommandWindow.input.onInputText += ReloadCallback;
                UnregisterRocketCommands(); // First check.
                Logger.LogInfo($"Enabled ({stopwatch.ElapsedMilliseconds} ms)");
            } catch (Exception e) {
                var msg = new List<string>() {
                    "An error occurred while enabling uEssentials.",
                    "If this error is not related with wrong configuration please report",
                    "immediatly here https://github.com/uEssentials/uEssentials/issues",
                    "Error: " + e
                };

                if (!Provider.APP_VERSION.EqualsIgnoreCase(UNTURNED_VERSION)) {
                    msg.Add("I detected that you are using a different version of the recommended, " +
                            "please update your uEssentials/Unturned.");
                    msg.Add("If you are using the latest uEssentials release, please wait for update.");
                }

                if (Logger == null) {
                    Console.BackgroundColor = ConsoleColor.Red;
                    msg.ForEach(Console.WriteLine);
                    Console.BackgroundColor = ConsoleColor.White;
                } else {
                    msg.ForEach(Logger.LogError);
                }
            }

#if !DEV
            TriggerGaData($"Server/{Parser.getIPFromUInt32(Provider.ip)}");
#endif

#if DEV
            Console.Title = "Unturned Server";
#else
            CheckUpdates();
#endif
        }

        protected override void Unload() {
            CommandWindow.input.onInputText -= ReloadCallback;
            Provider.onServerDisconnected -= PlayerDisconnectCallback;
            Provider.onServerConnected -= PlayerConnectCallback;

            var executingAssembly = GetType().Assembly;

            HookManager.UnloadAll();
            HookManager.UnregisterAll();
            CommandManager.UnregisterAll(executingAssembly);
            EventManager.UnregisterAll(executingAssembly);
            ModuleManager.UnloadAll();

            TaskExecutor.Stop();
        }

        internal static int _errCount;

        internal static void TriggerGaData(string path) {
            if (_errCount > 10) {
                return;
            }
            Task.Create()
                .Id($"TriggerGaData '{path}'")
                .Async()
                .Action(() => {
                    try {
                        using (var wc = new System.Net.WebClient()) {
                            var data = wc.DownloadData($"https://ga-beacon.appspot.com/UA-81494650-1/{path}");
                            Debug.Print($"TriggerGaData: Success (data_len: {data.Length})");
                        }
                    } catch (Exception ex) {
                        Debug.Print(ex.ToString());
                        EssCore._errCount++;
                    }
                })
                .Submit();
        }

        private static void ReloadCallback(string command) {
            if (!command.StartsWith("rocket reload", true, CultureInfo.InvariantCulture)) {
                return;
            }

            Console.WriteLine();
            UEssentials.Logger.LogError("Rocket reload cause many issues, consider restarting the server");
            UEssentials.Logger.LogError("Or use '/essentials reload' to reload essentials correctly.");
            Console.WriteLine();
        }

        private void PlayerConnectCallback(CSteamID id) {
            ConnectedPlayers.Add(id.m_SteamID, new UPlayer(UnturnedPlayer.FromCSteamID(id)));
        }

        private void PlayerDisconnectCallback(CSteamID id) {
            ConnectedPlayers.Remove(id.m_SteamID);
        }

        private void CheckUpdates() {
            if (!Config.Updater.CheckUpdates) {
                return;
            }

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, args) => {
                Logger.LogInfo("Checking updates.");

                var isUpdated = Updater.IsUpdated();
                var lastResult = Updater.LastResult;

                if (isUpdated) {
                    Logger.LogInfo("Plugin is up-to-date!");
                    return;
                }

                Logger.LogInfo($"New version avalaible: {lastResult.LatestVersion}");

                if (!string.IsNullOrEmpty(lastResult.AdditionalData)) {
                    JToken changesStr;
                    if (JObject.Parse(lastResult.AdditionalData)
                        .TryGetValue("changes", out changesStr)) {
                        Logger.LogInfo("====================== [ Update  Notes ] ======================");

                        changesStr.ToString().Split('\n').ForEach(msg => {
                            Logger.Log("", ConsoleColor.Green, suffix: "");
                            Logger.Log("  " + msg, ConsoleColor.White, "");
                        });

                        Logger.LogInfo("");
                        Logger.Log("", ConsoleColor.Green, suffix: "");
                        Logger.Log(
                            "  " +
                            $"See more in: https://github.com/uEssentials/uEssentials/releases/tag/{lastResult.LatestVersion}",
                            ConsoleColor.White, "");

                        Logger.LogInfo("===============================================================");
                    }
                }

                if (Config.Updater.DownloadLatest) {
                    Updater.DownloadLatestRelease($"{Folder}/updates/");
                }
            };

            worker.RunWorkerCompleted += (sender, args) => {
                if (args.Error != null) {
                    Logger.LogError($"Could not update, try again later. ({args.Error.Message})");
                    Logger.LogError("Try download manually here: https://github.com/uEssentials/uEssentials/releases");
                }
            };

            worker.RunWorkerAsync();
        }

        private void UnregisterRocketCommands(bool silent = false) {
            var _rocketCommands =
                AccessorFactory.AccessField<List<RocketCommandManager.RegisteredRocketCommand>>(R.Commands, "commands").Value;

            if (_rocketCommands == null) {
                Logger.LogError("Could not unregister Rocket commands.");
                return;
            }

            /* All names & aliases of uEssentials command */
            var essCommands = _rocketCommands
                .Where(c => c.Command is CommandAdapter)
                .Select(c => c.Name.ToLowerInvariant())
                .ToList();

            _rocketCommands.RemoveAll(c => {
                if (essCommands.Contains(c.Name.ToLowerInvariant()) && !(c.Command is CommandAdapter)) {
                    if (!silent)
                        Logger.LogInfo($"Overriding Rocket command ({c.Name.ToLowerInvariant()})");
                    return true;
                }
                return false;
            });
        }

        private static string MkDirIfNotExists(string dir) {
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            return dir;
        }

    }

    internal class EssentialsConsoleTraceListener : ConsoleTraceListener {

        public override void WriteLine(string message, string category) {
#if !EVENT_MANAGER_DEBUG
            if (category == "EventManager") return;
#endif

#if !COMMAND_MANAGER_DEBUG
            if (category == "CommandManager") return;
#endif
            WriteLine($"[{category}] {message}");
        }

        public override void WriteLine(object obj, string category) {
            WriteLine(ObjectToString(obj), category);
        }

        public override void WriteLine(string message) {
            UEssentials.Logger.LogDebug(message);
        }

        public override void WriteLine(object obj) {
            WriteLine(ObjectToString(obj));
        }

        private static string ObjectToString(object obj) => obj == null ? "null" : obj.ToString();

    }

}