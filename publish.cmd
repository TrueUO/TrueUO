dotnet restore --force-evaluate


:<<"::SHELLSCRIPT"
@ECHO OFF
GOTO :CMDSCRIPT

::SHELLSCRIPT

CURPATH=`pwd`
PUBLISHPATH=${CURPATH}/Distribution
EXENAME=ServUO

os=$3

if [[ $os ]]; then
  r="-r $os-x64"
elif [[ $(uname) = "Darwin" ]]; then
  r="-r osx-x64"
elif [[ -f /etc/os-release ]]; then
  . /etc/os-release
  NAME="$(tr '[:upper:]' '[:lower:]' <<< $NAME)"
  r="-r $NAME.$VERSION_ID-x64"
fi

framework="$(tr '[:upper:]' '[:lower:]' <<< $2)"

if [[ $framework = "net" ]]; then
  f="-f net5.0"
else
  f="-f netcoreapp3.1"
fi

if [[ -z $1 ]]; then
  c="-c Release"
else
  config="$(tr '[:lower:]' '[:upper:]' <<< ${1:0:1})${1:1}"
  c="-c $config"
fi

echo dotnet publish ${c} ${r} ${f} --no-restore --self-contained=false -o Distribution Application/ServUO.csproj
dotnet publish ${c} ${r} ${f} --no-restore --self-contained=false -o Distribution Application/ServUO.csproj

dotnet ${PUBLISHPATH}/${EXENAME}.dll

exit $?

:CMDSCRIPT

SET CURPATH=%~dp0%Distribution\
SET EXENAME=ServUO

set result=false
if "%~1" == "" set result=true
if "%~1" == "release" set result=true

if "%result%" == "true" (
  SET c=-c Release
) ELSE (
  SET c=-c Debug
)

IF "%~2" == "" (
  SET f=-f netcoreapp3.1
) ELSE (
  IF "%~2" == "core" (
    SET f=-f netcoreapp3.1
  ) ELSE (
    SET f=-f net5.0
  )
)

IF "%~3" == "" (
  SET r=-r win-x64
) ELSE (
  SET r=-r %~1-x64
)

echo dotnet publish %c% %r% %f% --no-restore --self-contained=false -o Distribution Application/ServUO.csproj
dotnet publish %c% %r% %f% --no-restore --self-contained=false -o Distribution Application/ServUO.csproj

"%CURPATH%%EXENAME%.exe"
