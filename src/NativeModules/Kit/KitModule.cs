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
using static Essentials.Api.UEssentials;
using SDG.Unturned;

namespace Essentials.NativeModules.Kit
{

    [ModuleInfo(Name = "Kits")]
    public class KitModule : NativeModule
    {

        private const string CommandsNamespace = "Essentials.NativeModules.Kit.Commands";
        public KitManager KitManager { get; internal set; }
        public static KitModule Instance { get; internal set; }

        public override void OnLoad()
        {
            Instance = this;

            KitManager = new KitManager();

            Level.onPostLevelLoaded += Instance.OnPostLevelLoaded;
            if (Level.isLoaded)
            {
                OnPostLevelLoaded(420);
            }

            CommandManager.RegisterAll(CommandsNamespace);
            EventManager.RegisterAll<KitEventHandler>();
        }

        public override void OnUnload()
        {
            Level.onPostLevelLoaded -= OnPostLevelLoaded;

            CommandManager.UnregisterAll(CommandsNamespace);
            EventManager.UnregisterAll<KitEventHandler>();
            KitManager.Save();
        }

        public void OnPostLevelLoaded(int level)
        {
            KitManager.Load();

            Logger.LogInfo($"Loaded {KitManager.Count} kits");
        }

    }

}