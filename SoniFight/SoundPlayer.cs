using System;
using System.Collections.Generic;
using IrrKlang;

using au.edu.federation.SoniFight.Properties;

namespace au.edu.federation.SoniFight
{
    // Class used to play and manipulate sonification events
    public class SoundPlayer : ISoundStopEventReceiver
    {
        // We'll build up a dictionary (i.e. list of key-value pairs) mapping the sample filename to the actual loaded sample
        // which can be used by any given GameConfig        
        private Dictionary<string, ISound> sampleDictionary = new Dictionary<string, ISound>();

        // We also keep track of all samples which are currently playing. When we play them we add them to this list, when they
        // stop they are removed from this list by the OnSoundStopped method (which is part of the ISoundStopEventReceiver interface).
        private Queue<Trigger> playQueue = new Queue<Trigger>();

        // The sound engine for menu samples
        private ISoundEngine menuEngine;
        public ISoundEngine MenuEngine
        {
            get { return menuEngine;  }
            set { menuEngine = value; }
        }

        // The sound engine for normal samples (these may have AllowanceTypes of InGame or Any)
        private ISoundEngine normalEngine;
        public ISoundEngine NormalEngine
        {
            get { return normalEngine;  }
            set { normalEngine = value; }
        }

        // The sound engine for continuously playing samples
        private ISoundEngine continuousEngine;
        public ISoundEngine ContinuousEngine
        {
            get { return continuousEngine;  }
            set { continuousEngine = value; }
        }

        // Flag to keep track of whether we're currently playing any continuous samples
        private bool playingContinuousSamples = false;
        public bool PlayingContinuousSamples
        {
            get { return playingContinuousSamples;  }
            set { playingContinuousSamples = value; }
        }

        private bool playingNormalSample = false;
        public bool PlayingNormalSample
        {
            get { return playingNormalSample;  }
            set { playingNormalSample = value; }
        }

        // Constructor
        public SoundPlayer()
        {
            menuEngine = new ISoundEngine();
            normalEngine = new ISoundEngine();
            continuousEngine = new ISoundEngine();
        }        

        // OnSoundStopped handler method - this is used for triggers with a type of Normal and an allowance type of InGame or ANy ONLY -
        // other types of triggers do not get assigned this OnSoundStopped handler.
        void ISoundStopEventReceiver.OnSoundStopped(ISound sound, StopEventCause reason, object userData)
        {
            Console.WriteLine("Sound: " + sound.ToString() + " stopped for reason: " + reason + " userdata tostring: " + userData.ToString() );

            // Set the flag that we are not playing a normal sample
            PlayingNormalSample = false;
        }

