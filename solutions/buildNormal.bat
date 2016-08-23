msbuild tModLoader.sln /p:Configuration=WindowsRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (
pause
EXIT /B %ERRORLEVEL%
)
msbuild tModLoader.sln /p:Configuration=WindowsServerRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (
pause
EXIT /B %ERRORLEVEL%
)
msbuild tModLoader.sln /p:Configuration=MacRelease /p:Platform="x86"
@IF %ERRORLEVEL% NEQ 0 (
pause
EXIT /B %ERRORLEVEL%
)
pause
