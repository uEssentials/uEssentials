@ECHO OFF

IF NOT EXIST "%cd%\uEssentials.sln" (
  SET scripts_base_path="..\"
) ELSE (
  SET scripts_base_path=".\"
)

:: Create shortcut build scripts
echo py scripts/commands.py BUILD %%* > %scripts_base_path%\build.bat
echo py scripts/commands.py RELEASE > %scripts_base_path%\release.bat