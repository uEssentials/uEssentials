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