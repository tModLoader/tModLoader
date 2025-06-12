@echo off
if "%DOTNET_EnableWriteXorExecute%"=="" (
	setx DOTNET_EnableWriteXorExecute 0
	echo "DOTNET_EnableWriteXorExecute has been set as 0 for .NET8 Compatibility"
)
