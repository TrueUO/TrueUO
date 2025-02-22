# [TrueUO]

[![GitHub issues](https://img.shields.io/github/issues/trueuo/trueuo.svg)](https://github.com/TrueUO/TrueUO/issues)
[![GitHub repo size](https://img.shields.io/github/repo-size/trueuo/trueuo.svg)](https://github.com/TrueUO/TrueUO/)

TrueUO started as a ServUO testing branch where Dexter, Argalep, and I would code stuff and test it out before deploying the commits to ServUO. ServUO systems like Vendor Search and the Pet Training Revamp were all coded and tested on TrueUO first. When ServUO reverted back to including support for older clients and eras the two branches slowly drifted apart. Now it is a standalone emulator but heavily based and influenced by ServUO and ModernUO.

Some distinct features/differences:
* only supports one era (most recent) / no era checks.
  - this greatly simplifies the code base and makes it easier to follow mechanics.
* only supports the newest production client / CUO / Orion.
* supports the newest production client files.
* uses the newest version of .NET (.NET 9 as of this writing)
* uses a new threaded save system that is 2x faster than current other RunUO versions.
* uses the timer wheel system from ModernUO for faster and more accurate timers.
* only works on Windows currently.
  - there were some changes that needed to be made to get it working off of .net framework that may have broken support for Linux. I am not a Linux user so until/if someone wants to tackle that it will remain that way for the foreseeable future.

Some things being worked on:
* moving most packets out of the server and eventually converting packets over to the ModernUO implementation.
* potentially merging server and scripts so both can easily communication with each other.

What I recommend:
* if you are new to the emulator scene check out ServUO or ModernUO. They have bigger more supporting communities.
* if you are a seasoned UO emulator veteran then you might enjoy poking around TrueUO too.
* if you are interested in creating a custom project then TrueUO might be a good starting point. Since there are no era checks and functions are more straightforward, I think it is easier to use as a base when the intent is something custom or not entirely based on UO mechanics.

If you find bugs please submit them via GitHub.

Always looking for help/bug fixing/enhancement support.

Join our Discord Server: https://discord.gg/5RtDbkY3fC
