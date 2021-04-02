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

        public static CommandResult Success() => SUCCESS;

        public static CommandResult ShowUsage() => SHOW_USAGE;

        public static CommandResult Empty() => EMPTY;

        public static CommandResult InvalidArgs() => INVALID_ARGS;

        public static CommandResult InvalidArgs(string message, params object[] args) {
            return new CommandResult(string.Format(message, args), ResultType.INVALID_ARGS);
        }

        public static CommandResult Error(string message, params object[] args) {
            if (!ColorUtil.HasColor(message)) {
                message = $"{message}";
            }
            return new CommandResult(string.Format(message, args), ResultType.ERROR);
        }

        public static CommandResult LangError(string key, params object[] args) {
            return new CommandResult(FailSafeTranslate(key, args), ResultType.ERROR);
        }

        public static CommandResult LangSuccess(string key, params object[] args) {
            return new CommandResult(FailSafeTranslate(key, args), ResultType.SUCCESS);
        }

        public static CommandResult Generic(string message, params object[] args) {
            return new CommandResult(string.Format(message, args), ResultType.GENERIC);
        }

        /* COMMON RESULTS */

        public static CommandResult NoPermission(string permission) {
            return UEssentials.Config.ShowPermissionOnErrorMessage
                ? LangError("COMMAND_NO_PERMISSION_WITH_PERM", permission)
                : LangError("COMMAND_NO_PERMISSION");
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
            GENERIC,
            EMPTY,
            INVALID_ARGS
        }

        private static string FailSafeTranslate(string key, params object[] args) =>
            EssLang.Translate(key, args) ?? string.Format(EssLang.KEY_NOT_FOUND_MESSAGE, key);
    }

}