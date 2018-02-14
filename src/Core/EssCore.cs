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
using Essentials.Common.Util;
using Essentials.Configuration;
using Essentials.Core.Command;
using Essentials.Core.Event;
using Essentials.I18n;
using Essentials.Compatibility;
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
        internal const string UNTURNED_VERSION = "3.21.5.0";
        internal const string PLUGIN_VERSION = "1.3.5.0";

#if EXPERIMENTAL
        internal const string BUILD_INFO = " experimental (commit: COMMIT_HASH)";
#elif DEV
        internal const string BUILD_INFO = " (development)";
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
        private bool _wasLoadedBefore;
        private List<RocketCommandManager.RegisteredRocketCommand> _overriddenCommands = new List<RocketCommandManager.RegisteredRocketCommand>();

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

                R.Plugins.OnPluginsLoaded += OverrideCommands;

                TaskExecutor = new EssentialsTaskExecutor();

                SteamGameServer.SetKeyValue("essversion", PLUGIN_VERSION);

                Logger = new ConsoleLogger("[uEssentials] ");
                ConnectedPlayers = new Dictionary<ulong, UPlayer>();
                Debug.Listeners.Add(new EssentialsConsoleTraceListener());

                Provider.onServerDisconnected += PlayerDisconnectCallback;
                Provider.onServerConnected += PlayerConnectCallback;

                Logger.LogInfo("Enabling uEssentials...");

                new [] {
                    "Plugin version: ~white~" + PLUGIN_VERSION + BUILD_INFO,
                    "Recommended Rocket version: ~white~" + ROCKET_VERSION,
                    "Recommended Unturned version: ~white~" + UNTURNED_VERSION,
                    "Author: ~white~leonardosnt",
                    "Wiki: ~white~uessentials.github.io",
                }.ForEach(text => Logger.LogInfo(text, true));

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

                CommandOptions = new CommandOptions();
                Updater = new GithubUpdater();
                EventManager = new EventManager();
                CommandManager = new CommandManager();
                ModuleManager = new ModuleManager();
                HookManager = new HookManager();

                WebResources = new WebResources();
                Config = new EssConfig();

                var webResourcesPath = Path.Combine(Folder, WebResources.FileName);
                var configPath = Path.Combine(Folder, Config.FileName);

                WebResources.Load(webResourcesPath);

                 // Sync web config with local config.json
                if (WebResources.Loaded.ContainsKey("Config")) {
                    File.WriteAllText(configPath, WebResources.Loaded["Config"]);
                }

                Config.Load(configPath);
                CommandOptions.Load(Path.Combine(Folder, CommandOptions.FileName));
                EssLang.Load();

                EventManager.RegisterAll(GetType().Assembly);

                // Register all commands from namespace Essentials.Commands
                CommandManager.RegisterAll("Essentials.Commands");

                HookManager.RegisterAll();
                HookManager.LoadAll();

                // Load after EventManager because we relies on it inside this routine.
                ConfigPostLoad();

                LoadNativeModules();

                Logger.LogInfo("Loading modules...");
                ModuleManager.LoadAll(ModulesFolder);
                Logger.LogInfo($"Loaded {ModuleManager.RunningModules.Count(t => !(t is NativeModule))} modules");

                // We log it here because the modules being loaded above can
                // register commands.
                Logger.LogInfo($"Loaded {CommandManager.Commands.Count()} commands");

#if EXPERIMENTAL
                Logger.LogWarning("THIS IS AN EXPERIMENTAL BUILD, IT CAN BE BUGGY.");
                Logger.LogWarning("THIS IS AN EXPERIMENTAL BUILD, IT CAN BE BUGGY.");
