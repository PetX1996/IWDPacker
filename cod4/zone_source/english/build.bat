@echo off

echo 1 - %1%
echo 2 - %2%
echo 3 - %3%
echo 4 - %4%
echo 5 - %5%
echo 6 - %6%
echo 7 - %7%
echo 8 - %8%
echo 9 - %9%

set LANG=%1
shift

IF EXIST ..\zone_source\%1_load.csv (
	SET MYFASTFILES=%1 %1_load %2 %3 %4 %5 %6 %7 %8 %9
) ELSE (
	SET MYFASTFILES=%1 %2 %3 %4 %5 %6 %7 %8 %9
)

linker_pc.exe -language english -verbose %MYFASTFILES%

chdir ..\zone\
set DIR_ENG=%CD%\english
set DIR_LANG=%CD%\%LANG%

IF NOT "%LANG%" == "english" (
	echo language is not english
	echo copying fast files to %LANG% folder
	mkdir %DIR_LANG%
	IF EXIST "%DIR_ENG%\%1.ff"	move "%DIR_ENG%\%1.ff" "%DIR_LANG%\%1.ff"
	IF EXIST "%DIR_ENG%\%1_load.ff"	move "%DIR_ENG%\%1_load.ff" "%DIR_LANG%\%1_load.ff"
	IF EXIST "%DIR_ENG%\%2.ff"	move "%DIR_ENG%\%2.ff" "%DIR_LANG%\%2.ff"
	IF EXIST "%DIR_ENG%\%3.ff"	move "%DIR_ENG%\%3.ff" "%DIR_LANG%\%3.ff"
	IF EXIST "%DIR_ENG%\%4.ff"	move "%DIR_ENG%\%4.ff" "%DIR_LANG%\%4.ff"
	IF EXIST "%DIR_ENG%\%5.ff"	move "%DIR_ENG%\%5.ff" "%DIR_LANG%\%5.ff"
	IF EXIST "%DIR_ENG%\%6.ff"	move "%DIR_ENG%\%6.ff" "%DIR_LANG%\%6.ff"
	IF EXIST "%DIR_ENG%\%7.ff"	move "%DIR_ENG%\%7.ff" "%DIR_LANG%\%7.ff"
	IF EXIST "%DIR_ENG%\%8.ff"	move "%DIR_ENG%\%8.ff" "%DIR_LANG%\%8.ff"
	IF EXIST "%DIR_ENG%\%9.ff"	move "%DIR_ENG%\%9.ff" "%DIR_LANG%\%9.ff"
)

chdir ..\
set GAMEDIR=%CD%
chdir bin\IWDPacker

echo Creating IWD...

call MapPacker.bat "1" "BestCompression" "%1"

echo Copying map to usermaps...

set DIR_MAPS=%GAMEDIR%\usermaps
echo DIR_MAPS=%DIR_MAPS%

IF NOT EXIST "%DIR_MAPS%\%1" mkdir "%DIR_MAPS%\%1"
IF EXIST "%DIR_ENG%\%1.ff" xcopy /Y "%DIR_ENG%\%1.ff" "%DIR_MAPS%\%1\"
IF EXIST "%DIR_ENG%\%1_load.ff"	xcopy /Y "%DIR_ENG%\%1_load.ff" "%DIR_MAPS%\%1\"
IF EXIST "%DIR_ENG%\%1.iwd"	xcopy /Y "%DIR_ENG%\%1.iwd" "%DIR_MAPS%\%1\"

pause