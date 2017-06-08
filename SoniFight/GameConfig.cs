using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SoniFight
{
    public class GameConfig
    {
        // Constants are implied static in C# - you cannot mark them as such.
        public const int MAX_STRING_LENGTH = 150;

        // Valid ranges of how often to poll in milliseconds. A decent value might be 10ms, so we poll 100 times per second.
        public const int MIN_POLL_SLEEP_MS = 1;
        public const int MAX_POLL_SLEEP_MS = 200;

        // Valid ranges of how loud to play a sample
        public const float MIN_SAMPLE_VOLUME = 0.0f;
        public const float MAX_SAMPLE_VOLUME = 1.0f;

        // Valid ranges of how fast to play the sample (1.0f is 'normal speed')
        public const float MIN_SAMPLE_PLAYBACK_SPEED = 0.5f;
        public const float MAX_SAMPLE_PLAYBACK_SPEED = 4.0f;

        private static DateTime lastProcessConnectionCheckTime = DateTime.Now;

        // Description of this config
        private string description = "GameConfig description";
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        // The name of the process we're looking for
        private string processName = "Process to attach to - do not include the .exe extension";
        public string ProcessName
        {
            get { return processName;  }
            set { processName = value; }
        }

        // How long to sleep before polling watches in milliseconds (recommend: 1ms? 0 makes the CPU busy wait like crazy)
        private int pollSleepMS = 10;
        public int PollSleepMS
        {
            get { return pollSleepMS; }
            set { pollSleepMS = value; }
        }

        // How long each tick of the game clock takes in milliseconds. This is used to determine whether the clock has changed, which
        // informs us as to whether we're in a round (i.e. InGame) or not (InMenu)
        private int clockTickMS = 1000;
        public int ClockTickMS
        {
            get { return clockTickMS; }
            set { clockTickMS = value; }
        }

        // Lists of watches - these may be of various types
        public List<Watch> watchList = new List<Watch>();

        // List of triggers
        //private int currentWatchId = 0;
        public List<Trigger> triggerList = new List<Trigger>();

        // The actual process attached to
        [XmlIgnore]
        private Process gameProcess;

        // The handle to the process we are connected to
        [XmlIgnore]
        private int processHandle;
        [XmlIgnore]
        public int ProcessHandle
        {
            get { return processHandle; }
            set { processHandle = value; }
        }

        // The base address of the process (this will likely change per run due to Address Space Layout Randomisation (ASLR))
        [XmlIgnore]
        private int processBaseAddress;
        [XmlIgnore]
        public int ProcessBaseAddress
        {
            get { return processBaseAddress; }
            set { processBaseAddress = value; }
        }

        // We must validate the gameconfig to activate it - this occurs via the validate method.
        private bool valid = false;
        private bool active = false;

        // The name of the directory containing this GameConfig object and its associated samples
        private string configDirectory;
        public string ConfigDirectory
        {
            get { return configDirectory; }
            set { configDirectory = value; }
        }

        // Blank constructor req'd for XML serialisation
        public GameConfig() { }

        public void setDescription(string configDescription)
        {
            // Limit string description to 150 chars
            if (configDescription.Length > MAX_STRING_LENGTH) { description = configDescription.Substring(0, MAX_STRING_LENGTH); }
            else { description = configDescription; }
        }
        public void setProcessName(string procName) { processName = procName; }

        public void setPollSleepMS(int i) { pollSleepMS = i; }

        // Background worker so we can attempt to connect to a game process without locking up the UI
        [XmlIgnore]
        public static BackgroundWorker processConnectionBW = new BackgroundWorker();

        [XmlIgnore]
        private Process[] processArray = null;

        // DoWork method for the process connection background worker
        public void connectToProcess(object sender, System.ComponentModel.DoWorkEventArgs e)
        {   
            // Not connected and we're not cancelling? Then using the background worker...
            while (!Program.connectedToProcess && !processConnectionBW.CancellationPending)
            {
                // Find all instances of the named process running on the local computer.
                // This will return an empty array if the process isn't running.
                processArray = Process.GetProcessesByName(processName);

                // Not found? Indicate we're trying...
                if (processArray.Length < 1)
                {
                    Console.Write(".");
                }
                else // Found the process by name?
                {
                    // Flip the flag so we can stop trying to connect
                    Program.connectedToProcess = true;

                    // Get the process handle
                    gameProcess = processArray[0];
                    processHandle = (int)gameProcess.Handle;

                    // Get the process base address
                    processBaseAddress = Utils.findProcessBaseAddress(processName);
                    if (processBaseAddress == 0)
                    {
                        MessageBox.Show("Error: No process called " + processName + " found. Activation failed.");
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Found process base address at: " + MainForm.gameConfig.ProcessBaseAddress);
                    }



                    // Calculate initial destination addresses.
                    // Note: These get re-calculated per iteration, but these are the initial values.
                    // TODO: Remove this and see if it still works!
                    for (int watchLoop = 0; watchLoop < watchList.Count; watchLoop++)
                    {
                        watchList[watchLoop].DestinationAddress = Utils.findFeatureAddress(processHandle, processBaseAddress, watchList[watchLoop].PointerList);
                    }

                    // Set the triggered flag of all triggers to false
                    foreach (Trigger t in triggerList)
                    {
                        t.Triggered = false;

                        // Load sample if not already loaded - but only if a normal trigger and NOT the main clock trigger!
                        if (!SoundPlayer.SampleLoaded(t.sampleFilename) && t.controlType == Trigger.ControlType.Normal && !t.isClock)
                        {
                            SoundPlayer.LoadSample(ConfigDirectory, t.sampleFilename, t.allowanceType);
                        }
                    }


                    e.Cancel = true;

                    // This sets cancellation to pending, which we handle in the associated doWork method
                    // to actually perform the cancellation.
                    processConnectionBW.CancelAsync();
                }

                // Only poll in our background worker twice per second
                Thread.Sleep(500);
            }
        }

        public bool activate()
        {
            Process[] processArray = null;

            Console.WriteLine("Attempting to connect to process: " + processName);
            //Console.WriteLine("Press any key to abort.");

            // Set up the background worker to connect to our game process without freezing the UI and kick it off.
            processConnectionBW = new BackgroundWorker();
            processConnectionBW.DoWork += connectToProcess;
            processConnectionBW.WorkerReportsProgress = false;
            processConnectionBW.WorkerSupportsCancellation = true;
            processConnectionBW.RunWorkerAsync();

            return true;
        }

        public bool validate()
        {
            string s;

            // Check we have a config directory
            if (String.IsNullOrEmpty(ConfigDirectory))
            {
                MessageBox.Show("Validation Error: GameConfig directory cannot be blank.");
                return false;
            }

            // Ensure we have a process to locate
            if (String.IsNullOrEmpty(processName))
            {
                MessageBox.Show("Validation Error: GameConfig process name cannot be blank.");
                return false;
            }

            // Ensure pollSleepMS is an int within the valid range
            s = PollSleepMS.ToString();
            if (!string.IsNullOrEmpty(s))
            {
                // Have a value? Great - try to parse it to an int
                int i;
                if (Int32.TryParse(s, out i))
                {
                    // If we're here we got an int - now we need to check if it's within the valid range
                    if (i < GameConfig.MIN_POLL_SLEEP_MS || i > GameConfig.MAX_POLL_SLEEP_MS)
                    {
                        MessageBox.Show("Validation Error: Poll sleep must be an integer between " + GameConfig.MIN_POLL_SLEEP_MS + " and " + GameConfig.MAX_POLL_SLEEP_MS + " milliseconds.");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Validation Error: Could not parse poll sleep value " + s + " to int.");
                    return false;
                }
            }
            else // Null or empty poll sleep value?
            {
                MessageBox.Show("Validation Error: Poll sleep must be an integer between " + GameConfig.MIN_POLL_SLEEP_MS + " and " + GameConfig.MAX_POLL_SLEEP_MS + " milliseconds.");
                return false;
            }

            // Ensure we have at least one watch
            if (watchList.Count == 0)
            {
                MessageBox.Show("Validation Error: GameConfig does not have any watches.");
                return false;
            }

            // Ensure that each watch id is a unique value, no id is -1 (i.e. blank) and that each watch has a pointer trail which is not empty
            List<int> idList = new List<int>();
            foreach (Watch w in watchList)
            {
                idList.Add(w.Id);

                if (w.Id == -1)
                {
                    MessageBox.Show("Validation Error: Watch Id values must be greater than or equal to 0.");
                    return false;
                }

                if (w.PointerList.Count == 0)
                {
                    MessageBox.Show("Validation Error: Watch with id " + w.Id + " has an empty pointer trail.");
                    return false;
                }
                else // Have a pointer trail? Great - ensure it's valid
                {
                    int x;
                    foreach (string pointerValue in w.PointerList)
                    {
                        try
                        {
                            x = Convert.ToInt32(pointerValue, 16); // Convert from hex to int
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("Validation Error: Pointer value " + pointerValue + " in watch " + w.Id + " cannot be cast to int. Do not prefix pointer hops with 0x or such.");
                            return false;
                        }
                    }

                } // End of if PointerList.Count > 0 section

            } // End of loop over all watches in the watchList

            // Ensure watch ids are unique
            if (idList.Count != idList.Distinct().Count() )
            {
                MessageBox.Show("Validation Error: Watch Id values must be unique.");
                return false;
            }

            // Ensure we have at least one watch
            if (triggerList.Count == 0)
            {
                MessageBox.Show("Validation Error: GameConfig does not have any triggers.");
                return false;
            }

            // Got triggers? Great - let's make sure they're sane...
            idList.Clear();
            foreach (Trigger t in triggerList)
            {
                idList.Add(t.id);

                // Ensire trigger id is => 0
                if (t.id < 0)
                {
                    MessageBox.Show("Validation Error: Triggers must have unique ids with a minumum value of 0.");
                    return false;
                }

                // Ensure sample filename exists
                if (!t.isClock)
                {
                    if (string.IsNullOrEmpty(t.sampleFilename))
                    {
                        MessageBox.Show("Validation Error: A sample name must be provided to play trigger with Id: " + t.id);
                        return false;
                    }
                }

                // Ensure sample volume is a float within the valid range
                s = t.sampleVolume.ToString();
                if (!t.isClock)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        float f;
                        if (float.TryParse(s, out f))
                        {
                            // If we're here we got an int - now we need to check if it's within the valid range
                            if (f < MIN_SAMPLE_VOLUME || f > MAX_SAMPLE_VOLUME)
                            {
                                MessageBox.Show("Validation Error: Sample volume must be between " + MIN_SAMPLE_VOLUME + " and " + MAX_SAMPLE_VOLUME + " inclusive.");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Validation Error: Missing sample volume on trigger with Id: " + t.id);
                        return false;
                    }
                }

                // Ensure sample volume rate is a float within the valid range
                s = t.sampleSpeed.ToString();
                if (!t.isClock)
                {


                    if (!string.IsNullOrEmpty(s))
                    {
                        float f;
                        if (float.TryParse(s, out f))
                        {
                            // If we're here we got an int - now we need to check if it's within the valid range
                            if (f < MIN_SAMPLE_PLAYBACK_SPEED || f > MAX_SAMPLE_PLAYBACK_SPEED)
                            {
                                MessageBox.Show("Validation Error: Sample volume must be between " + MIN_SAMPLE_PLAYBACK_SPEED + " and " + MAX_SAMPLE_PLAYBACK_SPEED + " inclusive.");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Validation Error: Missing sample speed on trigger with Id: " + t.id);
                        return false;
                    }
                }

            } // End of loop over triggers

            // Ensure trigger ids are unique
            if (idList.Count != idList.Distinct().Count())
            {
                MessageBox.Show("Validation Error: Trigger Id values must be unique.");
                return false;
            }

            // Made it this far? Then indicate that we're hot to trot...            
            return true;
        }

        // This should be called if the process closes or the [Stop] button is clicked
        public void deactivateGameConfig()
        {
            valid = false;
        }

        // Method to calculate the address of all features - called once per polling event
        public void calcAllWatchAddresses()
        {
            for (int watchLoop = 0; watchLoop < watchList.Count; ++watchLoop)
            {
                watchList[watchLoop].DestinationAddress = Utils.findFeatureAddress(ProcessHandle, processBaseAddress, watchList[watchLoop].PointerList);
            }
        }

        // Method to reset all Triggered properties to false when a 'reset' trigger is matched (i.e. when clock is 99 or such).
        /*public void resetAllOnceTriggers()
        {
            // Set the triggered flag of all triggers to false
            foreach (Trigger t in triggerList)
            {
                if (t.triggerType == Trigger.TriggerType.Once)
                {
                    t.Triggered = false;
                }
            }
        }*/

        // Method to mute / stop all playing trigger samples
        /*public void disableAllOnceTriggers()
        {
            // Set all triggers which use the Once type to have their Triggered flag set to true so they can't activate until
            // reset has been called. This stops all health notifications being played between rounds as health for both players
            // goes to 0 before being reset to their starting health.
            foreach (Trigger t in triggerList)
            {
                if (t.triggerType == Trigger.TriggerType.Once)
                {
                    t.Triggered = true;
                }
            }

            //SoundPlayer.muteChannel();

            // Set the triggered flag of all triggers to true so they can't trigger again
            //foreach (Trigger t in triggerList)
            //{
            //    t.Triggered = true;
            //}

            // Stop any samples playing on the channel
            //SoundPlayer.StopChannel();
        }
        */

    } // End of GameConfig class  

} // End of namespace

