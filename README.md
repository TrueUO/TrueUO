# [ServUO]

[![GitHub issues](https://img.shields.io/github/issues/trueuo/trueuo.svg)](https://github.com/TrueUO/TrueUO/issues)
[![GitHub](https://img.shields.io/github/license/servuo/servuo.svg?color=a)](https://github.com/TrueUO/TrueUO/blob/master/LICENSE)


ServUO is a community driven Ultima Online Server Emulator written in C#.

## Publishing a build
#### Requirements
- [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

#### Publishing Builds
- Using terminal or powershell: `./publish.cmd [release|debug (default: release)] [os]`
  - Supported `os`:
    - `win` for Windows 8/10/2019
    - `osx` for MacOS
    - `ubuntu.16.04`, `ubuntu.18.04` `ubuntu.20.04` for Ubuntu LTS
    - `debian.9`, `debian.10` for Debian
    - `centos.7`, `centos.8` for CentOS
    - If blank, will use host operating system
  - Supported `framework`:
    - `net` for .NET 5.0

#### Example
- Windows (Debug Mode): `./publish.cmd Debug core win`
- Windows (Release Mode): `./publish.cmd Release core win`

https://www.servuo.com

### Development

Want to contribute? Great!

You can submit a pull request at any time and we will review it asap!

License
----

GPL v2


   [ServUO]: <https://www.servuo.com>
   [Quickstart]: <https://www.servuo.com/wiki/startup/>
