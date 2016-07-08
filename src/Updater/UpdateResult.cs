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

namespace Essentials.Updater {

    public class UpdateResult {

        public string LatestVersion;
        public uint LatestVersionDecimal;
        public string AdditionalData;

        public UpdateResult(string latestVersion, uint latestVersionDecimal, string additionalData) {
            LatestVersion = latestVersion;
            LatestVersionDecimal = latestVersionDecimal;
            AdditionalData = additionalData;
        }

        public override string ToString() {
            return $"LatestVersion: {LatestVersion}, " +
                   $"LatestVersionDecimal: {LatestVersionDecimal}, " +
                   $"AdditionalData: {AdditionalData}";
        }

    }

}