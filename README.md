# SoniFight #

SoniFight is a Windows application designed to provide additional sonification cues to video games for blind or visually impaired players. It is written in C# and released under the MIT license. Please see LICENSE.txt for further details, including separate licensing details for the embedded tolk and irrKlang libraries.

If referencing this project in your research, APA style referencing for the accompaniying research paper is:

Lansley, A., Vamplew, P., Foale, C., & Smith, P. (2018). SoniFight: Software to Provide Additional Sonification Cues to Video Games for Visually Impaired Players. The Computer Games Journal, 7(2), 115-130.

If you want to get an idea of what the SoniFight software can do then a demonstration video can be found here: https://www.youtube.com/watch?v=qHvcVv_BdmE

## Quick Start ##

To use SoniFight, please download a pre-compiled release from: https://github.com/FedUni/SoniFight/releases then run the SoniFight_x86.bat batch file for 32-bit games (it's likely this is the one you want) or the SoniFight_x64.bat file for 64-bit games (currrently proof of concept only).

Once running choose a game config from the dropdown menu for the game you want to play and click the "Run Selected Config" button, then launch the associated game. SoniFight will connect to the game process and provide additional sonification cues as specified in the game configuration file.

SoniFight presently ships with game configs to add sonification to the following games:
- Street Fighter 4 (good, steam version),
- Mortal Kombat 9, (good, steam version),
- BlazBlue Continuum Shift Extra (good, stream version),
- Day of the Tentacle Remastered (good, steam version),
- Beneath a Steel Sky (good, Good Old Games version),
- Divekick (main menus only, steam version), and
- Killer Instinct (64-bit proof of concept only, Windows Store version).

SoniFight also contains a comprehensive user interface where you can create your own game configs for games of your choice.

## Documentation ##

To learn more about creating your own game configs as well as how the software operates through 'watches' and 'triggers', or to take a look at the answers to some frequently asked questions please see the provided user documentation here: [SoniFight User Guide.pdf](https://github.com/FedUni/SoniFight/blob/master/Documentation/SoniFight_User_Guide.pdf)