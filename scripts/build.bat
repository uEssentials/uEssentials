@echo off

IF NOT EXIST "%cd%\uEssentials.sln" (
  SET base_path="../"
) ELSE (
  SET base_path="./"
)

if "%1"=="" (
  msbuild %base_path%\uEssentials.sln /nologo /p:Configuration=Debug
  goto :EOF
)

if "%1"=="rebuild" (
  msbuild %base_path%\uEssentials.sln /nologo /p:Configuration=Debug /t:Rebuild,Clean
  goto :EOF
)

if "%1"=="release" (
  msbuild %base_path% /nologo /p:Configuration=Release /t:Rebuild,Clean
  copy /Y %base_path%\bin\Release\uEssentials.dll %base_path%\bin\
  goto :EOF
)

echo Invalid option: %1

EOF: