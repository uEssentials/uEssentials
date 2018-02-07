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

using Essentials.Api.Task;
using System;
using System.Diagnostics;

namespace Essentials.Misc {

    internal static class Analytics {

        private static int _errorCount;

        internal static void SendEvent(string name) {
            // If something goes wrong, just stop sending events
            if (_errorCount > 10) {
                return;
            }
            Task.Create()
                .Id($"TriggerGaData '{name}'")
                .Async()
                .Action(() => {
                    try {
                        using (var wc = new System.Net.WebClient()) {
                            var data = wc.DownloadData($"https://ga-beacon.appspot.com/UA-81494650-1/{name}");
                            Debug.Print($"TriggerGaData: Success (data_len: {data.Length})");
                        }
                    }
                    catch (Exception ex) {
                        Debug.Print(ex.ToString());
                        _errorCount++;
                    }
                })
                .Submit();
        }
    }

}
