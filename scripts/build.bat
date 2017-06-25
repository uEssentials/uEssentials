@echo off

IF NOT EXIST "%cd%\uEssentials.sln" (
  SET base_path=".."
) ELSE (
  SET base_path="."
)

:: Change this to your Rocket/Plugins folder
set ROCKET_PLUGINS_FOLDER="C:\Users\Leonardo\Documents\Unturned\Rocket\All\Unturned\Servers\MyFirstRocketServer\Rocket\Plugins"

if "%1"=="" (
  msbuild %base_path% /nologo /p:Configuration=Debug
  goto :copy
) 

if "%1"=="rebuild" (
  msbuild %base_path% /nologo /p:Configuration=Debug /t:Rebuild,Clean
  goto :copy
)

if "%1"=="release" (
  msbuild %base_path% /nologo /p:Configuration=Release /t:Rebuild,Clean
  copy /Y %base_path%\bin\Release\uEssentials.dll %base_path%\bin\
  goto :copy
)

echo Invalid option: %1

goto :EOF
:copy
if exist "%ROCKET_PLUGINS_FOLDER%" (
 echo Copying uEssentials.dll to %ROCKET_PLUGINS_FOLDER%
 copy /Y %base_path%\bin\uEssentials.dll %ROCKET_PLUGINS_FOLDER%
) else (
 echo Directory "%ROCKET_PLUGINS_FOLDER%" does not exist
)