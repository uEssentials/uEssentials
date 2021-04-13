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

using Essentials.Api;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Common.Util;
using Essentials.I18n;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;

namespace Essentials.Misc {
    public sealed class Message
    {

        public string Text;

        public string Icon;

        public Message(string text, string icon)
        {
            Text = text;
            Icon = icon;
        }
        public Message()
        {
            Text = "";
            Icon = "";
        }
    }
    public class AutoAnnouncer {

        public int Interval { get; set; }
        public int lastindex = 0;
        public bool Enabled { get; set; }

        // Don't need this
        //public List<string> Messages { get; set; }

        public List<string> Icons { get; set; }
        public Message[] Messages;

        public void LoadDefaults() {
            Interval = 10;

            Enabled = true;

            Messages = new Message[]{
                new Message("<color=blue>[uEssentials]</color> This is an announcement", "https://avatars.githubusercontent.com/u/16111599?s=200&v=4.png"),
                new Message("<color=blue>[uEssentials]</color> This is something", "https://avatars.githubusercontent.com/u/16111599?s=200&v=4.png")
            };
        }

        /// <summary>
        /// Start broadcasting
        /// </summary>
        public void Start() {
            Task.Create()
                .Id("AutoMessage Executor")
                .Interval(TimeSpan.FromSeconds(Interval))
                .UseIntervalAsDelay()
                .Action(() => {

                    if (lastindex > (Messages.Length - 1)) lastindex = 0;

                    Message message = Messages[lastindex];

                    //var icon = message.Icon;
                    var messageColor = ColorUtil.GetColorFromString(ref message.Text);

                    if (UEssentials.Config.OldFormatMessages)
                    {
                        UnturnedChat.Say(message.Text.ToString(), messageColor);
                    }
                    else
                    {
                        ChatManager.serverSendMessage(message.Text.ToString(), messageColor, null, null, EChatMode.GLOBAL, message.Icon.ToString(), true);
                    }

                    lastindex++;
                })
                .Submit();
        }

    }

}