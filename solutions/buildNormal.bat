msbuild tModLoader.sln /restore /p:Configuration=WindowsRelease
@IF %ERRORLEVEL% NEQ 0 (
pause
EXIT /B %ERRORLEVEL%
)
msbuild tModLoader.sln /p:Configuration=WindowsServerRelease
@IF %ERRORLEVEL% NEQ 0 (
pause
EXIT /B %ERRORLEVEL%
)
msbuild tModLoader.sln /p:Configuration=MacRelease
@IF %ERRORLEVEL% NEQ 0 (
pause
EXIT /B %ERRORLEVEL%
)
pause
