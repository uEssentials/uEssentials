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

using System.Text;
using Essentials.I18n;

namespace Essentials.Common.Util {

    public static class TimeUtil {

        public static string FormatSeconds(uint seconds) {
            var msgSecond = EssLang.Translate("SECOND");
            var msgSeconds = EssLang.Translate("SECONDS");

            if (seconds < 60) {
                return $"{seconds} {(seconds == 1 ? msgSecond : msgSeconds)}";
            }

            const uint MIN = 60;
            const uint HOUR = MIN * 60;
            const uint DAY = HOUR * 24;

            var days = seconds / DAY;
            seconds -= days * DAY;

            var hours = seconds / HOUR;
            seconds -= hours * HOUR;

            var minutes = seconds / MIN;
            seconds -= minutes * MIN;

            var sb = new StringBuilder();

            if (days > 0) {
                var msgDay = EssLang.Translate("DAY");
                var msgDays = EssLang.Translate("DAYS");

                sb.Append(days)
                    .Append(" ")
                    .Append(days == 1 ? msgDay : msgDays)
                    .Append(", ");
            }

            if (hours > 0) {
                var msgHour = EssLang.Translate("HOUR");
                var msgHours = EssLang.Translate("HOURS");

                sb.Append(hours)
                    .Append(" ")
                    .Append(hours == 1 ? msgHour : msgHours)
                    .Append(", ");
            }

            if (minutes > 0) {
                var msgMinute = EssLang.Translate("MINUTE");
                var msgMinutes = EssLang.Translate("MINUTES");

                sb.Append(minutes)
                    .Append(" ")
                    .Append(minutes == 1 ? msgMinute : msgMinutes)
                    .Append(", ");
            }

            sb.Append(seconds)
                .Append(" ")
                .Append(seconds == 1 ? msgSecond : msgSeconds)
                .Append(", ");

            return sb.ToString().Substring(0, sb.Length - 2);
        }

    }

}