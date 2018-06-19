msbuild tModLoader.sln /t:clean /p:Configuration=WindowsRelease
msbuild tModLoader.sln /t:clean /p:Configuration=WindowsServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxServerRelease
msbuild tModLoader.sln /p:Configuration=WindowsRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=WindowsServerRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacServerRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxServerRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)