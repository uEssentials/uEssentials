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
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Configuration;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Essentials.Core.Command {

    internal class TextCommand : ICommand {

        public string Name { get; internal set; }
        public string Usage { get; set; }
        public string[] Aliases { get; set; }
        public string Description { get; set; }
        public string Permission { get; set; }
        public AllowedSource AllowedSource { get; set; }

        private readonly List<CommandEntry> _commands;
        private readonly List<TextEntry> _texts;

        public TextCommand(TextCommands.TextCommandData data) {
            _texts = new List<TextEntry>();
            _commands = new List<CommandEntry>();
            Name = data.Name;
            Usage = string.Empty;
            Aliases = new string[0];
            Description = string.Empty;
            AllowedSource = AllowedSource.BOTH;
            Permission = $"essentials.textcommand.{Name}";

            data.Text.ForEach(txt => {
                if (txt.StartsWith("console_execute:")) {
                    _commands.Add(new CommandEntry {
                        IsConsoleExecutor = true,
                        Command = txt.Substring(16)
                    });
                    return;
                }
                if (txt.StartsWith("execute:")) {
                    _commands.Add(new CommandEntry {
                        IsConsoleExecutor = false,
                        Command = txt.Substring(8)
                    });
                    return;
                }
                var color = ColorUtil.GetColorFromString(ref txt);
                _texts.Add(new TextEntry { Text = txt, Color = color });
            });
        }

        public CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            foreach (var entry in _texts) {
                if (UEssentials.Config.OldFormatMessages)
                {
                    src.SendMessage(ReplaceVariables(entry.Text, src, args), entry.Color);
                }
                else
                {
                    ChatManager.serverSendMessage(ReplaceVariables(entry.Text, src, args), entry.Color, null, src.ToPlayer().SteamPlayer, EChatMode.SAY, "", true);
                }
            }
            foreach (var entry in _commands) {
                var source = entry.IsConsoleExecutor ? UEssentials.ConsoleSource : src;
                source.DispatchCommand(ReplaceVariables(entry.Command, src, args));
            }
            return CommandResult.Success();
        }

        private string ReplaceVariables(string text, ICommandSource src, ICommandArgs _) {
            return text.Replace("%sender%", src.DisplayName);
        }

        private struct TextEntry {
            public string Text;
            public Color Color;
        }

        private struct CommandEntry {
            public bool IsConsoleExecutor;
            public string Command;
        }

        public override string ToString() {
            var text = MiscUtil.ValuesToString(_texts.Select(t => t.Text).ToArray());
            return $"TextCommand {{Name: {Name}, Text: {text}}}";
        }

    }

}