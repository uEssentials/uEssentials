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

using Essentials.Api.Module;
using Essentials.Api.Task;
using static Essentials.Api.UEssentials;

namespace Essentials.NativeModules.Warp {

    [ModuleInfo(Name = "Warps")]
    public class WarpModule : NativeModule {

        private const int kAutoSaveInterval = 60 * 1000; // 60 Seconds
        private const string kCommandsNamespace = "Essentials.NativeModules.Warp.Commands";

        public WarpManager WarpManager { get; private set; }
        public static WarpModule Instance { get; private set; }

        public override void OnLoad() {
            Instance = this;

            WarpManager = new WarpManager();
            WarpManager.Load();

            Logger.LogInfo($"Loaded {WarpManager.Count} warps");

            CommandManager.RegisterAll(kCommandsNamespace);
            EventManager.RegisterAll<WarpEventHandler>();

            Task.Create()
                .Id("Warp Auto-Save")
                .Interval(kAutoSaveInterval)
                .UseIntervalAsDelay()
                .Action(WarpManager.Save)
                .Submit();
        }

        public override void OnUnload() {
            WarpManager.Save();
            EventManager.UnregisterAll<WarpEventHandler>();
            CommandManager.UnregisterAll(kCommandsNamespace);
        }

    }

}