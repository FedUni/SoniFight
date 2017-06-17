# SoniFight #

SoniFight is utility software to provide additional sonification to fighting games for visually impaired players. It is written in C# and released under the MIT license. Please see LICENSE.txt for further details, including separate licensing details for the embedded IrrKlang audio library. If you want to quickly get an idea of what the SoniFight software can do then a demonstration video can be found here: LINK_TO_YOUTUBE_VID.

To run SoniFight either download a release or build the Visual Studio 2015 project provided, then launch the SoniFight executable, choose a game config for the game you want to play and then click the "Run Selected Config" button. SoniFight presently ships with game configs to add sonification to Ultra Street Fighter IV Arcade Edition and Mortal Kombat 9 (aka Mortal Kombat Komplete Edition).

Once running, SoniFight will locate the game process and provide additional sonification cues such as clock, health and bar status updates for both players. In the Street Figher config there are also triggers for a large number of menu options.

SoniFight also contains a user interface where you can create your own game configs for games of your choice, although the process to find pointer trails can be a little bit tricky and time consuming. To learn more about creating your own game configs as well as how the software operates through 'watches' and 'triggers', please see the provided user documentation.

## Frequently Asked Questions ##

#### Q1 - Does SoniFight support game X? / Could you write a config for game X? ####
A1 - At present SoniFight only ships with configs which support the games Ultra Street Fighter IV Arcade Edition and Mortal Kombat 9 (aka Mortal Kombat Komplete Edition) as proofs of concept. However, SoniFight was built to run multiple configurations for various games with the idea being that you can create a config for a game then it to add additional sonification cues to that game.

The problem is that I'm only one man, and if there are a lot of requests I simply don't have the time to create additional configs for various games because as soon as this project ships I have to move on to the next one in an effort to gain my PhD in the short time I have remaining to do so.

Tutorials are provided on how to use the Cheat Engine software to find pointer trails to values so that people can create their own configs for the games that they want. Unfortunately, the process to find these pointer trails would be difficult for a non-sighted person to perform, but I would hope that with some sighted assistance that configs could be made for a variety of different fighting games.

#### Q2 - Is SoniFight cheating? If I use it online will it get me banned from services like Steam? ####
A2 - SoniFight only aims to provide the same audio cues a sighted fighting game player has natively available, but through audio for those who may be partially or non-sighted. A sighted player will gain no real benefit from using this software because they can just glance at things - so I don't consider this cheating at all.

Whether or not using this software will get you banned from something like Steam is a harder question to unequivacably answer. I've been developing this software using Street Fighter IV running through Steam for over a year without any issues or problems. SoniFight only ever reads memory locations and provides sonification cues from values it encounters - it never writes to memory, and it does not attach a debugger to the host process. Please be aware that while I seriously doubt that you'd be banned from a gaming service for using this software, I cannot be held responsible should it occur and as the software license states - you use this software entirely at your own risk.

#### Q3 - I've made a config! Can you ship it with the next release? ####
A3 - Quite possibly! As long as your config works and does not use copyrighted materials I can incorporate it into the next release of the software so that more games are supported 'out-of-the-box' as it were. Please be aware that I can't ship copyrighted audio because I don't own the rights to do so, and unfortunately that includes ripping audio from the existing game (for example, the announcer saying the character names). While it would definitely make the audio more cohesive, I don't have the rights to distribute their audio.

#### Q4 - Both my friend and I are partially or non-sighted, can we play against each other properly? ####
A4 - Yup! The configs that ship with this release provide sonification for both player 1 and player 2 using different voices so that they can be easily told apart. If you don't find that you can easily differentiate between the voices you may like to speed up or slow down the playback by modifying the trigger(s) associated with a given in-game event via the edit tab.

#### Q5 - I want to add additional triggers, how easy it is to do? ####
A5 - That depends on whether the watch associated with a trigger already exists, or if it has to be found. For example, a watch exists for the player 1 and player 2 health bars that triggers when they hit 500, 250 and 100. Lets say you wanted to add a trigger for when player 1's health hits 750 - the easiest way would be to just clone the 500 health trigger and change the value of the clone to 750 (and possibly rename the trigger - it'll have the word CLONE appended to the name). That's the simple scenario.

If there isn't a watch for the specific value you want, then a pointer trail to that memory location must be found so that we can repeatedly find the value across game launches (i.e. it should work every time, not just this one time). Further details on the process of finding pointer trails are provided elsewhere in this documentation.

#### Q6 - I only want some of the triggers to play, can I disable the ones I don't find useful? ####
A6 - Absolutely. Every trigger has an active flag associated with it - just select the trigger(s) you want to turn off and uncheck the "Active" checkbox for that trigger in the edit tab.

#### Q7 - How are configs shipped? ####
A7 - Each config is simply a folder that lives inside SoniFight's Configs folder. It contains the file config.xml which stores all the GameConfig details for that particular game, along with a number of audio samples which are played when trigger conditions are met. If you've created a config and want to share it with someone, simply zip up the folder to send and tell the receiving person to extract it inside SoniFight's Configs folder.

#### Q8 - What platforms does SoniFight run on? ####
A8 - SoniFight runs on Windows only. Any version from Windows Vista onwards should be fine. The SoniFight application itself is 32-bit and can only connect to 32-bit processes. Should there be sufficient demand I could also provide a native 64-bit version, but this would then only be able to connect to 64-bit processes.

#### Q8 - Can I have access to and modify the SoniFight source code? Can I sell it? ####
A8 - Yes and no. SoniFight is released under a M.I.T. license, which broadly means that you may have the source code for no charge and can do with it as you please - including modifying it to your heart's content. If you're technically minded and provide a worthwhile pull request to the Github codebase I'll happily merge it in. If you don't want to merge in to my branch then you're of course free to keep your fork of the codebase separate.

However, SoniFight uses the irrKlang library for audio playback, and while free for non-commercial use, you cannot sell the irrKlang component of the SoniFight software without purchasing an irrKlang Pro (i.e. commercial) license to do so. For further details of irrKlang licensing, please see http://www.ambiera.com/irrklang/irrklang_pro.html