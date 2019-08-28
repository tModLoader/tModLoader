@ECHO OFF
if [%1]==[] goto usage
if [%2]==[] goto usage

ECHO tModLoader autoupdate in progress
call :UPDATE %1 %2 >tml_update.log
ECHO Successfully updated, tModLoader will restart now.
exit /b

:UPDATE
ECHO tML exe: %1
ECHO update folder: %2

:LOOP
copy /Y "%2\%1" %1
IF ERRORLEVEL 1 (
	ECHO %1 is still running, waiting for tModLoader to close...
	Timeout /T 5 /Nobreak >nul
	GOTO LOOP
)

del %2\README.txt %2\*Installer*
xcopy /s/y %2 . 
rmdir /s/q %2

ECHO silently starting %1
start """" %1 >nul 2>&1

ECHO deleting ""%~f0""
(goto) 2>nul & del ""%~f0""
exit /B 0

:usage
echo Please do not run this file manually
exit /B 1