        // Method to load a sample and specify whether it should loop or not when played
        public bool LoadSample(Trigger t)
        {
            if (t.triggerType == Trigger.TriggerType.Modifier)
            {
                Console.WriteLine("Modifier triggers do not use samples - they modify the samples of continuous triggers.");
                return false;
            }

            if (!t.active)
            {
                Console.WriteLine("Skipping inactive trigger: " + t.id + " - " + t.sampleFilename);
                return false;
            }

            if (t.useTolk)
            {
                Console.WriteLine("Skipping tolk trigger: " + t.id + " - " + t.sampleFilename);
                return false;
            }

            // Print just the sample name being loaded.
            //NOTE: The key itself is of the form: ".\Configs\CONFIG_FOLDER_NAME\SAMPLE_NAME.FILE_EXTENSION"
            int index = t.sampleKey.LastIndexOf("\\") + 1;
            string shortKey = t.sampleKey.Substring(index, t.sampleKey.Length - index);

            ISound sound = null;

            // Load the sound by playing it, but stop it from playing immediately.
            // Params: sample filename, loop, start paused, stream mode, enable sound effects

            if (t.triggerType == Trigger.TriggerType.Continuous)
            {
                Console.WriteLine("Loading continous sample: " + shortKey);

                // Params: key, loop, start paused, stream mode, allow effects
                sound = continuousEngine.Play2D(t.sampleKey, true, true, StreamMode.AutoDetect, true);
                continuousEngine.StopAllSounds();
            }
            else if (t.triggerType == Trigger.TriggerType.Normal)
            {
                if (t.allowanceType == Trigger.AllowanceType.InMenu)
                {
                    Console.WriteLine("Loading menu sample: " + shortKey);
                    sound = menuEngine.Play2D(t.sampleKey, false, true, StreamMode.AutoDetect, true);
                    menuEngine.StopAllSounds();
                }
                else // Allowance is InGame or Any
                {
                    Console.WriteLine("Loading normal sample: " + shortKey);
                    sound = normalEngine.Play2D(t.sampleKey, false, true, StreamMode.AutoDetect, true);
                    normalEngine.StopAllSounds();
                    //sound.setSoundStopEventReceiver(this, t.sampleKey);
                }
            }

            // Otherwise add to our sample list, increment the SAM and return true for success
            if (!sampleDictionary.ContainsKey(t.sampleKey))
            {
                sampleDictionary.Add(t.sampleKey, sound);
            }
           
            return true;
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayMenuSample(Trigger t)
        {
            // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
            ISound sound = menuEngine.Play2D(t.sampleKey, false, false, StreamMode.AutoDetect, true);

            if (sound != null)
            {
                // Set volume and pitch according to trigger
                sound.Volume = t.sampleVolume;
                sound.PlaybackSpeed = t.sampleSpeed;
            }
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayNormalSample(Trigger t)
        {
            // If we're already playing a normal trigger sample...
            if (PlayingNormalSample)
            {
                // ...then we enqueue this one for when the currently playing one has stopped.
                playQueue.Enqueue(t);
                Console.WriteLine(  Resources.ResourceManager.GetString("queuingTriggerString") +
                                    Resources.ResourceManager.GetString("inGameSampleString") + t.sampleFilename);
            }
            else // If we're not playing a normal trigger sample then we can play this one right away.
            {
                Console.WriteLine(  Resources.ResourceManager.GetString("playingTriggerString") + 
                                    Resources.ResourceManager.GetString("inGameSampleString") + t.sampleFilename +
                                    Resources.ResourceManager.GetString("triggerIdString") + t.id +
                                    Resources.ResourceManager.GetString("volumeString") + t.sampleVolume +
                                    Resources.ResourceManager.GetString("speedString") + t.sampleSpeed);

                // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
                ISound sound = normalEngine.Play2D(t.sampleKey, false, false, StreamMode.AutoDetect, true);

                if (sound != null)
                {
                    // Set volume and pitch according to trigger
                    sound.Volume = t.sampleVolume;
                    sound.PlaybackSpeed = t.sampleSpeed;

                    // Set sound stop event listener
                    sound.setSoundStopEventReceiver(this, t.sampleKey);

                    // Update the sound in the sample dictionary to be this specific sound instance
                    sampleDictionary[t.sampleKey] = sound;

                    PlayingNormalSample = true;

                    //normalSamplePlaying = t.sampleKey;
                }
                else
                {
                    Console.WriteLine( Resources.ResourceManager.GetString("normalSampleNullSoundWarningString") + t.sampleFilename);
                }

            } // End of if we're not already playing a normal trigger sample block

        } // End of method

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayQueuedNormalSample()
        {
            if (playQueue.Count > 0)
            {
                Trigger t = playQueue.Dequeue();

                Console.WriteLine("PLAYING QUEUED!!!! " + Resources.ResourceManager.GetString("inGameSampleString") + t.sampleFilename +
                                      Resources.ResourceManager.GetString("triggerIdString") + t.id +
                                      Resources.ResourceManager.GetString("volumeString") + t.sampleVolume +
                                      Resources.ResourceManager.GetString("speedString") + t.sampleSpeed);

                // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
                ISound sound = normalEngine.Play2D(t.sampleKey, false, false, StreamMode.AutoDetect, true);

                if (sound != null)
                {
                    // Set volume and pitch according to trigger
                    sound.Volume = t.sampleVolume;
                    sound.PlaybackSpeed = t.sampleSpeed;

                    // Set sound stop event listener
                    sound.setSoundStopEventReceiver(this, t.sampleKey);

                    // Update the sound in the sample dictionary to be this specific sound instance
                    sampleDictionary[t.sampleKey] = sound;

                    PlayingNormalSample = true;

                    //normalSamplePlaying = t.sampleKey;
                }
                else
                {
                    Console.WriteLine("Playing queued normal sample gave us a null sound for: " + t.sampleFilename);
                }
            }
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayContinuousSample(Trigger t)
        {
            // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
            ISound sound = continuousEngine.Play2D(t.sampleKey, true, false, StreamMode.AutoDetect, true);

            if (sound != null)
            {
                // Set volume and pitch according to trigger
                sound.Volume = t.sampleVolume;
                sound.PlaybackSpeed = t.sampleSpeed;

                // Set sound stop event listener
                //sound.setSoundStopEventReceiver(this, t.sampleKey);

                // Update the sound in the sample dictionary to be this specific sound instance
                sampleDictionary[t.sampleKey] = sound;

                playingContinuousSamples = true;
            }
        }

        // Method to stop any playing sounds on the static soundEngine instance
        public void StopMenuSounds()
        {   
            menuEngine.StopAllSounds();
        }

        public void PauseNormalSound()
        {   
            normalEngine.SetAllSoundsPaused(true);
        }

        public void ResumeNormalSound()
        {
            normalEngine.SetAllSoundsPaused(false);
        }


        // Method to pause a currently playing sample
        public void PauseSample(string sampleKey)
        {
            /*ISoundSource sample = soundEngine.pa.GetSoundSource(sampleKey);

            sample.*/

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
        public void ResumeSample(string sampleKey)
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
        public bool IsPaused(string sampleKey)
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
        public void ChangeContinuousSampleVolume(string sampleKey, float volume)
        {
            ISound sound = null;
            if (sampleDictionary.TryGetValue(sampleKey, out sound) && sound != null)
            {
                if (volume < 0.0f) { volume = 0.0f; }
                if (volume > 1.0f) { volume = 1.0f; }
                sound.Volume = volume;
            }            
        }

        // Method to change the playback speed of a given sample
        public void ChangeContinuousSampleSpeed(string sampleKey, float speed)
        {
            // If we're playing the sample...
            
                // ...get access to it. If successfully found, modify its speed.
                ISound sound = sampleDictionary[sampleKey];
                sound.PlaybackSpeed = speed;
        }

        // Method to update the SoundEngine
        public void UpdateEngines()
        {
            // We must call the Update method on the SoundEngine several times per second for everything to run smoothly, especially
            // when moving sounds around. As such, this is called from the main loop 1000 / POLL_SLEEP_MS times per second.
            menuEngine.Update();
            normalEngine.Update();
            continuousEngine.Update();
        }

        // Method to unload all samples and clear the sample dictionary
        public void UnloadAllSamples()
        {
            menuEngine.StopAllSounds();
            normalEngine.StopAllSounds();
            continuousEngine.StopAllSounds();
            
            sampleDictionary.Clear();
            playQueue.Clear();

            // Unload all samples and force garbage collection
            menuEngine.RemoveAllSoundSources();
            normalEngine.RemoveAllSoundSources();
            continuousEngine.RemoveAllSoundSources();
        }

        // Method to unload all samples, dispose of the engines and perform garbage collection
        public void ShutDown()
        {
            UnloadAllSamples();

            menuEngine.Dispose();
            normalEngine.Dispose();
            continuousEngine.Dispose();

            System.GC.Collect();
        }

    } // End of SoundPlayer class

} // End of namespace