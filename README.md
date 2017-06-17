# SoniFight #

SoniFight is utility software to provide additional sonification to fighting games for visually impaired players. It is written in C# and released under the MIT license. Please see LICENSE.txt for further details, including separate licensing details for the embedded IrrKlang audio library. If you want to quickly get an idea of what the SoniFight software can do then a demonstration video can be found here: LINK_TO_YOUTUBE_VID.

To run SoniFight either download a release or build the Visual Studio 2015 project provided, then launch the SoniFight executable, choose a game config from the dropdown menu for the game you want to play and then click the "Run Selected Config" button. SoniFight presently ships with game configs to add sonification to Ultra Street Fighter IV Arcade Edition and Mortal Kombat 9 (aka Mortal Kombat Komplete Edition).

Once running, SoniFight will locate the game process and provide additional sonification cues such as clock, health and bar status updates for both players. In the Street Figher config there are also triggers for a large number of menu options.

SoniFight also contains a user interface where you can create your own game configs for games of your choice, although the process to find pointer trails can be a little bit tricky and time consuming. To learn more about creating your own game configs as well as how the software operates through 'watches' and 'triggers', please see the provided user documentation.

## Frequently Asked Questions ##

#### Does SoniFight support game X? / Could you write a config for game X? ####
At present SoniFight only ships with two configs that support Ultra Street Fighter IV Arcade Edition and Mortal Kombat 9 (aka Mortal Kombat Komplete Edition) as proof of concept. However, SoniFight was built to run configurations for various games with the idea being that users can create a config for a game you want to add additional sonification cues to.

In terms of writing configs for requested games, the problem is that I'm only one man and as much as I'd love to I simply don't have the time to create additional configs for various games because as soon as this project ships I have to move on to the next one in an effort to gain my PhD in the short time I have remaining to do so.

However, while I might not have the time to create new configs - perhaps you do? There's guidance in the user manual on how to use Cheat Engine to find pointer trails to values for use in new game configs for whatever fighting game you're interested in. Unfortunately, the process to find these pointer trails is difficult for a non-sighted person to perform, but I would hope that with some sighted assistance that configs could be made for a variety of different fighting games. And remember - once a config is made, it'll work forever (for that particular version of that particular game) - or even if one pointer trail is found, then it's found and there's no going back, so potentially making a solid game config could be a distributed 'many-hands-make-light-work' process, or at least that's my hope.

#### Does SoniFight use a lot of CPU or RAM? / Will it have a detrimental affect on game performance? ####
SoniFight will quite happily run using less than 1% CPU when using a game config with over 30 watches and 300 triggers and polling every tenth of a second, so it shouldn't affect the game's performance in any meaningful fashion. In terms of RAM usage it's directly dependent on the size of the samples associated with the game config (which all get loaded into memory). Before loading any samples the app will take up around 13MB of RAM, but even with the aforementioned game config loaded (which uses around 300 individual samples) we're still only up to around 50MB RAM usage.

#### Is SoniFight cheating? If I use it online will it get me banned from services like Steam? ####
SoniFight only aims to provide the same audio cues a sighted fighting game player has natively available, but through audio for those who may be partially or non-sighted. A sighted player will gain no real benefit from using this software because they can just glance at things - so I don't consider this cheating at all.

Whether or not using this software will get you banned from something like Steam is a harder question to unequivacably answer. I've been developing this software using Street Fighter IV running through Steam for over a year, including occassionally playing online matches, without any issues or problems. SoniFight only ever reads memory locations and provides sonification cues from the changes in values it encounters. It never writes to memory, and it does not attach a debugger to the host process. Please be aware that while I seriously doubt that you'd be banned from a gaming service for using this software, I cannot be held responsible should it occur and as the software license states in LICENSE.txt - you use this software entirely at your own risk.

#### I've made a config! Can you ship it with the next release? ####
Quite possibly! As long as your config works and does not use copyrighted audio materials I can incorporate it into the next release of the software so that more games are supported 'out-of-the-box' as it were. Please be aware that I can't ship copyrighted audio because I don't own the rights to do so, and unfortunately that includes ripping audio from the existing game (for example, the announcer saying the character names). While it would definitely make the audio more cohesive, as mentioned I don't have the right to distribute copyrighted audio.

#### Both my friend and I are partially or non-sighted, can we play against each other properly? ####
Yup! The configs that ship with this release provide sonification for both player 1 and player 2 using different voices so that they can be easily told apart. If you don't find that you can easily differentiate between the voices you may like to speed up or slow down the playback by modifying the trigger(s) associated with given in-game event(s) via the edit tab.

#### I want to add additional triggers, how easy it is to do? ####
That depends on whether the watch associated with a trigger already exists, or if it has to be found. If, for example, a watch exists for the player 1 health bar that triggers when they hit 500, 250 and 100 - and lets say you wanted to add a trigger for when player 1's health hits 750 - the easiest way would be to just clone the 500 health trigger and change the matching value of the clone to 750 and give it a sample to play (and rename the cloned trigger - it'll have the word CLONE appended to the name) and you're golden. That's the simple scenario - just do that, save the config and run it and you're laughing.

If there isn't a watch for the specific value you want, then a pointer trail to that memory location must be found so that we can repeatedly find the value across game launches and reboots (i.e. it should work every time, not just this one time, on everyone's PC). Further details on the process of finding pointer trails are provided in the user documentation.

#### I only want some of the triggers to play / random non-sensical menu triggers sometimes play, can I disable them? ####
Absolutely. Every trigger has an active flag associated with it - just select the trigger(s) you want to turn off and uncheck the "Active" checkbox for that trigger in the edit tab. Alternatively, you can delete the offensing trigger(s) entirely if you prefer.

#### How are configs shipped? ####
Each config is simply a folder that lives inside SoniFight's **Configs** folder. It contains the file config.xml (which stores all the GameConfig details for that particular game) along with a number of audio samples which are played when trigger conditions are met. If you've created a config and want to share it with someone, simply zip up the folder to send and tell the receiving person to extract it inside SoniFight's **Configs** folder.

#### What platforms does SoniFight run on? ####
SoniFight runs on Windows 7 and above only. The SoniFight application itself is 32-bit and can only connect to 32-bit processes, but it will happily run on a 64-bit system just like any other 32-bit process. Should there be sufficient demand I could also provide a native 64-bit version of SoniFight, but this would then only be able to connect to 64-bit processes.

#### Can I have access to and modify the SoniFight source code? Can I sell it? ####
Yes and no. SoniFight is released under a M.I.T. license, which broadly means that you may have the source code for no charge and can do with it as you please - including modifying it to your heart's content. If you're technically minded and provide a worthwhile pull request to the Github codebase I'll happily merge it in.

However, SoniFight uses the irrKlang library for audio playback, and while free for non-commercial use, you cannot sell the irrKlang component of the SoniFight software without purchasing an irrKlang Pro (i.e. commercial) license to do so. For further details of irrKlang licensing, please see http://www.ambiera.com/irrklang/irrklang_pro.html

#### I have an issue with the software or a question not covered here ####
Please feel free to raise any bug reports or issues on GitHub at: https://github.com/FedUni/SoniFight/issues