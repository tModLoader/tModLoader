msbuild tModLoader.sln /restore /t:clean /p:Configuration=WindowsRelease
msbuild tModLoader.sln /t:clean /p:Configuration=WindowsServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxServerRelease
msbuild tModLoader.sln /p:Configuration=WindowsRelease /p:GenerateDocumentation=true /p:DocumentationFile=tModLoader.xml
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=WindowsServerRelease
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacRelease
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacServerRelease
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxRelease
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxServerRelease
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)