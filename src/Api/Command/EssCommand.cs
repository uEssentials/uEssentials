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

using Essentials.Api.Command.Source;
using Essentials.Api.Logging;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core;

namespace Essentials.Api.Command
{
    public abstract class EssCommand : ICommand
    {
        private readonly CommandInfo _commandInfo;

        protected string        UsageMessage    { get; set; }
        protected EssLogger     Logger          { get; }
        public string           Permission      { get; }
        public string           Name            => _commandInfo.Name;
        public string[]         Aliases         => _commandInfo.Aliases;
        public string           Usage           => _commandInfo.Usage;
        public AllowedSource    AllowedSource   => _commandInfo.AllowedSource;
        public string           Description     => _commandInfo.Description;

        protected EssCommand()
        {
            _commandInfo = Preconditions.NotNull(
                ReflectionUtil.GetAttributeFrom<CommandInfo>( this ),
                "EssCommand must have 'CommandInfo' attribute" 
            );

            UsageMessage = "Use /" + Name + " " + Usage;
            Logger = EssProvider.Logger;

            Permission = GetType().Assembly.Equals( typeof (EssCore).Assembly )
                ? $"essentials.command.{Name}"
                : _commandInfo.Permission;

            if ( Permission.IsNullOrEmpty() )
            {
                Permission = Name;
            }
        }

        protected virtual void ShowUsage( ICommandSource source )
        {
            source.SendMessage( UsageMessage );
        }

        public abstract void OnExecute( ICommandSource src, ICommandArgs args );
    }
}