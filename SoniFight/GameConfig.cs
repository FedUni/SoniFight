using au.edu.federation.SoniFight.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace au.edu.federation.SoniFight
{
    public class GameConfig
    {
        // Constants are implied static in C# - you cannot mark them as such.
        public const int MAX_STRING_LENGTH = 150;

        // Valid ranges of how often to poll in milliseconds. A decent value might be 10ms, so we poll 100 times per second.
        public const int MIN_POLL_SLEEP_MS = 1;
        public const int MAX_POLL_SLEEP_MS = 200;

        // Valid ranges of how long betweeen ticks of the clock in the game - in milliseconds. This is used to determine if we're 'InGame' or 'InMenu'.
        public const int MIN_CLOCK_TICK_MS = 100;
        public const int MAX_CLOCK_TICK_MS = 3000;

        // Valid ranges of how loud to play a sample
        public const float MIN_SAMPLE_VOLUME = 0.0f;
        public const float MAX_SAMPLE_VOLUME = 1.0f;

        // Valid ranges of how fast to play the sample (1.0f is 'normal speed')
        public const float MIN_SAMPLE_PLAYBACK_SPEED = 0.1f;
        public const float MAX_SAMPLE_PLAYBACK_SPEED = 4.0f;

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
            get { return processName; }
            set { processName = value; }
        }

        // How long to sleep before polling watches in milliseconds (recommend: 1ms? 0 makes the CPU busy wait like crazy)
        private int pollSleepMS = 100;
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

        // The maximum value for the clock in a given game. Default: 99
        // Used to stop SoniFight from switching to InGame mode between rounds.
        private int clockMax = 99;
        public int ClockMax
        {
            get { return clockMax; }
            set { clockMax = value; }
        }

        // GameConfigs for fighting games will typically use a clock trigger to determine the in-game vs in-menu state. However, 
        // if sonifying other games like Doom or such for low ammo or health then there's no clock.
        [XmlIgnore]
        private int clockTriggerId = -1;
        [XmlIgnore]
        public int ClockTriggerId
        {
            get { return clockTriggerId; }
            set { clockTriggerId = value; }
        }

        // Lists of watches - these may be of various types
        public List<Watch> watchList = new List<Watch>();

        // List of triggers (all triggers to be saved)
        public List<Trigger> triggerList = new List<Trigger>();

        // Trigger list broken up into categories for looping efficiency
        [XmlIgnore]
        public List<Trigger> menuTriggerList = new List<Trigger>();
        [XmlIgnore]
        public List<Trigger> normalTriggerList = new List<Trigger>();
        [XmlIgnore]
        public List<Trigger> continuousTriggerList = new List<Trigger>();
        [XmlIgnore]
        public List<Trigger> modifierTriggerList = new List<Trigger>();

        // The actual process attached to
        [XmlIgnore]
        private Process gameProcess;

        // The handle to the process we are connected to
        [XmlIgnore]
        private IntPtr processHandle;
        [XmlIgnore]
        public IntPtr ProcessHandle
        {
            get { return processHandle; }
            set { processHandle = value; }
        }

        // The base address of the process (this will likely change per run due to Address Space Layout Randomisation (ASLR))
        [XmlIgnore]
        private IntPtr processBaseAddress;
        [XmlIgnore]
        public IntPtr ProcessBaseAddress
        {
            get { return processBaseAddress; }
            set { processBaseAddress = value; }
        }

        // We must validate the gameconfig to activate it - this occurs via the validate method.
        [XmlIgnore]
        public bool valid = false;
        [XmlIgnore]
        public bool active = false;

        // The name of the directory containing this GameConfig object and its associated samples
        private string configDirectory;
        public string ConfigDirectory
        {
            get { return configDirectory; }
            set { configDirectory = value; }
        }

        // Blank constructor req'd for XML serialisation
        public GameConfig()
        {
            processConnectionBGW.WorkerSupportsCancellation = true;
        }

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
        public static BackgroundWorker processConnectionBGW = new BackgroundWorker();

        [XmlIgnore]
        private Process[] processArray = null;

        // DoWork method for the process connection background worker
        public void connectToProcess(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // Not connected and we're not cancelling? Then using the background worker...
            while (!Program.connectedToProcess && !processConnectionBGW.CancellationPending)
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
                    processHandle = gameProcess.Handle;

                    // Get the process base address
                    processBaseAddress = Utils.findProcessBaseAddress(processName);
                    if (processBaseAddress == (IntPtr)0)
                    {
                        string s1 = Resources.ResourceManager.GetString("processNotFoundWarningString1");
                        string s2 = Resources.ResourceManager.GetString("processNotFoundWarningString2");
                        MessageBox.Show(s1 + processName + s2);
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine(Resources.ResourceManager.GetString("foundProcessBaseAddressString") + MainForm.gameConfig.ProcessBaseAddress);
                    }

                    // Calculate initial destination addresses.
                    // Note: These get re-calculated per iteration, but these are the initial values.
                    // TODO: Remove this and see if it still works!
                    for (int watchLoop = 0; watchLoop < watchList.Count; watchLoop++)
                    {
                        watchList[watchLoop].DestinationAddress = Utils.findFeatureAddress(processHandle, processBaseAddress, watchList[watchLoop].PointerList);
                    }

                    // Get configDirectory in correct state (relative path ending with a backslash)
                    if (!configDirectory.EndsWith("\\"))
                    {
                        configDirectory += "\\";
                    }

                    // Load all samples
                    foreach (Trigger t in triggerList)
                    {
                        // Construct the sample key used for the dictionary
                        t.SampleKey = ".\\Configs\\" + configDirectory + t.SampleFilename;

                        // If the sample isn't loaded and it's not the clock or a modifier trigger (these don't use samples) and we're not using tolk...
                        //if ( !(Program.irrKlang.SampleLoaded(t.sampleKey)) && !(t.IsClock) && (t.triggerType != Trigger.TriggerType.Modifier) && !(t.useTolk) )
                        if (!(t.IsClock) && (t.triggerType != Trigger.TriggerType.Modifier) && !(t.UseTolk))
                        {
                            // ...then load the sample for the trigger.
                            // NOTE: The sample is loaded into the specific engine used for playback based on the trigger type
                            Program.irrKlang.LoadSample(t);
                        }

                        // Only add active triggers to these separated lists, and don't add the clock
                        if (t.Active && !t.IsClock)
                        {
                            if (t.triggerType == Trigger.TriggerType.Normal)
                            {
                                // This list contains ALL the normal triggers, such as those used for menus as well as those using tolk for sonification
                                normalTriggerList.Add(t); 
                            }
                            else if (t.triggerType == Trigger.TriggerType.Continuous)
                            {
                                continuousTriggerList.Add(t);
                            }                            
                            else // if (t.triggerType == Trigger.TriggerType.Modifier)
                            {
                                modifierTriggerList.Add(t);
                            }
                        }
                    }

                    // Set our process grabbing background worker to cancel
                    e.Cancel = true;

                    // This sets cancellation to pending, which we handle in the associated doWork method
                    // to actually perform the cancellation.
                    processConnectionBGW.CancelAsync();
                }

                // Only poll in our background worker twice per second
                Thread.Sleep(500);
            }
        }

        // Method to activate this GameConfig
        public bool activate()
        {
            Console.WriteLine("Attempting to connect to process: " + processName);

            Program.connectedToProcess = false;

            // Set up the background worker to connect to our game process without freezing the UI and kick it off.
            processConnectionBGW = new BackgroundWorker();
            processConnectionBGW.DoWork += connectToProcess;
            processConnectionBGW.WorkerReportsProgress = false;
            processConnectionBGW.WorkerSupportsCancellation = true;
            processConnectionBGW.RunWorkerAsync();

            return true;
        }

        // Method to validate this GameConfig
        public bool validate()
        {
            string s;

            // Check we have a config directory
            if (String.IsNullOrEmpty(ConfigDirectory))
            {   
                MessageBox.Show( Resources.ResourceManager.GetString("configDirCannotBeBlankString") );
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

            // Ensure clockTickMS is an int within the valid range
            s = ClockTickMS.ToString();
            if (!string.IsNullOrEmpty(s))
            {
                // Have a value? Great - try to parse it to an int
                int i;
                if (Int32.TryParse(s, out i))
                {
                    // If we're here we got an int - now we need to check if it's within the valid range
                    if (i < GameConfig.MIN_CLOCK_TICK_MS || i > GameConfig.MAX_CLOCK_TICK_MS)
                    {
                        MessageBox.Show("Validation Error: Clock tick must be an integer between " + GameConfig.MIN_CLOCK_TICK_MS + " and " + GameConfig.MAX_CLOCK_TICK_MS + " milliseconds. If unknown just guess 1000.");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Validation Error: Could not parse clock tick value " + s + " to int.");
                    return false;
                }
            }
            else // Null or empty poll sleep value?
            {
                MessageBox.Show("Validation Error: Clock tick must be an integer between " + GameConfig.MIN_CLOCK_TICK_MS + " and " + GameConfig.MAX_CLOCK_TICK_MS + " milliseconds. If unknown just guess 1000.");
                return false;
            }

            // Ensure clockMax is an int within the valid range
            s = ClockMax.ToString();
            if (!string.IsNullOrEmpty(s))
            {
                // Have a value? Great - try to parse it to an int
                int i;
                if (Int32.TryParse(s, out i))
                {
                    // If we're here we got an int - now we need to check if it's within the valid range
                    if (i < 30 || i > 100)
                    {
                        MessageBox.Show("Validation Error: Clock max must be an integer between 30 and 100.");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Validation Error: Could not parse clock max value " + s + " to int.");
                    return false;
                }
            }
            else // Null or empty poll sleep value?
            {
                MessageBox.Show("Validation Error: Clock max must be an integer between 30 and 100.");
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
            if (idList.Count != idList.Distinct().Count())
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
            int clockTriggersFound = 0;
            List<int> clockTriggerIdList = new List<int>();
            foreach (Trigger t in triggerList)
            {
                idList.Add(t.Id);

                // Ensire trigger id is => 0
                if (t.Id < 0)
                {
                    MessageBox.Show("Validation Error: Triggers must have unique ids with a minumum value of 0.");
                    return false;
                }

                // Ensure sample filename exists
                if (!t.IsClock)
                {
                    if ( string.IsNullOrEmpty(t.SampleFilename) )
                    {
                        MessageBox.Show("Validation Error: A sample name must be provided to play trigger with Id: " + t.Id);
                        return false;
                    }
                }
                else // Found clock trigger?
                {
                    // If the clock trigger is active then track it so we can ensure we only have a single active clock trigger later on (no more, no less)
                    if (t.Active)
                    {
                        // Set this game config's clock trigger id so we can go straight to it rather than finding it per poll
                        clockTriggerId = t.Id;
                        clockTriggerIdList.Add(t.Id);
                        ++clockTriggersFound;
                    }
                }

                // Ensure sample volume is a float within the valid range (the clock trigger doesn't use a sample)
                s = t.SampleVolume.ToString();
                if (!t.IsClock)
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
                        MessageBox.Show("Validation Error: Missing sample volume on trigger with Id: " + t.Id);
                        return false;
                    }
                }

                // Ensure sample volume rate is a float within the valid range (the clock trigger doesn't use a sample)
                s = t.SampleSpeed.ToString();
                if (!t.IsClock)
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
                        MessageBox.Show("Validation Error: Missing sample speed on trigger with Id: " + t.Id);
                        return false;
                    }
                }

            } // End of loop over triggers

            // Moan if we have more than 1 clock trigger (but we will accept no clock triggers for games like Doom etc. which do not have a clock)
            if (clockTriggersFound > 1)
            {
                s = "Validation Error: There is more than a single active trigger marked isClock and we may only have one. Triggers marked isClock: ";
                for (int i = 0; i < clockTriggerIdList.Count; ++i)
                {
                    s += clockTriggerIdList[i].ToString();

                    if (i != (clockTriggerIdList.Count - 1))
                    {
                        s += ", ";
                    }
                }

                MessageBox.Show(s);
                return false;
            }

            // Ensure trigger ids are unique
            if (idList.Count != idList.Distinct().Count())
            {
                MessageBox.Show("Validation Error: Trigger Id values must be unique.");
                return false;
            }

            // Made it this far? Then indicate that we're hot to trot...            
            return true;
        }

        // Method to calculate the address of all features - called once per polling event
        public void calcAllWatchAddresses()
        {
            for (int watchLoop = 0; watchLoop < watchList.Count; ++watchLoop)
            {
                watchList[watchLoop].DestinationAddress = Utils.findFeatureAddress(ProcessHandle, processBaseAddress, watchList[watchLoop].PointerList);
            }
        }

    } // End of GameConfig class  

} // End of namespace