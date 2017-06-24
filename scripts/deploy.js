/*
 *  Copyright (C) 2016-2017  leonardosnt
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

/**
 * THIS SCRIPT WILL BE USED BY APPVEYOR
 */

'use strict';

if (process.env['APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL'] != 'devleeo@outlook.com') {
    process.exit(0);
}

const fs = require('fs');
const request = require('sync-request');

const COMMIT = process.env['APPVEYOR_REPO_COMMIT'];
const ACCESS_TOKEN = process.env['GH_RELEASE_ACCESS_TOKEN'];

const REPO = "uessentials/builds";

const CREATE_RELEASE_URL = `https://api.github.com/repos/${REPO}/releases?access_token=${ACCESS_TOKEN}`;
const UPLOAD_RELEASE_URL = `https://uploads.github.com/repos/${REPO}/releases/{id}/assets?access_token=${ACCESS_TOKEN}&name={name}`;

const RELEASE_BODY = `Released at {date}\r\n\r\nRelease for commit: https://github.com/uEssentials/uEssentials/commit/${COMMIT} (${process.env['APPVEYOR_REPO_COMMIT_MESSAGE']})`;

let shortSha = COMMIT.substring(0, 7);

function createRelease(version) {
  let res = request('POST', CREATE_RELEASE_URL, {
    headers: {
      'User-Agent': 'node'
    },
    body: JSON.stringify({
      tag_name: version,
      target_commitish: "master",
      name: version,
      body: RELEASE_BODY.replace("{date}", new Date().toUTCString()),
      draft: false,
      prerelease: true
    })
  });
  if (res.body.toString().indexOf('already_exists') > -1) {
    console.log(`Release for commit ${COMMIT} already exists. Exiting...`);
    fs.writeFileSync('download_url', `https://github.com/uEssentials/Builds/releases/tag/build-${shortSha}`);
    process.exit(0);
  }
  return JSON.parse(res.getBody('utf8'));
}

function uploadAsset(releaseInfo, filePath, fileName) {
  let id = releaseInfo['id'];
  if (!id) {
    throw 'id == undefined'
  }
  let res = request('POST', UPLOAD_RELEASE_URL.replace('{id}', id).replace('{name}', fileName), {
    headers: {
      'User-Agent': 'node',
      'Content-Type': 'application/zip'
    },
    body: fs.readFileSync(filePath)
  });
  return JSON.parse(res.getBody('utf8'));
}

// Create a "blank commit" to "organize" releases.
// Get file information, (content & sha)
let fileInfoResp = request('GET', `https://api.github.com/repos/${REPO}/contents/README.md?access_token=${ACCESS_TOKEN}`, {
  headers: { 'User-Agent': 'node' }
});

let fileInfo = JSON.parse(fileInfoResp.getBody('utf-8'));

// Create commit.
request('PUT', `https://api.github.com/repos/${REPO}/contents/README.md?access_token=${ACCESS_TOKEN}`, {
  headers: { 'User-Agent': 'node' },
  body: JSON.stringify({
    message: `Added new release for https://github.com/uEssentials/uEssentials/commit/${COMMIT}!`,
    content: fileInfo.content,
    sha: fileInfo.sha
  })
});

// Create release & upload asset
uploadAsset(createRelease(`build-${shortSha}`), 'deploy/uEssentials.zip', `uEssentials-build-${shortSha}.zip`);

// Add 'Download available' status
request("POST", `https://api.github.com/repos/uessentials/uessentials/statuses/${COMMIT}?access_token=${ACCESS_TOKEN}`, {
  headers: { 'User-Agent': 'node' },
  body: JSON.stringify({
    state: "success",
    target_url: `https://github.com/uEssentials/Builds/releases/tag/build-${shortSha}`,
    description: "available!",
    context: "Download"
  })
});

fs.writeFileSync('download_url', `https://github.com/uEssentials/Builds/releases/tag/build-${shortSha}`);
