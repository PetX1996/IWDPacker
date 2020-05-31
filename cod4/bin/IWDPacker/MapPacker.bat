@echo off

set verbose=%~1
shift
set iwdQuality=%~1
shift
set FFNAME=%~1

echo.	==============================
echo.	Settings from args...
call:FUNC_SETTINGS_DEBUG
echo.	==============================

call:FUNC_DIRECTORIES

echo.	==============================
echo.	Directories...
call:FUNC_DIRECTORIES_DEBUG
echo.	==============================

if "%verbose%"=="" set verbose=1

echo.	==============================
echo.	Settings after checks...
call:FUNC_SETTINGS_DEBUG
echo.	==============================

:START                                                                       
rem set color=1e
rem color %color%

rem cls
echo.   I========================================================================I
echo.   I                    ___  _____  _____                                   I
echo.   I                   /   !!  __ \!  ___!                                  I
echo.   I                  / /! !! !  \/! !_          ___  ____                  I
echo.   I                 / /_! !! ! __ !  _!        / __!!_  /                  I
echo.   I                 \___  !! !_\ \! !      _  ! (__  / /                   I
echo.   I                     !_/ \____/\_!     (_)  \___!/___!                  I
echo.   I========================================================================I
echo.   I                                                                        I
echo.   I                            Map IWD Packer                              I
echo.   ==========================================================================
echo.   I                                 V1.1                                   I


:MAKEOPTIONS
echo.   ==========================================================================
echo.   I                         Enter name of the map                          I
echo.   ==========================================================================
if "%FFNAME%"=="" set /p FFNAME=:
if not exist "%GAMEDIR%\zone\english\%FFNAME%.ff" goto MAKEOPTIONS

goto COMPILE

:COMPILE
call:FUNC_SETTINGS_DEBUG

call:FUNC_COMPILE_IWD

goto FINAL

:: ==================================
:: FUNCTIONS
:: ==================================

:: ==================================
:: SETTINGS

:FUNC_SETTINGS_DEBUG
echo.	verbose: %verbose%
echo.	iwdQuality: %iwdQuality%
echo.	FFNAME: %FFNAME%
goto FINAL

:FUNC_DIRECTORIES
cd ..\..\
set GAMEDIR=%CD%
set OUTPUTDIR=%GAMEDIR%\zone\english
goto FINAL

:FUNC_DIRECTORIES_DEBUG
echo.	GAMEDIR: %GAMEDIR%
echo.	OUTPUTDIR: %OUTPUTDIR%
goto FINAL

:: ==================================
:: COMPILING

:FUNC_COMPILE_IWD
rem cls
if "%iwdQuality%"=="" call:FUNC_COMPILE_IWD_QUALITYCHOICE

cd %GAMEDIR%\bin\IWDPacker
echo.   ==========================================================================
echo.   I                            Creating IWD                                I
echo.   ==========================================================================
rem if exist %OUTPUTDIR%\%iwdFileName%.iwd del %OUTPUTDIR%\%iwdFileName%.iwd
set iwdSettings="-compareDate"
if "%verbose%"=="1" set iwdSettings="-compareDate -verbose"
set iwdSettings=%iwdSettings:~1,-1%

IWDPacker.exe -gameDir="%GAMEDIR%" -ffName="%FFNAME%" -outputFile="%OUTPUTDIR%\%FFNAME%.iwd" -compression=%iwdQuality% -imagesDir="%GAMEDIR%\raw\images" -soundsDir="%GAMEDIR%\raw\sound" -weaponsDir="%GAMEDIR%\raw\weapons" %iwdSettings%
echo.   ==========================================================================
echo  New IWD file successfully built!
echo.   ==========================================================================
if "%verbose%"=="1" pause
goto FINAL

:FUNC_COMPILE_IWD_QUALITYCHOICE
echo.   ==========================================================================
echo.   I                             IWD COMPRESSION                            I
echo.   ==========================================================================
echo.   I                          Prosim zvolte moznost:                        I
echo.   ==========================================================================
echo    I                             1. BestSpeed                               I
echo.   I                             2. BestCompression                         I
echo.   I                                                                        I
echo.   I                             0. Exit                                    I
echo.   ==========================================================================
if "%iwdQuality%"=="" set /p iwdQuality=:
if "%iwdQuality%"=="1" set iwdQuality=BestSpeed
if "%iwdQuality%"=="2" set iwdQuality=BestCompression
goto FINAL

:FINAL