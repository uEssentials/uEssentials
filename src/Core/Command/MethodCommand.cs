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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Common.Util;
using System;
using System.Reflection;

namespace Essentials.Core.Command {

    internal class MethodCommand : EssCommand {

        private readonly Func<ICommandSource, ICommandArgs, CommandResult> _methodFunc;
        private readonly Func<ICommandSource, ICommandArgs, ICommand, CommandResult> _methodFuncWithCommand;

        internal Type Owner { get; private set; }

        internal MethodCommand(Func<ICommandSource, ICommandArgs, CommandResult> methodFunc) :
            base(Preconditions.NotNull(ReflectUtil.GetAttributeFrom<CommandInfo>(methodFunc.Method),
                 "methodFunc must have 'CommandInfo' attribute.")) {
            _methodFunc = methodFunc;
            Init(false, methodFunc.Method);
        }

        internal MethodCommand(Func<ICommandSource, ICommandArgs, ICommand, CommandResult> methodFunc) :
            base(Preconditions.NotNull(ReflectUtil.GetAttributeFrom<CommandInfo>(methodFunc.Method),
                 "methodFunc must have 'CommandInfo' attribute.")) {
            _methodFuncWithCommand = methodFunc;
            Init(true, methodFunc.Method);
        }

        private void Init(bool hasCmdParam, MethodInfo _) {
            Owner = hasCmdParam
                ? _methodFuncWithCommand.Method.DeclaringType
                : _methodFunc.Method.DeclaringType;
        }

        public override CommandResult OnExecute(ICommandSource source, ICommandArgs args) {
            return _methodFuncWithCommand != null
                ? _methodFuncWithCommand(source, args, this)
                : _methodFunc(source, args);
        }

        public override string ToString() {
            var method = _methodFunc == null ? _methodFuncWithCommand.Method : _methodFunc.Method;
            return $"{method.DeclaringType}::{method}";
        }

    }

}