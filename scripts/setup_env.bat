@ECHO OFF

IF NOT EXIST "%cd%\uEssentials.sln" (
  SET scripts_base_path="..\"
) ELSE (
  SET scripts_base_path=".\"
)

:: Create shortcut build scripts
echo "scripts/build.bat" %%* > %scripts_base_path%\build.bat
echo "scripts/build.bat" release > %scripts_base_path%\release.bat
echo "scripts/build.bat" rebuild > %scripts_base_path%\rebuild.bat