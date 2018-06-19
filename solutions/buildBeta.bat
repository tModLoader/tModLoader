:: We must clean since msbuild won't know if previous build was beta or not.
msbuild tModLoader.sln /t:clean /p:Configuration=WindowsRelease
msbuild tModLoader.sln /t:clean /p:Configuration=WindowsServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxServerRelease
msbuild tModLoader.sln /p:Configuration=WindowsRelease /p:Platform="x86" /p:DefineConstants="BETA;CLIENT;WINDOWS"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=WindowsServerRelease /p:Platform="x86" /p:DefineConstants="BETA;SERVER;WINDOWS"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacRelease /p:Platform="x86" /p:DefineConstants="BETA;CLIENT;MAC"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacServerRelease /p:Platform="x86" /p:DefineConstants="BETA;SERVER;MAC"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxRelease /p:Platform="x86" /p:DefineConstants="BETA;CLIENT;LINUX"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxServerRelease /p:Platform="x86" /p:DefineConstants="BETA;SERVER;LINUX"
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)