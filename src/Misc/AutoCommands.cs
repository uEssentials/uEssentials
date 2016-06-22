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

using System.Collections.Generic;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Common;

namespace Essentials.Misc
{
    public class AutoCommands
    {
        public List<AutoCommand> Commands { get; set; }

        public bool Enabled { get; set; }

        public void LoadDefaults()
        {
            Enabled = false;

            Commands = new List<AutoCommand> {
                new AutoCommand {
                    Timer = 120,
                    Commands = new [] { "say \"Be sure to drink your Ovaltine!\" 0 255 255", "item * glue" }
                },
                new AutoCommand {
                    Timer = 900,
                    Commands = new[] { "clear v", "respawnvehicles" }
                }
            };

        }

        public void Start()
        {
            Commands.ForEach( cmd => {
                var task = Tasks.New( t =>
                    cmd.Commands.ForEach( UServer.DispatchCommand )
                ).Delay( cmd.Timer * 1000 );

                if ( !cmd.RunOnce )
                    task.Interval( cmd.Timer * 1000 );

                task.Go();
            } );
        }

        public struct AutoCommand
        {
            public bool RunOnce;
            public int Timer;
            public string[] Commands;
        }
    }
}