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

using System.Text;
using Essentials.I18n;

namespace Essentials.Common.Util {

    public static class TimeUtil {

        public static string FormatSeconds(uint seconds) {
            var msgDay = EssLang.DAY.GetMessage();
            var msgDays = EssLang.DAYS.GetMessage();
            var msgSecond = EssLang.SECOND.GetMessage();
            var msgSeconds = EssLang.SECONDS.GetMessage();
            var msgMinute = EssLang.MINUTE.GetMessage();
            var msgMinutes = EssLang.MINUTES.GetMessage();
            var msgHour = EssLang.HOUR.GetMessage();
            var msgHours = EssLang.HOURS.GetMessage();

            const uint MIN = 60;
            const uint HOUR = MIN*MIN;
            const uint DAY = HOUR*24;

            var days = seconds/DAY;
            seconds -= days*DAY;

            var hours = seconds/HOUR;
            seconds -= hours*HOUR;

            var minutes = seconds/MIN;
            seconds -= minutes*MIN;

            var sb = new StringBuilder();

            if (days > 0)
                sb.Append(days)
                    .Append(" ")
                    .Append(days > 1 ? msgDays : msgDay)
                    .Append(", ");

            if (hours > 0)
                sb.Append(hours)
                    .Append(" ")
                    .Append(hours > 1 ? msgHours : msgHour)
                    .Append(", ");

            if (minutes > 0)
                sb.Append(minutes)
                    .Append(" ")
                    .Append(minutes > 1 ? msgMinutes : msgMinute)
                    .Append(", ");

            sb.Append(seconds)
                .Append(" ")
                .Append(seconds > 1 ? msgSeconds : msgSecond)
                .Append(", ");

            return sb.ToString().Substring(0, sb.Length - 2);
        }

    }

}