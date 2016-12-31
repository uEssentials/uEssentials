rem Running deployment script...

mkdir deploy > nul
copy /y "../bin/Release/uEssentials.dll" "../deploy/" > nul
cd deploy
7z a -tzip uEssentials.zip uEssentials.dll > nul
cd ..

npm install sync-request
node ./scripts/deploy.js

rem Done!