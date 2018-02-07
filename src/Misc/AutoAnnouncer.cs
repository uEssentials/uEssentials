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

using System;
using System.Collections.Generic;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Common.Util;

namespace Essentials.Misc {

    public class AutoAnnouncer {

        public int Interval { get; set; }

        public bool RandomMessages { get; set; }

        public bool Enabled { get; set; }

        public List<string> Messages { get; set; }

        public void LoadDefaults() {
            Interval = 30;
            RandomMessages = false;
            Enabled = false;

            Messages = new List<string> {
                "<red>Automatic message 1",
                "<green>Automatic message 2",
                "<blue>Automatic message 3",
                "<yellow>Automatic message 4",
                "<magenta>Automatic message 5"
            };
        }

        /// <summary>
        /// Start broadcasting
        /// </summary>
        public void Start() {
            var messageIndex = 0;
            var rand = RandomMessages ? new Random() : null;

            Task.Create()
                .Id("AutoMessage Executor")
                .Interval(TimeSpan.FromSeconds(Interval))
                .UseIntervalAsDelay()
                .Action(() => {
                    messageIndex = RandomMessages
                        ? rand.Next(Messages.Count)
                        : (++messageIndex == Messages.Count ? 0 : messageIndex);

                    var message = (string) Messages[messageIndex].Clone();
                    var messageColor = ColorUtil.GetColorFromString(ref message);

                    UServer.Broadcast(message, messageColor);
                })
                .Submit();
        }

    }

}