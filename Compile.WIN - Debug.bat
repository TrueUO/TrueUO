@SET CURPATH=%~dp0

@SET EXENAME=TrueUO

@TITLE: %EXENAME% - https://github.com/TrueUO/TrueUO

::##########

@ECHO:
@ECHO: Compile %EXENAME% for Windows
@ECHO:

@PAUSE

dotnet build -c Debug

@ECHO:
@ECHO: Done!
@ECHO:

@PAUSE

@CLS

::##########

@ECHO OFF

"%CURPATH%%EXENAME%.exe"

