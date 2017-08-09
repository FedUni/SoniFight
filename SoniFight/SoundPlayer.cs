﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IrrKlang;

namespace SoniFight
{
    public class SoundPlayer
    {        
        // We'll build up a dictionary (i.e. list of key-value pairs) mapping the sample filename to the actual loaded sample
        // which can be used by any given GameConfig        
        private static Dictionary<string, ISound> sampleDictionary = new Dictionary<string, ISound>();

        // The sound engine object itself
        private static ISoundEngine soundEngine;

        

        // Constructor
        public SoundPlayer()
        {
            soundEngine = new ISoundEngine();            
        }

        public static bool LoadSample(string sampleKey, bool loopSample)
        {
            // Print just the sample name being loaded
            int index = sampleKey.LastIndexOf("\\") + 1;
            string shortKey = sampleKey.Substring(index, sampleKey.Length - index);
            Console.WriteLine("Loading sample: " + shortKey);

            // Load the sound by playing it. Params: sample filename, loop, start paused
            ISound sound = soundEngine.Play2D(sampleKey, loopSample, true, StreamMode.NoStreaming, true);

            // Otherwise add to our sample list, increment the SAM and return true for success
            if ( !sampleDictionary.ContainsKey(sampleKey))
            {
                sampleDictionary.Add(sampleKey, sound);
            }

            return true;
        }

        public static bool SampleLoaded(string sampleFilename)
        {
            if (sampleDictionary.ContainsKey(sampleFilename))
            {
                return true;
            }
            return false;
        }

        public static bool CurrentlyPlaying(string sampleKey)
        {
            return soundEngine.IsCurrentlyPlaying(sampleKey);
        }


        public static void ToggleSamplePaused(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample))
            {
                sample.Paused = !sample.Paused;
            }
        }

        public static void PauseSample(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
                if (!sample.Paused)
                {
                    sample.Paused = true;
                }
            }
        }

        public static void ResumeSample(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
                if (sample.Paused)
                {
                    sample.Paused = false;
                }
            }
        }

        public static bool IsPaused(string sampleKey)
        {
            // Attempt to get the ISound (i.e. sample) with the given key name...
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample))
            {
                sample = sampleDictionary[sampleKey];
                return sample.Paused;
            }
            else // Samples not loaded so key does not yet exist in dictionary? Then yes, I'd say we're paused.
            {
                return false;
            }
        }


        public static void ChangeSampleVolume(string sampleKey, float volume)
        {
            // If we're playing the sample...
            if (soundEngine.IsCurrentlyPlaying(sampleKey))
            {
                // ...get access to it. If successfully found, modify its volume.
                ISound sample = sampleDictionary[sampleKey];
                sample.Volume = volume;
            }                
        }

        public static void ChangeSamplePitch(string sampleKey, float pitch)
        {
            // If we're playing the sample...
            if (soundEngine.IsCurrentlyPlaying(sampleKey))
            {
                // ...get access to it. If successfully found, modify its volume.
                ISound sample = sampleDictionary[sampleKey];
                sample.PlaybackSpeed = pitch;
            }
        }

        // Method to check if any sample in the dictionary is currently playing
        public static bool IsPlaying()
        {        
            try
            {
                foreach (KeyValuePair<string, ISound> sample in sampleDictionary)
                {
                    if (soundEngine.IsCurrentlyPlaying(sample.Key))
                    {
                        //Console.WriteLine("FOUND PLAYING SAMPLE: " + sample.Key);
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

        // Method to update the SoundEngine
        public static void updateEngine()
        {
            // We must call the Update method on the SoundEngine several times per second for everything to run smoothly, especially
            // when moving sounds around. As such, this is called from the main loop 1000 / POLL_SLEEP_MS times per second.
            soundEngine.Update();
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public static void Play(string sampleKey, float volume, float pitch, bool loopSample)
        {
            // If we have a sample loaded with a given filename...
            if (sampleDictionary.ContainsKey(sampleKey))
            {
                ISound sample;
                if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
                {
                    // ...then set the volume and pitch of the specified sample and then play it
                    sample.Volume = volume;
                    sample.PlaybackSpeed = pitch;

                    // Sample, play looped, start-paused, stream-mode, enable-sound-effects
                    sample = soundEngine.Play2D(sampleKey, loopSample, false, StreamMode.AutoDetect, true);                    
                }
                else
                {
                    Console.WriteLine("Sample not found: " + sampleKey);
                }
            }
            else // Warn user of issue
            {
                Console.WriteLine("WARNING: Sample key: " + sampleKey + " does not exist in sampleDictionary.");
            }            
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
            // Free the samples and finally realease the irrKlang sound engine itself
            UnloadAllSamples();
            soundEngine.Dispose();
        }

    } // End of SoundPlayer class

} // End of SoniFight namespace
