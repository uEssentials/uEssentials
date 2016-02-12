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

using System;
using System.Reflection;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Common.Util;

namespace Essentials.Core.Command
{
    internal class MethodCommand : ICommand
    {
        private readonly Action<ICommandSource, ICommandArgs> _methodAction;
        private readonly Action<ICommandSource, ICommandArgs, ICommand> _methodActionWithCommand;
        private CommandInfo _info;
        private bool _hasCommandParameter;

        internal Type Owner { get; private set; }

        public string Name => _info.Name;

        public string Usage => _info.Usage;

        public string[] Aliases => _info.Aliases;

        public string Description => _info.Description;

        public string Permission { get; private set; }

        public AllowedSource AllowedSource => _info.AllowedSource;

        internal MethodCommand( Action<ICommandSource, ICommandArgs> methodAction )
        {
            _methodAction = methodAction;
            Init( false, methodAction.Method );
        }

        internal MethodCommand( Action<ICommandSource, ICommandArgs, ICommand> methodAction )
        {
            _methodActionWithCommand = methodAction;
            Init( true, methodAction.Method );
        }

        private void Init( bool hasCmdParam, MethodInfo method )
        {
            _info = Preconditions.NotNull(
                ReflectionUtil.GetAttributeFrom<CommandInfo>( method ),
                "methodAction must have 'CommandInfo' attribute." 
            );

            Owner = hasCmdParam ? _methodActionWithCommand.Method.DeclaringType
                                : _methodAction.Method.DeclaringType;

            _hasCommandParameter = hasCmdParam;

            Permission = GetType().Assembly.Equals( typeof (EssCore).Assembly )
                        ? $"essentials.command.{Name}"
                        : _info.Permission;
        }

        public void OnExecute( ICommandSource source, ICommandArgs args )
        {

            if ( _hasCommandParameter )
                _methodActionWithCommand( source, args, this );
            else
                _methodAction( source, args );
        }
    }
}