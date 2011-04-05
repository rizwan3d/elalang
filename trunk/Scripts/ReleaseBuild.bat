echo "Building release package"

del ..\bin\lib\*.* /Q
mkdir ..\bin\lib
xcopy ..\StandardLibrary\Code\*.ela ..\bin\lib
xcopy ..\StandardLibrary\ElaLibrary\LibChangeList.txt ..\bin\lib
xcopy ..\bin\elalib.dll ..\bin\lib

cd ..\bin

..\Binaries\7za.exe a -tzip Ela-%1.zip ..\ElaConsole\elac.exe.config ..\Ela\ChangeList.txt ..\ElaConsole\ConsoleChangeList.txt ela.dll ..\Docs\License.txt elac.exe lib\*.*

echo "Release package successfully created"
pause