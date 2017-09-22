using System;
using System.Collections.Generic;
using IrrKlang;

using au.edu.federation.SoniFight.Properties;

namespace au.edu.federation.SoniFight
{
    interface ISoundEventStopReceiver
    {
        void OnSoundStopped(ISound sound, StopEventCause reason, object userData);
    }

    // Class used to play and manipulate sonification events
    public class SoundPlayer 
    {
        // We'll build up a dictionary (i.e. list of key-value pairs) mapping the sample filename to the actual loaded sample
        // which can be used by any given GameConfig        
        private static Dictionary<string, ISound> sampleDictionary = new Dictionary<string, ISound>();

        // The sound engine object itself
        private static ISoundEngine soundEngine;

        public static bool readyToPlayNextQueuedSample = true;

        // Constructor
        public SoundPlayer()
        {
            soundEngine = new ISoundEngine();
        }

        public void OnSoundStopped(ISound sound, StopEventCause reason, object userData)
        {
            Console.WriteLine("Sound: " + sound.ToString() + " stopped for reason: " + reason);
        }

        // Method to load a sample and specify whether it should loop or not when played
        public static bool LoadSample(string sampleKey, bool loopSample)
        {
            // Print just the sample name being loaded.
            //NOTE: The key itself is of the form: ".\Configs\CONFIG_FOLDER_NAME\SAMPLE_NAME.FILE_EXTENSION"
            int index = sampleKey.LastIndexOf("\\") + 1;
            string shortKey = sampleKey.Substring(index, sampleKey.Length - index);
            Console.WriteLine("Loading sample: " + shortKey);

            // Load the sound by playing it. Params: sample filename, loop, start paused, stream mode, enable sound effects
            ISound sound = soundEngine.Play2D(sampleKey, loopSample, true, StreamMode.AutoDetect, true);

            // Otherwise add to our sample list, increment the SAM and return true for success
            if (!sampleDictionary.ContainsKey(sampleKey))
            {
                sampleDictionary.Add(sampleKey, sound);
            }

            //soundEngine.StopAllSounds();

            return true;
        }

        // Method to return whether a specific sample is currently loaded
        public static bool SampleLoaded(string sampleKey)
        {
            if (sampleDictionary.ContainsKey(sampleKey))
            {
                return true;
            }
            return false;
        }

        // Method to return whether a specific sample is currently playing or not
        public static bool CurrentlyPlaying(string sampleKey)
        {
            ISound sample;
            try
            {
                if (sampleDictionary.TryGetValue(sampleKey, out sample))
                {
                    if (soundEngine.IsCurrentlyPlaying(sampleKey) && !(sample.Paused))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("NOPE!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Exception was: " + e.Message);
            }
            return false;
        }

        // NOTE: Samples which are paused return as playing, but we only want samples which are actually playing!
        public static void printAllSamplesCurrentlyPlaying()
        {
            try
            {
                int x = 1;
                foreach (KeyValuePair<string, ISound> sample in sampleDictionary)
                {
                    if (soundEngine.IsCurrentlyPlaying(sample.Key) && !sample.Value.Paused)
                    {
                        Console.WriteLine( (x++) + " currently playing " + sample.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("MENTAL" + e.Message);
            }
        }

        // Method to toggle a sample between paused and unpaused states
        public static void ToggleSamplePaused(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample))
            {
                sample.Paused = !sample.Paused;
            }
        }

        // Method to toggle a sample between paused and unpaused states
        /*** THIS METHOD DOES NOT WORK =( ***/
        /*public static void StopPlayingSample(string sampleKey)
        {
            try
            {
                ISound sample;
                if (sampleDictionary.TryGetValue(sampleKey, out sample))
                {
                    Console.WriteLine("About to stop sample with key: " + sampleKey);
                }

                sample.Stop(); //.Paused = !sample.Paused;

                soundEngine.Update();

                if ( IsSamplePlaying(sampleKey) )
                {
                    Console.WriteLine("Sample is still fucking playing!");
                }
                else
                {
                    Console.WriteLine("SAMPLE HAS FINALLY STOPPED PLAYING!");
                }

                


            }
            catch (Exception e)
            {
                Console.WriteLine("BAD THINGS HAPPENED IN StopPlayingSample: " + e.Message);
                Console.WriteLine(e.InnerException);
            }
        }*/

        // Method to stop any playing sounds on the static soundEngine instance
        public static void StopAllSounds()
        {
            Console.WriteLine("Stopping all sounds!");
            soundEngine.StopAllSounds();

            
        }

        public static void PauseAllSounds(bool pauseState)
        {
            soundEngine.SetAllSoundsPaused(pauseState);
        }

        // Method to pause a currently playing sample
        public static void PauseSample(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
                if (!sample.Paused)
                {
                    Console.WriteLine("Found non-paused sample - pausing: " + sampleKey);
                    sample.Paused = true;
                }
                else
                {
                    //Console.WriteLine("Skipping pause of already paused sample: " + sampleKey);
                }
            }
        }

        // Method to resume a currently paused sample
        public static void ResumeSample(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
                if (sample.Paused)
                {
                    Console.WriteLine("Actually flipping paused flag of sample: " + sampleKey);
                    sample.Paused = false;
                }
            }
        }

