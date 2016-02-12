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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Common.Util;

namespace Essentials.Core.Command
{
    //TODO: !!! CLEANUP !!!
    internal class MethodCommand : ICommand
    {
        internal Type Owner => _methodAction != null
            ? _methodAction.Method.DeclaringType
            : _methodActionWithCommand.Method.DeclaringType;

        private readonly Action<ICommandSource, ICommandArgs> _methodAction;
        private readonly Action<ICommandSource, ICommandArgs, ICommand> _methodActionWithCommand;
        private readonly CommandInfo _info;
        private readonly bool _hasCommandParameter;
        private readonly string _permission;

        internal MethodCommand( Action<ICommandSource, ICommandArgs> methodAction )
        {
            _methodAction = methodAction;
            _hasCommandParameter = false;

            _info = Preconditions.NotNull(
                ReflectionUtil.GetAttributeFrom<CommandInfo>( methodAction.Method ), 
                "methodAction must have 'CommandInfo' attribute."
            );

            _permission = GetType().Assembly.Equals( typeof( EssCore ).Assembly ) 
                ? $"essentials.command.{Name}" 
                : _info.Permission;
        }

       internal MethodCommand( Action<ICommandSource, ICommandArgs, ICommand> methodAction )
        {
            _methodActionWithCommand = methodAction;
            _hasCommandParameter = true;

            _info = Preconditions.NotNull(
                ReflectionUtil.GetAttributeFrom<CommandInfo>( methodAction.Method ), 
                "methodAction must have 'CommandInfo' attribute." );

            _permission = GetType().Assembly.Equals( typeof( EssCore ).Assembly ) 
                ? $"essentials.command.{Name}" 
                : _info.Permission;
        }

        public string Name                      => _info.Name;
        public string Usage                     => _info.Usage;
        public string[] Aliases                 => _info.Aliases;
        public string Description               => _info.Description;
        public string Permission                => _permission;
        public AllowedSource AllowedSource      => _info.AllowedSource;

        public void OnExecute(ICommandSource source, ICommandArgs args)
        {
            if ( _hasCommandParameter )
                _methodActionWithCommand( source, args, this );
            else
                _methodAction( source, args );
        }
    }
}