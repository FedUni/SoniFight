using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoniFight
{
    public class SoundPlayer
    {
        // Import functionality to load a library
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        private static int numSamples;

        private static int SAMPLE_INDEX = 0;

        //public const int SONG_INDEX_1 = 0;
        //public const int SONG_INDEX_2 = 1;
        
        // The FMOD system itself and a channel used to play audio (multiple samples can play at once)
        private static FMOD.System FMODSystem;
        private static FMOD.Channel Channel;

        // We group sounds in InGame and InMenu SoundGroups so that we can allow a large number of InGame sounds
        // to play at once, while limiting the number of InMenu sounds to 1 so that the last sound always 'overwrites'
        // any currently playing menu sonification. This stops us from 'buffering' sonification events as we quickly
        // move through menus (which sounds like a cacophany!)
        static FMOD.SoundGroup InGameSoundGroup;
        static FMOD.SoundGroup InMenuSoundGroup;


        // We'll build up a dictionary (i.e. list of key-value pairs) mapping the sample filename to the actual loaded sample
        // which can be used by any given GameConfig
        private static Dictionary<string, FMOD.Sound> sampleDictionary = new Dictionary<string, FMOD.Sound>();

        

        public SoundPlayer()
        {
            if (Environment.Is64BitProcess) { LoadLibrary(System.IO.Path.GetFullPath("FMOD\\64bit\\fmod.dll")); }
            else { LoadLibrary(System.IO.Path.GetFullPath("FMOD\\32bit\\fmod.dll")); }

            numSamples = 0;

            FMOD.Factory.System_Create(out FMODSystem);

            FMODSystem.setDSPBufferSize(1024, 10);
            FMODSystem.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)0);

            FMODSystem.createSoundGroup("InGameSoundGroup", out InGameSoundGroup);
            FMODSystem.createSoundGroup("InMenuSoundGroup", out InMenuSoundGroup);
        }

        public static bool LoadSample(string configDirectory, string sampleName, Trigger.AllowanceType at)
        {
            //MainForm.gameConfig.ConfigDirectory

            // Append trailing backslash if necessary
            int l = configDirectory.Length;
            if (!configDirectory.EndsWith("\\"))
            {
                configDirectory += "\\";
            }
            
            configDirectory = ".\\Configs\\" + configDirectory;

            Console.WriteLine("Loading sample: " + sampleName + " from: " + configDirectory);

            // Load our sound
            FMOD.Sound sound;
            FMOD.RESULT r = FMODSystem.createSound(configDirectory + sampleName, FMOD.MODE.DEFAULT, out sound);
            
            //if (at == Trigger.AllowanceType.InGame)
            //{
            //      InGameSoundGroup.add   
            //}

            // Moan if anything bad happened and return false for failure
            if (r != FMOD.RESULT.OK)
            {
                MessageBox.Show("Loading sample " + (configDirectory + sampleName) + " at index " + SAMPLE_INDEX + " failed, got result " + r);
                return false;
            }

            // Otherwise add to our sample list, increment the SAM and return true for success
            sampleDictionary.Add(sampleName, sound);

            ++numSamples;

            return true;
        }

        public static bool SampleLoaded(string sampleName)
        {
            if (sampleDictionary.ContainsKey(sampleName))
            {
                return true;
            }
            return false;
        }

        public static bool IsPlaying()
        {
            bool isPlaying = false;
            if (Channel != null) { Channel.isPlaying(out isPlaying); }
            return isPlaying;
        }

        
        public static void Play(string sampleFilename, float volume, float pitch)
        {
            // If we have a sample loaded with a given filename, then play it...
            if ( sampleDictionary.ContainsKey(sampleFilename) )
            {

                //TODO: Really need to use a ChannelGroup here with a pool of channels so I can play on the next available channel because
                //      I think changing the volume/pitch on the channel will change it for all samples playing not just 'this' sample being played.

                // NOTE 1: Playing a sound with Channel as the output CREATES the Channel - there's no constructor for it - which means...
                // Params: Sound, ChannelGroup, Paused, Channel
                FMODSystem.playSound(sampleDictionary[sampleFilename], null, false, out Channel);

                // IMPORTANT: If we don't call FMODSystem.update() often then we use all our sample spots and only one sample can be played at a time.
                //            I believe calling update 'clears' samples which have finished playing, allowing those spots to be used in muxing again
                //            so our multi-sample-at-once audio keeps working properly. See: https://www.fmod.org/questions/question/forum-29440/
                FMODSystem.update();

                // NOTE 2: However, the creation seems fine to adjust this even if the first use of the Channel was just above, and it'll modify the volume and pitch as planned
                if (Channel != null)// !IsPlaying())
                {
                    Channel.setVolume(volume);
                    Channel.setPitch(pitch);                    
                }
            }
            else // Warn user of issue
            {
                string s = "WARNING: Sample: " + sampleFilename + " does not exist in sampleDictionary.";
                Console.WriteLine(s);
            }
            
        }

        /*
        public void SetVolume(float volume)
        {
            if (Channel != null)
            {
                if (volume < 0.0f) { volume = 0.0f; }
                if (volume > 1.0f) { volume = 1.0f; }
                Channel.setVolume(volume);
            }
                
            //Channel.setVolume(Settings.GetInstance().MusicVolume / 100f);
        }

        public void SetPitch(float pitch)
        {
            if (Channel != null)
            {
                if (pitch < 0.5f) { pitch = 0.5f; }
                if (pitch > 4.0f) { pitch = 4.0f; }
                Channel.setPitch(pitch);
            }

            //Channel.setVolume(Settings.GetInstance().MusicVolume / 100f);
        }

        // Default pitch is 1.0f, half is 0.5, double is 2.0 etc
        public void IncreasePitch()
        {
            if (Channel != null)
            {
                float pitch;
                Channel.getPitch(out pitch);
                if (pitch < 2.0f)
                {
                    Channel.setPitch(pitch + 0.1f);
                }
            }
        }

        // Default pitch is 1.0f, half is 0.5, double is 2.0 etc
        public void DecreasePitch()
        {
            if (Channel != null)
            {
                float pitch;
                Channel.getPitch(out pitch);
                if (pitch > 0.5f)
                {
                    Channel.setPitch(pitch - 0.1f);
                }
            }
        }*/

        public static void StopChannel()
        {
            if (Channel != null && IsPlaying()) { Channel.stop(); }
        }

        //TODO: This isn't used - we just don't play stuff when in currentlyMuted state - delete method
        public static bool muteChannel()
        {
            FMOD.RESULT result = FMOD.RESULT.OK;
            if (Channel != null)
            {
                result = Channel.setMute(true);
            }
            if (result == FMOD.RESULT.OK)
            {
                return true;
            }
            return false;
        }

        //TODO: This isn't used - we just don't play stuff when in currentlyMuted state - delete method
        public static bool unmuteChannel()
        {
            FMOD.RESULT result = FMOD.RESULT.OK;
            if (Channel != null)
            {
                result = Channel.setMute(false);
            }
            if (result == FMOD.RESULT.OK)
            {
                return true;
            }
            return false;
        }


        public static bool isMuted()
        {
            FMOD.RESULT result = FMOD.RESULT.OK;
            bool muted = false;
            if (Channel != null)
            {
                result = Channel.getMute(out muted);
            }
            return muted;
        }

        public static void UnloadAllSamples()
        {
            if (IsPlaying()) { Channel.stop(); }


            // Release all samples in the dictionary then clear it
            foreach (KeyValuePair<string, FMOD.Sound> sample in sampleDictionary)
            {
                Console.WriteLine("Releasing sample: " + sample.Key);
                sample.Value.release();
            }
            sampleDictionary.Clear();
        }

        public static void ShutDown()
        {
            // Free the samples and finally realease the FMODSystem itself
            UnloadAllSamples();
            FMODSystem.release();
        }
    }
}
