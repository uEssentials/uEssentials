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

const fs = require('fs')
const cjson = require('cjson')

const LANG_PATH = './default/'

fs.readdirSync(LANG_PATH)
  .filter(f => f.endsWith(".json"))
  .forEach(f => {
    console.log(`Validating syntax '${f}'...`)
    cjson.load(`${LANG_PATH}${f}`)
    console.log(`Ok`)
  });