#endif

                // Delete useless files generated by Rocket
                Task.Create()
                    .Id("Delete Xml Files")
                    .Delay(TimeSpan.FromSeconds(1))
                    .Async()
                    .Action(() => {
                        File.Delete($"{Folder}uEssentials.en.translation.xml");
                        File.Delete($"{Folder}uEssentials.configuration.xml");
                    })
                    .Submit();

                // If _wasLoadedBefore, then it means that uEssentials is
                // being reloaded, and it also means that R.Plugins.OnPluginsLoaded will not be called,
                // consequently OverrideCommands will not be called too
                // so we need to call it here.
                if (_wasLoadedBefore) {
                    OverrideCommands();
                }

                _wasLoadedBefore = true;
                CommandWindow.input.onInputText += ReloadCallback;
                Logger.LogInfo($"Enabled ({stopwatch.ElapsedMilliseconds} ms)");
            } catch (Exception e) {
                string[] messages = {
                    "An error occurred while enabling uEssentials.",
                    "If this error is not related with wrong configuration, please report",
                    "it here https://github.com/uEssentials/uEssentials/issues",
                    "Error: " + e
                };

                if (Logger == null) {
                    Console.BackgroundColor = ConsoleColor.Red;
                    messages.ForEach(Console.WriteLine);
                    Console.BackgroundColor = ConsoleColor.White;
                } else {
                    messages.ForEach(m => Logger.LogError(m));
                }
            }

#if !DEV
            Analytics.SendEvent($"ServerInit");
#endif

#if DEV
            Console.Title = "Unturned Server";
#else
            CheckUpdates();
