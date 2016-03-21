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
        private readonly Func<ICommandSource, ICommandArgs, CommandResult> _methodFunc;
        private readonly Func<ICommandSource, ICommandArgs, ICommand, CommandResult> _methodFuncWithCommand;
        private CommandInfo _info;
        private bool _hasCommandParameter;

        internal Type Owner { get; private set; }

        public string Name { get; set; }

        public string Usage { get; set; }

        public string[] Aliases { get; set; }

        public string Description { get; set; }

        public string Permission { get; set; }

        public AllowedSource AllowedSource { get; set; }

        internal MethodCommand( Func<ICommandSource, ICommandArgs, CommandResult> methodFunc )
        {
            _methodFunc = methodFunc;
            Init( false, methodFunc.Method );
        }

        internal MethodCommand( Func<ICommandSource, ICommandArgs, ICommand, CommandResult> methodFunc )
        {
            _methodFuncWithCommand = methodFunc;
            Init( true, methodFunc.Method );
        }

        private void Init( bool hasCmdParam, MethodInfo method )
        {
            _info = Preconditions.NotNull(
                ReflectionUtil.GetAttributeFrom<CommandInfo>( method ),
                "methodAction must have 'CommandInfo' attribute." 
            );

            Owner = hasCmdParam ? _methodFuncWithCommand.Method.DeclaringType
                                : _methodFunc.Method.DeclaringType;

            _hasCommandParameter = hasCmdParam;

            Name = _info.Name;
            Usage = _info.Usage;
            Description = _info.Description;
            AllowedSource = _info.AllowedSource;
            Aliases = _info.Aliases;

            Permission = GetType().Assembly.Equals( typeof (EssCore).Assembly )
                        ? $"essentials.command.{Name}"
                        : _info.Permission;
        }

        public CommandResult OnExecute( ICommandSource source, ICommandArgs args )
        {
            var result = _hasCommandParameter 
                    ? _methodFuncWithCommand( source, args, this ) 
                    : _methodFunc( source, args );

            return result;
        }
    }
}