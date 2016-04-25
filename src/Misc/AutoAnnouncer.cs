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
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Common.Util;
using Newtonsoft.Json;
using Rocket.Core.Assets;

namespace Essentials.Misc
{
    [JsonObject]
    public class AutoAnnouncer : IDefaultable
    {
        /// <summary>
        /// Interval between the messages
        /// </summary>
        public int MessageInterval { get; set; }

        /// <summary>
        /// Flag that determine if messages will be random
        /// </summary>
        public bool RandomMessages { get; set; }

        /// <summary>
        /// Flag that determine if announcer  is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// List of messages
        /// </summary>
        public List<string> Messages { get; set; }

        /// <summary>
        /// Load default values
        /// </summary>
        public void LoadDefaults()
        {
            MessageInterval = 30;
            RandomMessages = false;
            Enabled = false;

            Messages = new List<string>
            {
                "<cyan>You can see kits by typing /kits, and to get a kit by typing /kit <name>",
                "<red>Messaage 1",
                "<green>Message 2",
                "<blue>Message 3",
                "<yellow>Message 4",
                "<purple>Message 5"
            };
        }

        /// <summary>
        /// Start broadcasting
        /// </summary>
        public void Start()
        {
            var messageIndex = 0;
            var rand = new Random();

            Tasks.New( t =>
            {
                var messagesCount = Messages.Count;

                messageIndex = RandomMessages 
                            ? rand.Next( messagesCount ) 
                            : (++messageIndex == messagesCount ? 0 : messageIndex);

                var message = (string) Messages[messageIndex].Clone();
                var messageColor = ColorUtil.GetColorFromString( ref message );

                UServer.Broadcast( message, messageColor );
            } ).Delay( MessageInterval*1000 ).Interval( MessageInterval*1000 ).Go();
        }
    }
}
