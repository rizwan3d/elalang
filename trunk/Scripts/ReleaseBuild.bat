echo "Building release package"

del ..\Binaries\ReleaseBuild\lib\*.* /Q
del ..\Binaries\ReleaseBuild\*.* /Q
mkdir ..\Binaries\ReleaseBuild\lib
xcopy ..\StandardLibrary\Code\*.ela ..\Binaries\ReleaseBuild\lib
xcopy ..\StandardLibrary\ElaLibrary\LibChangeList.txt ..\Binaries\ReleaseBuild\lib
xcopy ..\Binaries\elalib.dll ..\Binaries\ReleaseBuild\lib

cd ..\Binaries\ReleaseBuild

..\7za.exe a -tzip Ela-%1.zip ..\..\ElaConsole\elac.exe.config ..\..\Ela\ChangeList.txt ..\ela.dll ..\elac.exe lib\*.*

echo "Release package successfully created"