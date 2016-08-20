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

using System;
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using Essentials.Core;

namespace Essentials.Api.Command {

    public abstract class EssCommand : ICommand {

        protected string UsageMessage => "Use /" + Name + " " + Usage;

        public CommandInfo Info { get; }

        public string Name {
            get { return Info.Name; }
            internal set { Info.Name = value; }
        }

        public string Permission {
            get { return Info.Permission; }
            set { Info.Permission = value; }
        }

        public string[] Aliases {
            get { return Info.Aliases; }
            set { Info.Aliases = value; }
        }

        public string Usage {
            get { return Info.Usage; }
            set { Info.Usage = value; }
        }

        public AllowedSource AllowedSource {
            get { return Info.AllowedSource; }
            set { Info.AllowedSource = value; }
        }

        public string Description {
            get { return Info.Description; }
            set { Info.Description = value; }
        }

        protected EssCommand() {
            Info = ReflectionUtil.GetAttributeFrom<CommandInfo>(this);

            if (Info == null) {
                throw new ArgumentException("EssCommand must have 'CommandInfo' attribute.");
            }

            Permission = GetType().Assembly.Equals(typeof(EssCore).Assembly)
                ? $"essentials.command.{Name}"
                : Info.Permission;

            if (string.IsNullOrEmpty(Permission)) {
                Permission = Name;
            }
        }

        internal EssCommand(CommandInfo info) {
            Info = info;

            Permission = GetType().Assembly.Equals(typeof(EssCore).Assembly)
               ? $"essentials.command.{Name}"
               : Info.Permission;

            if (string.IsNullOrEmpty(Permission)) {
                Permission = Name;
            }
        }

        protected virtual void ShowUsage(ICommandSource source) {
            source.SendMessage(UsageMessage);
        }

        protected virtual void OnUnregistered() {}

        protected virtual void OnRegistered() {}

        public abstract CommandResult OnExecute(ICommandSource src, ICommandArgs args);

    }

}