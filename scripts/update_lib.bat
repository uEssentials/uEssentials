@ECHO OFF

SET "UNTURNED_DIR=C:\Program Files (x86)\Steam\steamapps\common\Unturned"
SET "ROCKET_DIR=%UNTURNED_DIR%\Extras\Rocket.Unturned"
SET "LIB_DIR=..\lib"

echo Copying unturned assemblies
xcopy /Y "%UNTURNED_DIR%\Unturned_Data\Managed\Assembly-CSharp.dll" %LIB_DIR%
xcopy /Y "%UNTURNED_DIR%\Unturned_Data\Managed\Assembly-CSharp-firstpass.dll" %LIB_DIR%
xcopy /Y "%UNTURNED_DIR%\Unturned_Data\Managed\UnityEngine.dll" %LIB_DIR%
xcopy /Y "%UNTURNED_DIR%\Unturned_Data\Managed\Newtonsoft.Json.dll" %LIB_DIR%
echo Done!

echo Copying Rocket.Unturned
xcopy /Y "%ROCKET_DIR%\Rocket.Unturned.dll" %LIB_DIR%
xcopy /Y "%ROCKET_DIR%\Rocket.Core.dll" %LIB_DIR%
xcopy /Y "%ROCKET_DIR%\Rocket.API.dll" %LIB_DIR%
echo Done!

git diff --name-only %LIB_DIR%

:: Commit changes
set /P commit="Commit changes ? [Y/N] "

IF /I "%commit%" EQU "Y" (
  git add %LIB_DIR%
  git commit -m "Update lib"
  git push -u origin master
)