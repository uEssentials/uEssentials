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

namespace Essentials.Api.Command
{
    /// <summary>
    /// Author: leonardosc
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<ICommand> Commands { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        void Register( ICommand command );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCommandType"></typeparam>
        void Register<TCommandType>() where TCommandType : ICommand;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        void Register( Func<ICommandSource, ICommandArgs, CommandResult> method );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        void Register( Func<ICommandSource, ICommandArgs, ICommand, CommandResult> method );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        void RegisterAll( Assembly assembly );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetNamespace"></param>
        void RegisterAll( string targetNamespace );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        void UnregisterAll( Assembly assembly );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetNamespace"></param>
        void UnregisterAll( string targetNamespace );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCommandType"></typeparam>
        void Unregister<TCommandType>() where TCommandType : ICommand;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        void Unregister( ICommand command );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        void Unregister( Type commandType );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        bool HasWithName( string commandName );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCommandType"></typeparam>
        /// <returns></returns>
        bool HasWithType<TCommandType>() where TCommandType : ICommand;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeAliases"></param>
        /// <returns></returns>
        ICommand GetByName( string name, bool includeAliases = true );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        ICommand GetByType( Type commandType );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCommandType"></typeparam>
        /// <returns></returns>
        TCommandType GetByType<TCommandType>() where TCommandType : ICommand;
    }
}
