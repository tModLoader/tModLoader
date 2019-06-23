msbuild tModLoader.sln /restore /p:Configuration=WindowsDebug
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=WindowsServerDebug
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