        // Method to check if a given sample is paused or not
        public static bool IsPaused(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {   
                Console.WriteLine("Found sample " + sampleKey + " and returning paused state of: " + sample.Paused);
                return sample.Paused;
            }
            
            Console.WriteLine("Could not find sample to determine paused state: " + sampleKey);
            return true;
            
        }

        // Method to change the volume of a given sample
        public static void ChangeSampleVolume(string sampleKey, float volume)
        {
            // If we're playing the sample...
            if (soundEngine.IsCurrentlyPlaying(sampleKey))
            {
                // ...get access to it. If successfully found, clamp the requested volume between zero and one and then set the new volume.
                ISound sample = sampleDictionary[sampleKey];
                if (volume < 0.0f) { volume = 0.0f; }
                if (volume > 1.0f) { volume = 1.0f; }
                sample.Volume = volume;
            }
        }

        // Method to change the playback speed of a given sample
        public static void ChangeSampleSpeed(string sampleKey, float speed)
        {
            // If we're playing the sample...
            if (soundEngine.IsCurrentlyPlaying(sampleKey))
            {
                // ...get access to it. If successfully found, modify its speed.
                ISound sample = sampleDictionary[sampleKey];
                sample.PlaybackSpeed = speed;
            }
        }

        // Method to check if any sample in the dictionary is currently playing
        public static bool IsAnythingPlaying()
        {
            try
            {
                foreach (KeyValuePair<string, ISound> samplePair in sampleDictionary)
                {
                    ISound sample;
                    if (sampleDictionary.TryGetValue(samplePair.Key, out sample) && sample != null)
                    {
                        if (soundEngine.IsCurrentlyPlaying(samplePair.Key) && !sample.Paused)
                        {
                            //Console.WriteLine("IsAnythingPlaying returns true for sample: " + sample.Key);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ResourceManager.GetString("isAnythingPlayingExceptionString") + e.Message);
            }
            //Console.WriteLine("IsAnythingPlaying returns false!!!!!!!!!!!!!!!!!!");
            return false;
        }

        // Method to check if any sample in the dictionary is currently playing
        public static bool IsThisNormalInGameTriggerPlaying(Trigger t)
        {
            if (soundEngine.IsCurrentlyPlaying(t.sampleKey))
            {
                return true;
            }
            return false;
        }

        // Method to determine if we're current playing a normal InGame trigger (if we are, we'll queue any normal InGame triggers that match their trigger conditions)
        // NOTE: Paused samples count as playing - be careful of this!
        public static bool PlayingQueueableTrigger(List<Trigger> queueableTriggerList)
        {
            foreach (Trigger t in queueableTriggerList)
            {
                if (Program.gameState == Program.GameState.InGame && SoundPlayer.CurrentlyPlaying(t.sampleKey))
                {
                    //Console.WriteLine(DateTime.Now + " ******************************** Found playing sample: " + t.sampleFilename);
                    return true;
                }
            }
            return false;
        }
            

        // Method to check if a given sample currently playing.
        // WARNING: Paused samples count as playing!
        public static bool IsSamplePlaying(string sampleKey)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
                if (soundEngine.IsCurrentlyPlaying(sampleKey) && !sample.Paused)
                {
                    return true;
                }
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
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
                // Set volume and pitch then play the sample
                sample.Volume = volume;
                sample.PlaybackSpeed = pitch;

                // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
                sample = soundEngine.Play2D(sampleKey, loopSample, false, StreamMode.AutoDetect, true);
            }            
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public static void PlayQueueableSample(string sampleKey, float volume, float pitch, bool loopSample)
        {
            ISound sample;
            if (sampleDictionary.TryGetValue(sampleKey, out sample) && sample != null)
            {
               

                // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
                sample = soundEngine.Play2D(sampleKey, loopSample, false, StreamMode.AutoDetect, true);

                // Set volume and pitch then play the sample
                sample.Volume = volume;
                sample.PlaybackSpeed = pitch;

                sampleDictionary[sampleKey] = sample;

                //sample.setSoundStopEventReceiver(au.edu.federation.SoniFight.SoundPlayer, "foo!");

                readyToPlayNextQueuedSample = false;
            }
        }

        // Method to unload all samples and clear the sample dictionary
        public static void UnloadAllSamples()
        {
            soundEngine.StopAllSounds();

            // Release all samples in the dictionary then clear it
            /*foreach (KeyValuePair<string, ISound> sample in sampleDictionary)
            {
                Console.WriteLine("Releasing sample: " + sample.Key);
                sample.Value.Dispose();
            }*/
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