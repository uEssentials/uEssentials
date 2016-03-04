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
using Essentials.Api.Command.Source;
using Essentials.Common.Util;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Essentials.Core.Command
{
    internal class ConsoleSource : ICommandSource
    {
        private static ConsoleSource _instance;
        private static readonly object LockObj = new object();

        public string Id => CSteamID.Nil.ToString();

        public string DisplayName => "*console*";

        public bool IsConsole => true;

        public bool IsAdmin => true;

        public List<string> Permissions => new List<string> {"*"};

        internal static ConsoleSource Instance
        {
            get
            {
                lock ( LockObj )
                {
                    return _instance ?? (_instance = new ConsoleSource());
                }
            }
        }

        public bool HasPermission( string permission ) => true;

        public void SendMessage( object message )
        {
            SendMessage( message, Color.green );
        }

        public void SendMessage( object message, Color color )
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ColorUtil.UnityColorToConsoleColor( color );
            Console.WriteLine( message );
            Console.ForegroundColor = oldColor;
        }

        public void DispatchCommand( string command )
        {
            if ( command.StartsWith( "/" ) )
            {
                command = command.Remove( 0 );
            }

            CommandWindow.ConsoleInput.onInputText?.Invoke( command );
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}