#endif
        }

        protected override void Unload() {
            R.Plugins.OnPluginsLoaded -= OverrideCommands;
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

            // Restore overridden commands
            var rocketCommands = GetRocketCommands();
            if (rocketCommands != null) {
                rocketCommands.AddRange(_overriddenCommands);
                _overriddenCommands.Clear();
            }
        }

        // Load other things based in the Config.
        private void ConfigPostLoad() {
            if (Config.Economy.UseXp) {
                EconomyProvider = Optional<IEconomyProvider>.Of(new ExpEconomyProvider());
            } else if (HookManager.GetActiveByType<AviEconomyHook>().IsPresent) {
                EconomyProvider = Optional<IEconomyProvider>.Of(HookManager.GetActiveByType<AviEconomyHook>().Value);
            } else if (HookManager.GetActiveByType<UconomyHook>().IsPresent) {
                EconomyProvider = Optional<IEconomyProvider>.Of(HookManager.GetActiveByType<UconomyHook>().Value);
            } else {
                EconomyProvider = Optional<IEconomyProvider>.Empty();
            }

            if (Config.AutoAnnouncer.Enabled) {
                Config.AutoAnnouncer.Start();
            }

            if (Config.AutoCommands.Enabled) {
                Config.AutoCommands.Start();
            }

            if (Config.ServerFrameRate != -1) {
                var frameRate = Config.ServerFrameRate;

                if (Config.ServerFrameRate < -1) {
                    frameRate = -1; // Set to default
                }

                UnityEngine.Application.targetFrameRate = frameRate;
            }

            Config.DisabledCommands.ForEach(cmdName => {
                var command = CommandManager.GetByName(cmdName);

                if (command == null || command is CommandEssentials) {
                    Logger.LogWarning($"There is no command named '{cmdName}' to disable.");
                } else {
                    CommandManager.Unregister(command);
                    Logger.LogInfo($"Disabled command: '{command.Name}'");
                }
            });

            if (Config.EnableTextCommands) {
                TextCommands = new TextCommands();

                var textCommandsFile = Path.Combine(Folder, TextCommands.FileName);

                TextCommands.Load(textCommandsFile);

                TextCommands.Commands.ForEach(txtCommand => {
                    CommandManager.Register(new TextCommand(txtCommand));
                });
            }

            // TODO: FEATURE: Maybe we could have some kind of conditional
            // event register -- directly in EventManager?
            if (!Config.EnableJoinLeaveMessage) {
                EventManager.Unregister<EssentialsEventHandler>("JoinMessage");
                EventManager.Unregister<EssentialsEventHandler>("LeaveMessage");
            }

            if (!Config.Updater.AlertOnJoin) {
                EventManager.Unregister<EssentialsEventHandler>("UpdateAlert");
            }

            if (!Config.EnableDeathMessages) {
                EventManager.Unregister<EssentialsEventHandler>("DeathMessages");
            }
        }

        private static void LoadNativeModules() {
            // Load native modules
            Instance.Assembly.GetTypes()
                .Where(t => typeof(NativeModule).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Where(t => {
                    var moduleInfo = (ModuleInfo)t.GetCustomAttributes(typeof(ModuleInfo), false)[0];
                    return Instance.Config.EnabledSystems.Contains(moduleInfo.Name);
                })
                .ForEach(t => {
                    Instance.ModuleManager.LoadModule((NativeModule)Activator.CreateInstance(t));
                });
        }

        private static void ReloadCallback(string command) {
            if (!command.StartsWith("rocket reload", true, CultureInfo.InvariantCulture)) {
                return;
            }

            Console.WriteLine();
            UEssentials.Logger.LogWarning("/rocket reload can cause issues. If you experience any problems after running");
            UEssentials.Logger.LogWarning("this command, try restarting the server.");
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

                if (
                    !string.IsNullOrEmpty(lastResult.AdditionalData) &&
                    JObject.Parse(lastResult.AdditionalData).TryGetValue("changes", out var changes)
                ) {
                    Logger.LogInfo("====================== [ Update  Notes ] ======================");

                    changes.ToString().Split('\n').ForEach(msg => {
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

                if (Config.Updater.DownloadLatest) {
                    Updater.DownloadLatestRelease($"{Folder}/updates/");
                }
            };

            worker.RunWorkerCompleted += (sender, args) => {
                if (args.Error != null) {
                    Logger.LogError($"Could not update, try again later. Error: ({args.Error.Message})");
                    Logger.LogError("Or try to download it manually here: https://github.com/uEssentials/uEssentials/releases");
                }
            };

            worker.RunWorkerAsync();
        }

        private void OverrideCommands() {
            R.Plugins.OnPluginsLoaded -= OverrideCommands;

            var rocketCommands = GetRocketCommands();

            if (rocketCommands == null) {
                Logger.LogError("Could not override commands, rocketCommands == null.");
                return;
            }

            var essCommands = new HashSet<string>(
              CommandManager.Commands.Select(c => c.Name.ToLowerInvariant()).Concat(
              CommandManager.Commands.SelectMany(c => c.Aliases).Select(c => c.ToLowerInvariant())));

            // Used to check which commands are not "owned' by uEssentials
            var mappedRocketCommands = new Dictionary<string, RocketCommandManager.RegisteredRocketCommand>();

            rocketCommands.RemoveAll(command => {
                var name = command.Name.ToLowerInvariant();
                var wrapper = command.Command;

                // Override commands from Rocket and Unturned by default,
                // since uEssentials commands are an improved version of them.
                if (wrapper.GetType().FullName.StartsWith("Rocket.Unturned.Commands") && essCommands.Contains(name)) {
                    Logger.LogInfo($"Overriding Unturned/Rocket command ({command.Name.ToLowerInvariant()})");
                    _overriddenCommands.Add(command);
                    return true;
                }

                // It will override a command from another plugin only if it's specified in the config.json
                if (Config.CommandsToOverride.Contains(name) && !(command.Command is CommandAdapter)) {
                    var pluginName = command.Command.GetType().Assembly.GetName().Name;
                    Logger.LogInfo($"Overriding command \"{command.Name.ToLowerInvariant()}\" from plugin: {pluginName}");
                     _overriddenCommands.Add(command);
                    return true;
                }

                if (!mappedRocketCommands.ContainsKey(name)) {
                    mappedRocketCommands.Add(name, command);
                }
                return false;
            });

            // Get commands that are not registered by uEssentials.
            var commandsNotOwnedByEss = CommandManager.Commands
                .Select(command => {
                    var rocketCommand = mappedRocketCommands[command.Name.ToLowerInvariant()];
                    if (rocketCommand.Command is CommandAdapter) return null;
                    return rocketCommand;
                })
                .Where(command => command != null)
                .ToList();

            if (commandsNotOwnedByEss.Count == 0) {
                return;
            }

            lock (Console.Out) {
                Logger.LogWarning("The following commands couldn't be used by uEssentials because they already exists in another plugin:");
                commandsNotOwnedByEss.ForEach(command => {
                    var pluginName = command.Command.GetType().Assembly.GetName().Name;
                    Logger.LogWarning($" The command \"{command.Name}\" is owned by the plugin: {pluginName}");
                });
                Logger.LogWarning("If you want to use a command from uEssentials instead of from other plugin, you must add its name in \"CommandsToOverride\" in the config.json");
            }
        }

        private static List<RocketCommandManager.RegisteredRocketCommand> GetRocketCommands() {
            var commandsField = ReflectUtil.GetField(R.Commands.GetType(), "commands");
            var rocketCommands = (List<RocketCommandManager.RegisteredRocketCommand>) commandsField.GetValue(R.Commands);
            return rocketCommands;
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