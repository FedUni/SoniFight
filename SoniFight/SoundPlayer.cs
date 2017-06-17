using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IrrKlang;

namespace SoniFight
{
    public class SoundPlayer
    {
        // Import functionality to load a library
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        private static int numSamples;
        
        private static bool soundPlaying = false;
        
        // We'll build up a dictionary (i.e. list of key-value pairs) mapping the sample filename to the actual loaded sample
        // which can be used by any given GameConfig
        //private static Dictionary<string, ISoundSource> sampleDictionary = new Dictionary<string, ISoundSource>();
        private static Dictionary<string, ISound> sampleDictionary = new Dictionary<string, ISound>();

        private static ISoundEngine soundEngine;

        public SoundPlayer()
        {
            numSamples = 0;
            soundEngine = new ISoundEngine();            
        }

        public static bool LoadSample(string configDirectory, string sampleName)
        {
            // Append trailing backslash if necessary
            int l = configDirectory.Length;
            if (!configDirectory.EndsWith("\\"))
            {
                configDirectory += "\\";
            }
            
            configDirectory = ".\\Configs\\" + configDirectory;

            Console.WriteLine("Loading sample: " + sampleName);

            // Load the sound. Params: sample filename, loop, start paused
            ISound sound = soundEngine.Play2D(configDirectory + sampleName, false, true);
            soundEngine.StopAllSounds();

            // Otherwise add to our sample list, increment the SAM and return true for success
            if ( !sampleDictionary.ContainsKey(configDirectory + sampleName))
            {
                sampleDictionary.Add(configDirectory + sampleName, sound);
                //Console.WriteLine("Added: " + configDirectory + sampleName);
            }

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
            // Release all samples in the dictionary then clear it
            try
            {
                foreach (KeyValuePair<string, ISound> sample in sampleDictionary)
                {
                    if (soundEngine.IsCurrentlyPlaying(sample.Key))
                    {
                        Console.WriteLine("FOUND PLAYING SAMPLE: " + sample.Key);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception during IsPlaying check: " + e.Message);
            }
            return false;
        }

        // Update the SoundEngine
        public static void updateEngine()
        {
            // We must call the Update method on the SoundEngine several times per second for everything to run smoothly, especially
            // when moving sounds around. As such, this is called from the main loop 1000 / POLL_SLEEP_MS times per second.
            soundEngine.Update();
        }

        
        public static void Play(string sampleFilename, float volume, float pitch)
        {
            // If we have a sample loaded with a given filename, then play it...
            if ( sampleDictionary.ContainsKey(sampleFilename) )
            {
                //TODO: Really need to use a ChannelGroup here with a pool of channels so I can play on the next available channel because
                //      I think changing the volume/pitch on the channel will change it for all samples playing not just 'this' sample being played.

                sampleDictionary[sampleFilename].Volume = volume;
                sampleDictionary[sampleFilename].PlaybackSpeed = pitch;
                soundEngine.Play2D(sampleFilename);
            }
            else // Warn user of issue
            {
                string s = "WARNING: Sample: " + sampleFilename + " does not exist in sampleDictionary.";
                Console.WriteLine(s);
            }
            
        }

        public static void StopChannel()
        {
            //if (Channel != null && IsPlaying()) { Channel.stop(); }
            soundEngine.StopAllSounds();
        }

        
        // Method to unload all samples and clear the sample dictionary
        public static void UnloadAllSamples()
        {
            //if (IsPlaying()) { Channel.stop(); }
            soundEngine.StopAllSounds();

            // Release all samples in the dictionary then clear it
            foreach (KeyValuePair<string, ISound> sample in sampleDictionary)
            {
                Console.WriteLine("Releasing sample: " + sample.Key);
                sample.Value.Dispose();
            }
            sampleDictionary.Clear();

            // Unload all samples and force garbage collection
            soundEngine.RemoveAllSoundSources();
            System.GC.Collect();
        }

        public static void ShutDown()
        {
            // Free the samples and finally realease the FMODSystem itself
            UnloadAllSamples();

            //FMODSystem.release();
            soundEngine.Dispose();
        }
    }
}
