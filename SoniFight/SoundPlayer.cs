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
            //Trigger t = (Trigger)userData;
            //Console.WriteLine( Resources.ResourceManager.GetString("triggerSampleStoppedString") + t.id + " - " + t.sampleFilename);

            // Set the flag that we are not playing a normal sample
            PlayingNormalSample = false;
        }

        // Method to load a sample and specify whether it should loop or not when played
        public bool LoadSample(Trigger t)
        {
            // We allow triggers without sample because they can be used as dependent triggers
            if ( string.IsNullOrEmpty( t.SampleFilename.Trim() ) )
            {
                return false;
            }

            // Modifier triggers don't use samples, so if we're trying to load a sample for one it's an error and we'll inform the user
            if (t.triggerType == Trigger.TriggerType.Modifier)
            {
                Resources.ResourceManager.GetString("loadModifierSampleWarningString");
                return false;
            }

            // Non-active trigger? Then we don't need to load the sample for it.
            if (!t.Active)
            {
                Console.WriteLine(Resources.ResourceManager.GetString("skippingInactiveTriggerSampleString") + t.Id + " - " + t.SampleFilename);
                return false;
            }

            // Trigger uses tolk? Then we don't need to load a sample for it.
            if (t.UseTolk)
            {
                Console.WriteLine(Resources.ResourceManager.GetString("skippingTolkSampleLoadString") + t.Id + " - " + t.SampleFilename);
                return false;
            }

            // Print just the sample name being loaded.
            //NOTE: The key itself is of the form: ".\Configs\CONFIG_FOLDER_NAME\SAMPLE_NAME.FILE_EXTENSION"
            int index = t.SampleKey.LastIndexOf("\\") + 1;
            string shortKey = t.SampleKey.Substring(index, t.SampleKey.Length - index);

            ISound sound = null;

            // Load the sound by playing it, but stop it from playing immediately.
            // Params: sample filename, loop, start paused, stream mode, enable sound effects

            if (t.triggerType == Trigger.TriggerType.Continuous)
            {
                Console.WriteLine(Resources.ResourceManager.GetString("loadingContinuousSampleString") + shortKey);

                // Generate ISound for sample on continuous engine then stop all normal sounds
                // Params: key, loop, start paused, stream mode, allow effects
                sound = continuousEngine.Play2D(t.SampleKey, true, true, StreamMode.AutoDetect, true);
                continuousEngine.StopAllSounds();
            }
            else if (t.triggerType == Trigger.TriggerType.Normal)
            {
                if (t.allowanceType == Trigger.AllowanceType.InMenu)
                {
                    Console.WriteLine(Resources.ResourceManager.GetString("loadingMenuSampleString") + shortKey);

                    // Generate ISound for sample on menu engine then stop all menu sounds
                    // Params: key, loop, start paused, stream mode, allow effects
                    sound = menuEngine.Play2D(t.SampleKey, false, true, StreamMode.AutoDetect, true);
                    menuEngine.StopAllSounds();
                }
                else // Allowance is InGame or Any
                {
                    Console.WriteLine(Resources.ResourceManager.GetString("loadingNormalSampleString") + shortKey);

                    // Generate ISound for sample on normal engine then stop all normal sounds
                    // Params: key, loop, start paused, stream mode, allow effects
                    sound = normalEngine.Play2D(t.SampleKey, false, true, StreamMode.AutoDetect, true);
                    normalEngine.StopAllSounds();
                }
            }

            // If the sample dictionary doesn't already contain the key then add it
            if (!sampleDictionary.ContainsKey(t.SampleKey))
            {
                sampleDictionary.Add(t.SampleKey, sound);
            }
           
            return true;
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayMenuSample(Trigger t)
        {
            // Play the sample on the menu engine, generating the ISound object for it in the process
            // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
            ISound sound = menuEngine.Play2D(t.SampleKey, false, false, StreamMode.AutoDetect, true);

            if (sound != null)
            {
                // Print some debug useful for fine-tuning configs
                Console.WriteLine(Resources.ResourceManager.GetString("inMenuSampleString") + t.SampleFilename +
                                  Resources.ResourceManager.GetString("triggerIdString") + t.Id +
                                  Resources.ResourceManager.GetString("volumeString") + t.SampleVolume +
                                  Resources.ResourceManager.GetString("speedString") + t.SampleSpeed);

                // Set volume and pitch according to trigger
                sound.Volume = t.SampleVolume * MainForm.gameConfig.NormalTriggerMasterVolume;
                sound.PlaybackSpeed = t.SampleSpeed;
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
                                    Resources.ResourceManager.GetString("inGameSampleString") + t.SampleFilename);
            }
            else // If we're not playing a normal trigger sample then we can play this one right away.
            {
                Console.WriteLine(Resources.ResourceManager.GetString("playingTriggerString") + 
                                  Resources.ResourceManager.GetString("inGameSampleString") + t.SampleFilename +
                                  Resources.ResourceManager.GetString("triggerIdString") + t.Id +
                                  Resources.ResourceManager.GetString("volumeString") + t.SampleVolume +
                                  Resources.ResourceManager.GetString("speedString") + t.SampleSpeed);

                // Play the sample on the normal engine, generating the ISound object for it in the process
                // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
                ISound sound = normalEngine.Play2D(t.SampleKey, false, false, StreamMode.AutoDetect, true);

                if (sound != null)
                {
                    // Set volume and pitch according to trigger
                    sound.Volume = t.SampleVolume * MainForm.gameConfig.NormalTriggerMasterVolume;
                    sound.PlaybackSpeed = t.SampleSpeed;

                    // Set sound stop event listener
                    sound.setSoundStopEventReceiver(this, t);

                    // Update the sound in the sample dictionary to be this specific sound instance
                    sampleDictionary[t.SampleKey] = sound;

                    // Set our flag to say we're not playing a normal sample
                    PlayingNormalSample = true;
                }
                else
                {
                    Console.WriteLine( Resources.ResourceManager.GetString("normalSampleNullSoundWarningString") + t.SampleFilename);
                }

            } // End of if we're not already playing a normal trigger sample block

        } // End of method

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayQueuedNormalSample()
        {
            if (playQueue.Count > 0)
            {
                // Get the trigger at the front of the queue
                Trigger t = playQueue.Dequeue();

                // Provide some debug output
                Console.WriteLine(Resources.ResourceManager.GetString("playingQueuedTriggerString") + t.SampleFilename +
                                  Resources.ResourceManager.GetString("triggerIdString") + t.Id +
                                  Resources.ResourceManager.GetString("volumeString") + t.SampleVolume +
                                  Resources.ResourceManager.GetString("speedString") + t.SampleSpeed);

                // Play the sample on the normal engine, generating the ISound object for it in the process
                // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
                ISound sound = normalEngine.Play2D(t.SampleKey, false, false, StreamMode.AutoDetect, true);

                if (sound != null)
                {
                    // Set volume and pitch according to trigger
                    sound.Volume = t.SampleVolume * MainForm.gameConfig.NormalTriggerMasterVolume;
                    sound.PlaybackSpeed = t.SampleSpeed;

                    // Set sound stop event listener
                    sound.setSoundStopEventReceiver(this, t);

                    // Update the sound in the sample dictionary to be this specific sound instance
                    sampleDictionary[t.SampleKey] = sound;

                    // Set our flag to say we're not playing a normal sample
                    PlayingNormalSample = true;
                }
                else
                {
                    Console.WriteLine( Resources.ResourceManager.GetString("nullSoundInQueuedSampleWarningString") + t.SampleFilename);
                }
            }
        }

        // Method to play the sample identified by the key at the volume and pitch provided
        public void PlayContinuousSample(Trigger t)
        {
            // Params: Sample, play looped, start-paused, stream-mode, enable-sound-effects
            ISound sound = continuousEngine.Play2D(t.SampleKey, true, false, StreamMode.AutoDetect, true);

            if (sound != null)
            {
                // Set volume and pitch according to trigger
                sound.Volume = t.SampleVolume * MainForm.gameConfig.ContinuousTriggerMasterVolume;
                sound.PlaybackSpeed = t.SampleSpeed;

                // Update the sound in the sample dictionary to be this specific sound instance
                sampleDictionary[t.SampleKey] = sound;

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
            ISound sound = null;
            if (sampleDictionary.TryGetValue(sampleKey, out sound) && sound != null)
            {
                if (speed < GameConfig.MIN_SAMPLE_PLAYBACK_SPEED) { speed = GameConfig.MIN_SAMPLE_PLAYBACK_SPEED; }
                if (speed > GameConfig.MAX_SAMPLE_PLAYBACK_SPEED) { speed = GameConfig.MAX_SAMPLE_PLAYBACK_SPEED; }
                sound.PlaybackSpeed = speed;
            }
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