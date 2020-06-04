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
using System.IO;
using System.Linq;
using System.Net;
using Essentials.Api;
using Essentials.Common;
using Essentials.Core;
using Essentials.Logging;
using Newtonsoft.Json.Linq;

namespace Essentials.Updater {

    internal class GithubUpdater : IUpdater {

        private const string ReleasesUrl = @"https://api.github.com/repos/TH3AL3X/uEssentials/releases/latest";
        private static readonly ConsoleLogger Logger = UEssentials.Logger;

        public UpdateResult LastResult { get; private set; }

        internal GithubUpdater() {
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        }

        public UpdateResult CheckUpdate() {
            lock (this) {
                var result = new UpdateResult(
                    EssCore.PLUGIN_VERSION,
                    ToDecimalVersion(EssCore.PLUGIN_VERSION),
                    string.Empty
                );

                try {
                    var httpRequest = (HttpWebRequest) WebRequest.Create(ReleasesUrl);
                    httpRequest.UserAgent =
                        "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
                    var respStream = httpRequest.GetResponse().GetResponseStream();

                    if (respStream == null) {
                        throw new Exception("request returned null response");
                    }

                    using (var reader = new StreamReader(respStream)) {
                        var jsonData = reader.ReadToEnd();
                        var jsonObj = JObject.Parse(jsonData);
                        var latestVersion = jsonObj.GetValue("tag_name");

                        if (latestVersion == null) {
                            throw new Exception("latest version not found.");
                        }

                        var latestVersionDecimal = ToDecimalVersion(latestVersion.ToString());
                        var currentVersionDecimal = ToDecimalVersion(EssCore.PLUGIN_VERSION);

                        if (currentVersionDecimal < latestVersionDecimal) {
                            var additionalData = new JObject();
                            var body = jsonObj.GetValue("body")?.ToString() ?? "";
                            var assets = jsonObj.GetValue("assets").Children<JObject>();

                            additionalData.Add("changes", body);
                            additionalData.Add("download_url",
                                assets.First().GetValue("browser_download_url")?.ToString() ?? "null");

                            result = new UpdateResult(
                                latestVersion.ToString(),
                                latestVersionDecimal,
                                additionalData.ToString()
                            );
                        }
                    }
                } catch (Exception ex) {
                    Logger.LogWarning("Failed to check for updates!");
                    Logger.LogWarning($"Error: {ex.Message}");
                    Logger.LogWarning("Try downloading manually here https://github.com/TH3AL3X/uEssentials/releases");
                }

                return LastResult = result;
            }
        }

        public bool IsUpdated() {
            lock (this) {
                var updateResult = LastResult ?? CheckUpdate();

                return ToDecimalVersion(EssCore.PLUGIN_VERSION) >= updateResult.LatestVersionDecimal;
            }
        }

        private static uint ToDecimalVersion(string version) {
            return uint.Parse(version.Replace(".", string.Empty));
        }

        public void DownloadLatestRelease(string path) {
            lock (this) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                var result = LastResult ?? CheckUpdate();
                var downloadUrl = JObject.Parse(result.AdditionalData).GetValue("download_url").ToString();
                var filePath = $"{path}/{downloadUrl.Substring(downloadUrl.LastIndexOf('/') + 1)}";

                if (File.Exists(filePath)) {
                    Logger.LogInfo("Latest release is already downloaded. See 'uEssentials/updates/' folder.");
                } else if (downloadUrl.EqualsIgnoreCase("null")) {
                    Logger.LogWarning("Could not download latestRelease because 'download_url' is 'null'.");
                } else {
                    Logger.LogInfo("Downloading latest release...");

                    using (var client = new WebClient()) {
                        client.DownloadFileCompleted += (sender, e) => {
                            Logger.LogInfo("Download finished. Check 'uEssentials/updates/' folder.");
                        };

                        client.DownloadProgressChanged += (sender, e) => {
                            Logger.LogInfo(
                                $"Downloading {e.BytesReceived} of {e.TotalBytesToReceive} bytes. ({e.ProgressPercentage} %)");
                        };

                        client.DownloadFileAsync(new Uri(downloadUrl), filePath);
                    }
                }
            }
        }

    }

}
