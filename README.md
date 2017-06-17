# SoniFight

SoniFight is C# software to provide additional sonification to fighting games for visually impaired players and is released under the MIT license. Please see LICENSE.txt for further details, including separate licensing details for the embedded IrrKlang audio library. If you want to quickly get an idea of what the SoniFight software can do then a demonstration video is provided here: LINK_TO_YOUTUBE_VID.

To run SoniFight either download a release or build the Visual Studio 2015 project provided, then launch the SoniFight executable, choose a game config for the game you want to play and then click the "Run Selected Config" button. SoniFight presently ships with game configs to add sonification to Ultra Street Fighter IV Arcade Edition and Mortal Kombat 9 (aka Mortal Kombat Komplete Edition).

Once running, SoniFight will locate the game process and provide additional sonification cues such as clock, health and bar status updates for both players. In the Street Figher config there are also triggers for a large number of menu options. SoniFight also contains a user interface where you can create your own game configs for games of your choice, although the process to find pointer trails can be a little bit tricky and time consuming. To learn more about creating your own game configs as well as how the software operates through 'watches' and 'triggers', please see the provided user documentation.