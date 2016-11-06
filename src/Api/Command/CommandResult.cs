#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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

using Essentials.Common.Util;
using Essentials.I18n;

namespace Essentials.Api.Command {

    public class CommandResult {

        private static readonly CommandResult SUCCESS = new CommandResult(null, ResultType.SUCCESS);
        private static readonly CommandResult SHOW_USAGE = new CommandResult(null, ResultType.SHOW_USAGE);
        private static readonly CommandResult EMPTY = new CommandResult(null, ResultType.EMPTY);
        private static readonly CommandResult INVALID_ARGS = new CommandResult(null, ResultType.INVALID_ARGS);

        public string Message { get; }
        public ResultType Type { get; }

        /// <summary>
        /// Indicate that command was executed with success.
        /// </summary>
        /// <returns></returns>
        public static CommandResult Success() => SUCCESS;

        /// <summary>
        /// Commonly indicate that given arguments are invalid, or missing,
        /// and send the command 'Usage' to sender.
        /// </summary>
        /// <returns></returns>
        public static CommandResult ShowUsage() => SHOW_USAGE;

        /// <summary>
        /// Indicate that result of execution returned empty result.
        /// </summary>
        /// <returns></returns>
        public static CommandResult Empty() => EMPTY;

        /// <summary>
        /// Indicate that given arguments are invalid.
        /// </summary>
        /// <returns></returns>
        public static CommandResult InvalidArgs() => INVALID_ARGS;

        /// <summary>
        /// Indicate that given arguments are invalid, and send <paramref name="message"/>
        /// for the command sender.
        /// </summary>
        /// <param name="message">message for sender</param>
        /// <param name="args">Arguments of message</param>
        /// <returns></returns>
        public static CommandResult InvalidArgs(string message, params object[] args) {
            return new CommandResult(string.Format(message, args), ResultType.INVALID_ARGS);
        }

        /// <summary>
        /// Commonly indicate that sender given an invalid argument
        /// or something does not match the required.
        /// </summary>
        /// <param name="message">message for sender</param>
        /// <param name="args">Arguments of message</param>
        /// <returns></returns>
        public static CommandResult Error(string message, params object[] args) {
            if (!ColorUtil.HasColor(message)) {
                message = $"<red>{message}";
            }
            return new CommandResult(string.Format(message, args), ResultType.ERROR);
        }

        /// <summary>
        /// Commonly indicate that a given argument is invalid, and send the
        /// give <paramref name="key"/> to sender.
        /// </summary>
        /// <param name="key">Translation entry</param>
        /// <param name="args">Arguments of translation entry</param>
        /// <returns></returns>
        public static CommandResult Lang(string key, params object[] args) {
            return new CommandResult(EssLang.Translate(key, args), ResultType.LANG);
        }

        /// <summary>
        /// Indicate that given arguments are invalid, and send <paramref name="message"/>
        /// for the command sender.
        /// </summary>
        /// <param name="message">message for sender</param>
        /// <param name="args">Arguments of message</param>
        /// <returns></returns>
        public static CommandResult Generic(string message, params object[] args) {
            return new CommandResult(string.Format(message, args), ResultType.GENERIC);
        }

        /* COMMON UTIL RESULTS */
#endregion

        public static CommandResult NoPermission(string permission) {
            return UEssentials.Config.ShowPermissionOnErrorMessage
                ? Lang("COMMAND_NO_PERMISSION_WITH_PERM", permission)
                : Lang("COMMAND_NO_PERMISSION");
        }

        public CommandResult(string message, ResultType type) {
            Message = message;
            Type = type;
        }

        public override string ToString() {
            return $"{Type}{(Message == null ? "" : $" [{Message}]")}";
        }

        public enum ResultType {
            SUCCESS,
            ERROR,
            SHOW_USAGE,
            LANG,
            GENERIC,
            EMPTY,
            INVALID_ARGS
        }

    }

}