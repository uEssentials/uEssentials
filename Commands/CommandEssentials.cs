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
using Essentials.Api.Command;
using Essentials.Core;
using Essentials.NativeModules.Kit;
using Essentials.NativeModules.Warp;
using Rocket.API.Commands;
using Rocket.API.Permissions;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.Permissions;
using Rocket.Core.User;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        "essentials",
        "Plugin info",
        Aliases = new[] {"ess", "?", "uessentials"},
        Syntax = ""
    )]
    public class CommandEssentials : EssCommand
    {
        public override IChildCommand[] ChildCommands =>
            new IChildCommand[]
            {
                new CommandEssentialsSaveData(UEssentials),
                new CommandEssentialsInfo()
            };

        public CommandEssentials(IPlugin plugin) : base(plugin)
        {
        }

        public override bool SupportsUser(Type user)
        {
            return true;
        }

        public override void Execute(ICommandContext context)
        {
            throw new CommandWrongUsageException("Please use a sub command.");
        }
    }

    public class CommandEssentialsInfo : IChildCommand
    {
        public bool SupportsUser(Type user)
        {
            return true;
        }

        public void Execute(ICommandContext context)
        {
            context.User.SendMessage("Author:  leonardosnt", Color.yellow);
            context.User.SendMessage("Github:  github.com/leonardosnt", Color.yellow);
            context.User.SendMessage("Ported to RM5 by: Trojaner", Color.yellow);
            context.User.SendMessage("uEssentials Github:  github.com/uEssentials", Color.yellow);
            context.User.SendMessage("Version: " + EssCore.PLUGIN_VERSION, Color.yellow);
        }

        public string Name => "Info";
        public string[] Aliases => new[] {"i"};
        public string Summary => "Info about uEssentials";
        public string Description => null;
        public string Syntax => "";
        public IChildCommand[] ChildCommands => null;
    }

    public class CommandEssentialsSaveData : IChildCommand
    {
        private readonly EssCore _essentials;

        public CommandEssentialsSaveData(EssCore essentials)
        {
            _essentials = essentials;
        }

        public bool SupportsUser(Type user)
        {
            return true;
        }

        public void Execute(ICommandContext context)
        {
            if (context.User.CheckPermission($"Essentials.savedata") != PermissionResult.Grant)
                throw new NotEnoughPermissionsException(context.User, "Essentials.savedata");

            _essentials.ModuleManager.GetModule<KitModule>().IfPresent(m => { m.KitManager.CooldownData.Save(); });

            _essentials.ModuleManager.GetModule<WarpModule>().IfPresent(m => { m.WarpManager.Save(); });
            context.User.SendMessage("Data sucessfully saved.", Rocket.API.Drawing.Color.Green);
        }

        public string Name => "SaveData";
        public string[] Aliases => new[] {"s"};
        public string Summary => "Saves data";
        public string Description => null;
        public string Syntax => "";
        public IChildCommand[] ChildCommands => null;
    }
}