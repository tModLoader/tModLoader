;@echo off
;Title Converting batch scripts to file.exe with iexpress
;Mode 75,3 & color 0A
;Rem Original Script https://github.com/npocmaka/batch.scripts/edit/master/hybrids/iexpress/bat2exeIEXP.bat
;echo(
;if "%~1" equ "" (
    ;echo  Usage : Drag and Drop your batch file over this script:"%~nx0"  
    ;Timeout /T 5 /nobreak>nul & Exit
;)
;set "target.exe=%__cd__%%~n1.exe"
;set "batch_file=%~f1"
;set "bat_name=%~nx1"
;set "bat_dir=%~dp1"
;Set "sed=%temp%\2exe.sed"
;echo              Please  wait a while ...  Creating "%~n1.exe" ...
;copy /y "%~f0" "%sed%" >nul
;(
    ;(echo()
    ;(echo(AppLaunched=cmd /c "%bat_name%")
    ;(echo(TargetName=%target.exe%)
    ;(echo(FILE0="%bat_name%")
    ;(echo([SourceFiles])
    ;(echo(SourceFiles0=%bat_dir%)
    ;(echo([SourceFiles0])
    ;(echo(%%FILE0%%=)
;)>>"%sed%"

;iexpress /n /q /m %sed%
;del /q /f "%sed%"
;exit /b 0

[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles

[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
FriendlyName=-
PostInstallCmd=<None>
AdminQuietInstCmd=