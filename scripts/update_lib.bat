@ECHO OFF

:: Copy from Unturned Directory. Don't need to download it again...
SET "UNTURNED_ASM_FOLDER=C:\Program Files (x86)\Steam\steamapps\common\Unturned\Unturned_Data\Managed"

:: Download latest rocket
echo Downloading Rocket...
bash -c "wget \"https://ci.rocketmod.net/job/Rocket.Unturned/lastSuccessfulBuild/artifact/Rocket.Unturned/bin/Release/Rocket.zip\""

:: Create Temp dir
IF NOT EXIST "TmpRocket" (
  mkdir TmpRocket
)

IF NOT EXIST "C:\Program Files (x86)\7-Zip\7z.exe" (
  echo 7z.exe not found.
  goto EOF
)

:: Unzip Rocket
"C:\Program Files (x86)\7-Zip\7z.exe" -y x Rocket.zip -oTmpRocket/


:: Copy Rocket dll's to lib\
xcopy /y TmpRocket\Modules\Rocket.Unturned\*.dll ..\lib /s /i

:: Copy Unturned dll's
echo Copying unturned assemblies
copy /Y "%UNTURNED_ASM_FOLDER%\Assembly-CSharp.dll" ..\lib
copy /Y "%UNTURNED_ASM_FOLDER%\Assembly-CSharp-firstpass.dll" ..\lib
copy /Y "%UNTURNED_ASM_FOLDER%\UnityEngine.dll" ..\lib
copy /Y "%UNTURNED_ASM_FOLDER%\Newtonsoft.Json.dll" ..\lib
echo Done!

:: Clean Temp stuffs
rmdir /s /q TmpRocket
del Rocket.zip

git diff --name-only ..\lib

:: Commit changes
set /P commit=Commit changes ? [Y/N]

IF /I "%commit%" EQU "Y" (
  git add ..\lib
  git commit -m "Update lib"
  git push -u origin master
)

rem pause