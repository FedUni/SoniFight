# SoniFight #

SoniFight is a Windows application designed to provide additional sonification cues to video games for blind or visually impaired players. It is written in C# and released under the MIT license. Please see LICENSE.txt for further details, including separate licensing details for the embedded tolk and irrKlang libraries.

If you want to quickly get an idea of what the SoniFight software can do then a demonstration video can be found here: https://www.youtube.com/watch?v=qHvcVv_BdmE

To run SoniFight either download a release or build the Visual Studio 2017 project provided, then launch the SoniFight executable, choose a game config from the dropdown menu for the game you want to play and click the "Run Selected Config" button then launch the associated game. Once running, SoniFight will locate the game process and provide additional sonification cues as specified in the game configuration file.

SoniFight presently ships with game configs to add sonification to the following games:
- Street Fighter 4 (good),
- Mortal Kombat 9, (okay), and
- Killer Instinct (Windows Store edition, 64-bit), (proof-of-concept).

SoniFight also contains a comprehensive user interface where you can create your own game configs for games of your choice. To learn more about creating your own game configs as well as how the software operates through 'watches' and 'triggers', or to take a look at the answers to some frequently asked questions please see the provided user documentation.