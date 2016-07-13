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
using System.Collections.Generic;
using System.Reflection;
using Essentials.Api.Command.Source;

namespace Essentials.Api.Command {

    /// <summary>
    /// Author: leonardosc
    /// </summary>
    public interface ICommandManager {

        IEnumerable<ICommand> Commands { get; }

        void Register(ICommand command);

        void Register<TCommandType>() where TCommandType : ICommand;

        void Register(Func<ICommandSource, ICommandArgs, CommandResult> method);

        void Register(Func<ICommandSource, ICommandArgs, ICommand, CommandResult> method);

        void RegisterAll(Assembly assembly);

        void RegisterAll(string targetNamespace);

        void UnregisterAll(Assembly assembly);

        void UnregisterAll(string targetNamespace);

        void Unregister<TCommandType>() where TCommandType : ICommand;

        void Unregister(ICommand command);

        void Unregister(Type commandType);

        bool HasWithName(string commandName);

        bool HasWithType<TCommandType>() where TCommandType : ICommand;

        ICommand GetByName(string name, bool includeAliases = true);

        ICommand GetByType(Type commandType);

        TCommandType GetByType<TCommandType>() where TCommandType : ICommand;

    }

}