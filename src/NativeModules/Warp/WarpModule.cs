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

using Essentials.Api.Module;
using Essentials.Api.Task;
using Essentials.NativeModules.Warp.Commands;
using static Essentials.Api.UEssentials;

namespace Essentials.NativeModules.Warp
{
    [ModuleInfo( Name = "Warps" )]
    public class WarpModule : NativeModule
    {
        public WarpManager          WarpManager { get; private set; }
        public static WarpModule    Instance    { get; private set; }

        public override void OnLoad()
        {
            Instance = this;

            WarpManager = new WarpManager();
            WarpManager.Load();

            Logger.LogInfo( $"Loaded {WarpManager.Count} warps" );

            CommandManager.Register<CommandWarp>();
            CommandManager.Register<CommandWarps>();
            CommandManager.Register<CommandSetWarp>();
            CommandManager.Register<CommandDelWarp>();

            Tasks.New( t => {
                WarpManager.Save();
            }).Delay( 60 * 1000 ).Interval( 60 * 1000 ).Go();
        }

        public override void OnUnload()
        {
            WarpManager.Save();

            CommandManager.Unregister<CommandWarp>();
            CommandManager.Unregister<CommandWarps>();
            CommandManager.Unregister<CommandSetWarp>();
            CommandManager.Unregister<CommandDelWarp>();
        }
    }
}
