msbuild tModLoader.sln /p:Configuration=WindowsDebug /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=WindowsServerDebug /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
