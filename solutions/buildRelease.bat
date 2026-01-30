SET PATH="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\";%PATH%
msbuild tModLoader.sln /restore /t:clean /p:Configuration=WindowsRelease
msbuild tModLoader.sln /t:clean /p:Configuration=WindowsServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacRelease
msbuild tModLoader.sln /t:clean /p:Configuration=MacServerRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxRelease
msbuild tModLoader.sln /t:clean /p:Configuration=LinuxServerRelease
msbuild tModLoader.sln /p:Configuration=WindowsRelease /p:GenerateDocumentation=true /p:DocumentationFile=tModLoader.xml /p:WarningLevel=0
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=WindowsServerRelease /p:WarningLevel=0
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacRelease /p:WarningLevel=0
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=MacServerRelease /p:WarningLevel=0
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxRelease /p:WarningLevel=0
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)
msbuild tModLoader.sln /p:Configuration=LinuxServerRelease /p:WarningLevel=0
@IF %ERRORLEVEL% NEQ 0 (EXIT /B %ERRORLEVEL%)