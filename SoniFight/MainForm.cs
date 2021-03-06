﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using au.edu.federation.SoniFight.Properties;
using DavyKager;

//using System.Diagnostics;
//using System.Text.RegularExpressions;

namespace au.edu.federation.SoniFight
{
    public partial class MainForm : Form
    {
        // Provide PInvoke signatures for methods to register and unregister hotkeys
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Used in window title
        private static string formTitle = "SoniFight "; // Do not localise this - SoniFight is SoniFight.

        // Are we current running a gameconfig? Initially no!
        bool running = false;

        // The GameConfig object represented by the UI
        public static GameConfig gameConfig;

        // String versions of the type of data watches can use
        string[] dataTypesArray = { Resources.ResourceManager.GetString("integerString"),      // "Integer"
                                    Resources.ResourceManager.GetString("shortString"),        // "Short"
                                    Resources.ResourceManager.GetString("longString"),         // "Long"
                                    Resources.ResourceManager.GetString("unsignedIntString"),  // "Unsigned Int"
                                    Resources.ResourceManager.GetString("floatString"),        // "Float"
                                    Resources.ResourceManager.GetString("doubleString"),       // "Double"
                                    Resources.ResourceManager.GetString("booleanString"),      // "Boolean"
                                    Resources.ResourceManager.GetString("stringUTF8String"),   // "String (UTF-8)"
                                    Resources.ResourceManager.GetString("stringUTF16String"),  // "String (UTF-16)"
                                    Resources.ResourceManager.GetString("byteString")          // "Byte"
                                  };

        // String versions of the comparison types we can use
        string[] comparisonTypesArray = { Resources.ResourceManager.GetString("equalToString"),                  // "Equal To"
                                          Resources.ResourceManager.GetString("lessThanString"),                 // "Less Than"
                                          Resources.ResourceManager.GetString("lessThanOrEqualToString"),        // "Less Than Or Equal To"
                                          Resources.ResourceManager.GetString("greaterThanString"),              // "Greater Than"
                                          Resources.ResourceManager.GetString("greaterThanOrEqualToString"),     // "Greater Than Or Equal To"
                                          Resources.ResourceManager.GetString("notEqualToString"),               // "Not Equal To"
                                          Resources.ResourceManager.GetString("changedString"),                  // "Changed"
                                          Resources.ResourceManager.GetString("increasedString"),                // "Increased"
                                          Resources.ResourceManager.GetString("decreasedString"),                // "Decreased"
                                          Resources.ResourceManager.GetString("distanceVolumeDescendingString"), // "Distance - Volume Descending (Cont. Only)"
                                          Resources.ResourceManager.GetString("distanceVolumeAscendingString"),  // "Distance - Volume Ascending (Cont. Only)"
                                          Resources.ResourceManager.GetString("distancePitchDescendingString"),  // "Distance - Pitch Descending (Cont. Only)"
                                          Resources.ResourceManager.GetString("distancePitchAscendingString")    // "Distance - Pitch Ascending (Cont. Only)"
                                        };

        // String versions of the trigger types
        string[] triggerTypesArray = { Resources.ResourceManager.GetString("triggerTypeNormalString"),     // "Normal",
                                       Resources.ResourceManager.GetString("triggerTypeDependentString"),  // "Dependent"
                                       Resources.ResourceManager.GetString("triggerTypeContinuousString"), // "Continuous"
                                       Resources.ResourceManager.GetString("triggerTypeModifierString")    // "Modifier"
                                     };

        // String versions of the trigger allowance types
        string[] allowanceTypesArray = { Resources.ResourceManager.GetString("allowanceTypeAnyString"),    // "Any"
                                         Resources.ResourceManager.GetString("allowanceTypeInGameString"), // "In-Game"
                                         Resources.ResourceManager.GetString("allowanceTypeInMenuString")  // "In-Menu"
                                       };

        // String versions of the hotkey target types
        string[] hotkeyTargetsArray = { "Watch", "Trigger" };

        // Initial config dropdown index
        static int selectedConfigDropdownIndex = 0;

        // Details panel padding for UI elements
        private Padding padding = new System.Windows.Forms.Padding(5);

        // Flag for when to create a new config rather than attempt to load one from a config folder on tab index changed
        bool creatingNewConfig = false;

        // Used to keep track of what node is currently selected in the treeview...
        static TreeNode currentTreeNode;

        // ...which will typically relate to a specific watch, trigger, or hotkey if it's not the standard GameConfig settings or a brief text description of the watch/trigger/hotkey classes.
        static Watch currentWatch = new Watch();
        static Trigger currentTrigger = new Trigger();
        static Hotkey currentHotkey = new Hotkey();

        // Prior declarations of UI elements for triggers so we can modify or disable them if required based on other trigger settings.
        private ComboBox compTypeCB = new ComboBox();
        private TextBox watch1TB = new TextBox();
        private Label secondaryIdLabel = new Label();
        private TextBox secondaryIdTB = new TextBox();
        private TextBox valueTB = new TextBox();
        private Label sampleFilenameLabel = new Label();
        private TextBox sampleFilenameTB = new TextBox();
        private Button sampleFilenameButton = new Button();
        private TextBox sampleVolumeTB = new TextBox();
        private TextBox sampleSpeedTB = new TextBox();
        private Label tolkLabel = new Label();
        private CheckBox tolkCheckbox = new CheckBox();
        private CheckBox isClockCB = new CheckBox();
        private Label triggerTypeLabel = new Label();
        private ComboBox triggerAllowanceComboBox = new ComboBox();
        private TextBox configNotesTB = new TextBox();

        // Flag to keep track of whether we've loaded the Tolk library
        private static bool tolkLoaded = false;

        // A pointer to the form handle, used by the Hotkey class when registering / unregistering global hotkeys
        public static IntPtr formHandlePtr;

        // Form constructor
        public MainForm()
        {
            InitializeComponent();
            formHandlePtr = this.Handle;

            gameConfig = new GameConfig();

            // Load tolk library ready for use and flip the flag so we only ever load it once
            if (!tolkLoaded)
            {
                Tolk.Load();
                tolkLoaded = true;
            }

            // Initially we aren't running so we add that the current status is stopped to the window title
            this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");

            // Setup the textbox for our config notes            
            Rectangle pbBounds = pictureBox1.Bounds;
            configNotesTB.Multiline = true;
            configNotesTB.ScrollBars = ScrollBars.Vertical; // Use vertical scrollbars if necessary
            configNotesTB.ReadOnly = true;                  // Disable editing config notes from here
            configNotesTB.BackColor = Color.AliceBlue;
            configNotesTB.SetBounds(pbBounds.X, pbBounds.Y, pbBounds.Width, pbBounds.Height); // x-loc, y-loc, x-size, y-size

            populateMainConfigsBox();
        }

        /* -------- Hotkey related functions -------- */

        // Method to register all global hotkeys in the GameConfig. Note: This will display an error MessageBox if registration fails.
        private void registerHotkeys()
        {
            foreach (Hotkey h in gameConfig.hotkeyList)
            {
                h.Register();
            }
        }

        // Method to unregister all global hotkeys in the GameConfig. Note: This will display an error MessageBox if unregistration fails.
        private void unregisterHotkeys()
        {   
            foreach (Hotkey h in gameConfig.hotkeyList)
            {
                h.Unregister();
            }
        }

        // Method to return a hotkey with a given Id
        private Hotkey getHotkeyWithID(int id)
        {
            Hotkey tmp = null;
            foreach (Hotkey h in gameConfig.hotkeyList)
            {
                if (h.Id == id)
                {
                    tmp = h;
                    break;
                }
            }
            return tmp;
        }

        private Hotkey getHotkeyByName(string searchName)
        {
            //MessageBox.Show("hotkey list size: " + gameConfig.hotkeyList.Count);
            Hotkey tmp = null;
            foreach (Hotkey h in gameConfig.hotkeyList)
            {
                //MessageBox.Show("hotkey name: " + h.name);// gameConfig.hotkeyList.Count);                                
                if (h.name.Equals(searchName))                    
                {
                    tmp = h;
                    break;
                }
            }
            return tmp; // Note: This could be null
        }        

        // Main form loop. Note we need to override this so that we can look for hotkeys and act appropriately
        protected override void WndProc(ref Message m)
        {
            // Note: This runs all the time, just based on any window events that occur...

            // A hotkey was pressed? Note: 0x0312 correspondes to WM_HOTKEY.
            // See: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();

                // Now we have a hotkey ID, is it one of OUR hotkeys? (Remembering that many other hotkeys may exist that are not related to SoniFight!)
                // If so then activate the sonification for that hotkey
                Hotkey h = getHotkeyWithID(id);
                if (h != null)
                {
                    // If this hotkey activates a trigger...
                    if (h.targetType == Hotkey.TargetType.ExecutesTrigger)
                    {
                        // ...grab the trigger
                        Trigger t = gameConfig.getTriggerWithID(h.watchOrTriggerID);

                        if (t.UseTolk)
                        {
                            if (Program.screenReaderActive)
                            {
                                string s = Utils.substituteWatchValuesInString(t, t.SampleFilename, gameConfig.ProcessHandle, gameConfig.ProcessBaseAddress);
                                Console.WriteLine(DateTime.Now + " Trigger activated " + t.Id + " via hotkey " + h.Id + " " + Resources.ResourceManager.GetString("sayingTolkString") + s);

                                // ...then say the sample filename text. Final true means interrupt anything currently being spoken
                                Tolk.Speak(s, true);
                            }
                        }
                        else
                        {
                            Program.irrKlang.PlayMenuSample(t); // Note: Play menu sample cuts off any other playing sample - which is what we want for a hotkey
                        }
                    }
                    else // If the hotkey activates a Watch...
                    {
                        // Grab the watch and re-evaluate the pointer chain to ensure it's up to date
                        Watch w = gameConfig.getWatchWithID(h.watchOrTriggerID);
                        w.evaluateAndUpdateDestinationAddress(gameConfig.ProcessHandle, gameConfig.ProcessBaseAddress);

                        if (Program.screenReaderActive)
                        {
                            dynamic var = w.getDynamicValueFromType();
                            Console.WriteLine("Var is: " + var);
                            Console.WriteLine("Watch details: " + w.ToString());

                            string s = w.getDynamicValueFromType().ToString(); // Utils.substituteWatchValuesInString(t, t.SampleFilename);
                            Console.WriteLine(DateTime.Now + " Watch activated " + w.Id + " via hotkey " + h.Id + " - " + Resources.ResourceManager.GetString("sayingTolkString") + s);

                            // ...then say the sample filename text. Final true means interrupt anything currently being spoken
                            Tolk.Speak(s, true);
                        }
                    }

                } // End of if h != null check

                //MessageBox.Show(string.Format("Hotkey #{0} pressed", id));

            } // End of if hotkey detected

            // Carry on...
            base.WndProc(ref m);
        }

        /* -------- End of Hotkey related functions -------- */
        
        public static GameConfig getCurrentGameConfig() { return gameConfig; }

        // Make all controls support double buffering (so we don't have to do so on each control created).
        // Source: https://stackoverflow.com/a/25648710/1868200
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000; // WS_EX_COMPOSITED       
                return handleParam;
            }
        }

        // Read all the directories in the Configs folder and use each directory text as an item in the main GameConfig ComboBox
        private void populateMainConfigsBox()
        {
            configsComboBox.Items.Clear();

            string[] subdirectoryArray = null;
            string configPath = ".\\Configs"; // Do not localise name of Configs folder
            try
            {
                subdirectoryArray = System.IO.Directory.GetDirectories(configPath);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show( Resources.ResourceManager.GetString("missingConfigDirString") );
                System.IO.Directory.CreateDirectory(configPath);
                try
                {
                    subdirectoryArray = System.IO.Directory.GetDirectories(configPath);
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show( Resources.ResourceManager.GetString("configDirCreationFailedString") );
                    this.Close();
                }
            }

            // Strip ".\Configs\" (i.e. the first 10 chars) off the returned list of directories
            for (int loop = 0; loop < subdirectoryArray.Length; loop++)
            {
                subdirectoryArray[loop] = subdirectoryArray[loop].Substring(10);
                Console.WriteLine("Returned dir: " + subdirectoryArray[loop]);
            }

            // No subfolders in the Configs directory? Add some text that informs the user
            if (subdirectoryArray.Length == 0)
            {
                string s = Resources.ResourceManager.GetString("noConfigsFoundString");
                configsComboBox.Items.Add(s);
                configsComboBox.SelectedIndex = 0;

                // If we switch to the edit tab when we have no configs in the Configs folder then set up like we clicked the Create New Config button
                creatingNewConfig = true;
                gameConfig.ConfigDirectory = "";
                gameConfig.Description = "";
                gameConfig.ProcessName = "";
                gameConfig.PollSleepMS = 100;
                gameConfig.watchList.Clear();
                gameConfig.triggerList.Clear();
            }
            else
            {
                // Add the folders to the dropdown, select the first and set the GameConfig's ConfigDirectory to that folder
                configsComboBox.Items.AddRange(subdirectoryArray);

                // Re-select the previously selected index
                this.configsComboBox.SelectedIndex = selectedConfigDropdownIndex;

                if (gameConfig != null)
                {
                    MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);
                }

            } // End of if we found some config directories section
        }

        // Update selected config string (we'll use this string to load the 'config.xml' file from within this directory.
        private void configsComboBox_SelectedIndexChanged(object senderender, EventArgs e)
        {
            // Set the config directory to the text on the dropdown
            // Note: The config is activated when the 'activateAndValidate' method is called
            if (gameConfig != null)
            {
                MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);
            }

            //Console.WriteLine("Selected index text / config dir is now: " + MainForm.gameConfig.ConfigDirectory);

            // Update our configs selected index so we can move back to it if the user goes from the edit to the main tabs
            MainForm.selectedConfigDropdownIndex = this.configsComboBox.SelectedIndex;

            // Display our headphone picturebox
            pictureBox1.Show();

            // Unload samples
            Program.irrKlang.UnloadAllSamples();
        }

        // Method to cleanly close down the app
        private void quitButton_Click(object senderender, EventArgs e)
        {
            // This sets cancellation to pending, which we handle in the associated doWork method
            // to actually perform the cancellation.
            // Note: This is just the code from the stop running config button method - but it's required (including the sleep)
            //       as otherwise we get a "Pure Virtual Function" error on shutdown.
            GameConfig.processConnectionBGW.CancelAsync();
            Program.sonificationBGW.CancelAsync();
            running = false;

            // Close this form and sleep for half a second to give everything enough time to shut down
            this.Close();
            Thread.Sleep(500);

            // Note: Once here SoundPlayer.ShutDown() will be called from the main method because we've been stuck in this form loop up until then.
        }        

        // Method to set up creation of a new GameConfig
        private void createNewConfigButton_Click(object senderender, EventArgs e)
        {
            creatingNewConfig = true;

            // Reset the static GameConfig object so that it gets re-created when we hit the Edit tab...
            gameConfig.ConfigDirectory = "";
            gameConfig.Description = "";
            gameConfig.ProcessName = "";
            gameConfig.PollSleepMS = 100;
            gameConfig.watchList.Clear();
            gameConfig.triggerList.Clear();
            gameConfig.hotkeyList.Clear();

            // ...then change to the Edit tab!
            this.tabControl.SelectedIndex = 1;
        }

        private void loadGameConfig()
        {
            // Inflate game config from XML file
            string pathToConfig = ".\\Configs\\" + MainForm.gameConfig.ConfigDirectory + "\\config.xml";
            Console.WriteLine("About to read config: " + pathToConfig);
            gameConfig = Utils.ReadFromXmlFile<GameConfig>(pathToConfig);

            // Moan if we couldn't inflate the object
            if (gameConfig == null)
            {
                string s1 = Resources.ResourceManager.GetString("deserialiseFailString1");
                string s2 = Resources.ResourceManager.GetString("deserialiseFailString2");
                MessageBox.Show(s1 + gameConfig.ConfigDirectory + s2);
                return;
            }

            // Break up the description string so that it displays with blank lines in a TextBox
            gameConfig.Description = gameConfig.Description.Replace("\n", Environment.NewLine);
        }

        // Method to load the config and start polling
        private void runConfig_Click(object senderender, EventArgs e)
        {
            // The irrKlang object shouldn't be null, but make sure it's not and instantiate it if it is
            if (Program.irrKlang == null) { Program.irrKlang = new SoundPlayer(); }

            // Play the start config sample to let users know a config is running - but only if we're not ALREADY running
            // Note: This will mean running a config always makes a sound - if this was within the below (!running) block it would only make a sound if we were NOT already running
            Program.irrKlang.PlayStartStopSample(true);

            // We should definitely not already be running at this point
            if (!running)
            {
                // Load the game config!
                loadGameConfig();

                // IMPORTANT: Because loading a GameConfig object from file overwrites all properties, and
                //            the ConfigDirectory is not stored in the object, we need to reset it to the
                //            directory name via the selection in the configsComboBox dropdown menu!
                MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);

                // Validate and activate our gameconfig
                gameConfig.Valid = gameConfig.validate();
                gameConfig.Active = gameConfig.activate(); // Note: this activate() method starts our process connection background worker.

                while (!gameConfig.Active)
                {
                    Console.WriteLine("Waiting for gameconfig activation to complete via finding process...");
                    Thread.Sleep(100);
                }

                // Brief delay then announce that we're running if tolk is available. Delay is so announcing the button doesn't overwrite our speech, final true means interupt!
                
                Tolk.Output("SoniFight is now running config" + gameConfig.ConfigDirectory, true);

                // If we have a valid, active config and we're not already running then start our sonification background worker,
                // which calls the 'performSonification' method.
                if (gameConfig.Valid && gameConfig.Active)
                {
                    //Program.sonificationBGW.RunWorkerAsync();
                    this.Text = formTitle + Resources.ResourceManager.GetString("statusRunningString") + gameConfig.ConfigDirectory;

                    // Hotkeys don't work for now so not attempting to call them
                    registerHotkeys();

                    running = true;
                }

            } // End of if !running block
            else
            {
                Console.WriteLine("For some reason we were already running... What the heck!?");
            }

        } // End of runConfig_Click method

        // Method to save the current GameConfig to its config.xml file
        private void saveConfig_Click(object senderender, EventArgs e)
        {
            Console.WriteLine( Resources.ResourceManager.GetString("validatingGameConfigString") );

            bool configIsValid = gameConfig.validate();
            if (!configIsValid)
            {
                // Note: We don't display a MessageBox here as an appropriate one will be generated in the
                //       validate method - so we don't need to double up.
                Console.WriteLine( Resources.ResourceManager.GetString("validationFailedString") );
                return;
            }

            // Construct the relative config directory and relative path to the config.xml file. Do not localise Configs dir or configs.xml filename.
            string configDir = ".\\Configs\\" + gameConfig.ConfigDirectory;
            string configPath = configDir + "\\config.xml";

            // Ensure we don't have any accidental double backslashes in our path
            configPath = configPath.Replace("\\\\", "\\");

            // Try to create the directory. Note: If the directory already exists then this does nothing.
            Directory.CreateDirectory(configDir);

            // Finally, if the GameConfig is valid write it to file
            bool success = Utils.WriteToXmlFile(configPath, gameConfig);

            // If we've saved successfully then we'll set our creating new config flag to false to make the directory location textbox readonly
            if (success)
            {
                creatingNewConfig = false;
            }
            else
            {
                MessageBox.Show("ERROR: Failed to write config file. I don't know any more - it just didn't work out.");
            }

        }

        // Method to rebuild the treeview of the current gameconfig
        private void RebuildTreeViewFromGameConfig()
        {
            // Get the tree
            TreeView tv = this.gcTreeView;

            tv.BeginUpdate();

                // Remove all nodes
                tv.Nodes.Clear();

                /*** NOTE: Node tags are used to identify the type of node and display the correct UI for that node. ***/

                // Add the root node
                TreeNode rootNode = tv.Nodes.Add( Resources.ResourceManager.GetString("gameConfigString") );
                rootNode.Tag = Resources.ResourceManager.GetString("gameConfigTagString");
                tv.SelectedNode = rootNode;

                // Add the "Watches" node (parent of all watch nodes)
                TreeNode watchNode = rootNode.Nodes.Add( Resources.ResourceManager.GetString("watchesString") );
                watchNode.Tag = Resources.ResourceManager.GetString("watchesString");

                // Add the "Triggers" node (parent of all trigger nodes)
                TreeNode triggerNode = rootNode.Nodes.Add( Resources.ResourceManager.GetString("triggersString") );
                triggerNode.Tag = Resources.ResourceManager.GetString("triggersString");

                // Add the "Hotkeys" node (parent of all hotkey nodes)
                TreeNode hotkeyNode = rootNode.Nodes.Add("Hotkeys");
                hotkeyNode.Tag = "Hotkeys"; // Resources.ResourceManager.GetString("triggersString");

                // Add all watch nodes
                TreeNode tempNode;
                foreach (Watch w in gameConfig.watchList)
                {
                    string s = w.Id + "-" + w.Name;
                    tempNode = watchNode.Nodes.Add(s);
                    tempNode.Tag = Resources.ResourceManager.GetString("watchString");
                }

                // Add all trigger nodes
                foreach (Trigger t in gameConfig.triggerList)
                {
                    string s = t.Id + "-" + t.Name;
                    tempNode = triggerNode.Nodes.Add(s);
                    tempNode.Tag = Resources.ResourceManager.GetString("triggerString");
                }

                // Add all hotkey nodes
                foreach (Hotkey h in gameConfig.hotkeyList)
                {   
                    tempNode = hotkeyNode.Nodes.Add(h.name);
                    tempNode.Tag = "Hotkey"; // Resources.ResourceManager.GetString("hotkeyString");
                }

            tv.EndUpdate();

            // Expand the entire tree, enure the selected "Game Config" node is visible and focus it so that the final node displays as 'selected'
            tv.ExpandAll();
            rootNode.EnsureVisible();
            tv.Focus();
        }

        // Method to change the UI based on which tab is selected
        private void tabControl_SelectedIndexChanged(object senderender, EventArgs e)
        {
            // On edit tab?
            if (this.tabControl.SelectedIndex == 1)
            {
                // Cancel sonification if we move to the edit tab while running
                if (running)
                {
                    /*
                    // This sets cancellation to pending, which we handle in the associated doWork method
                    // to actually perform the cancellation.
                    Program.sonificationBGW.CancelAsync();

                    unregisterHotkeys(); // Unregister all hotkeys

                    Console.WriteLine( Resources.ResourceManager.GetString("sonificationStoppedString") );

                    this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");
                    running = false;

                    // Pause then announce we've stopped
                    Thread.Sleep(500);
                    Tolk.Output("SoniFight stopped", false);
                    */

                    // This sets cancellation to pending, which we handle in the associated doWork method
                    // to actually perform the cancellation.
                    GameConfig.processConnectionBGW.CancelAsync();
                    Program.sonificationBGW.CancelAsync();
                    this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");
                    running = false;
                    Thread.Sleep(500);
                    unregisterHotkeys(); // Unregister all hotkeys
                    Program.irrKlang.UnloadAllSamples();
                    Tolk.Output("SoniFight stopped", true);
                }

                // Loading an existing config? Okay...
                if (!creatingNewConfig)
                {
                    // Read GameConfig object from file
                    string pathToConfig = ".\\Configs\\" + this.configsComboBox.Text + "\\config.xml";

                    // Game config could not be loaded in previous pass? Bail.
                    // TODO: Not convinced I need this block... remove and ensure it works.
                    if (gameConfig == null)
                    {
                        return;
                    }

                    // Maintain the config directory as the actual directory we're in!
                    String s = gameConfig.ConfigDirectory;
                    gameConfig = Utils.ReadFromXmlFile<GameConfig>(pathToConfig);

                    // Could not deserialize XML? Bail.
                    if (gameConfig == null)
                    {
                        string s1 = Resources.ResourceManager.GetString("deserialiseFailString1");
                        string s2 = Resources.ResourceManager.GetString("deserialiseFailString2");
                        MessageBox.Show(s1 + gameConfig.ConfigDirectory + s2);
                        return;
                    }

                    // Loaded okay? Swap back the previous config directory because it's in the correct "full relative" format (i.e. not just the directory name on its own)
                    gameConfig.ConfigDirectory = s;
                }

                // Rebuild the TreeView for the newly loaded GameConfig
                RebuildTreeViewFromGameConfig();
            }
            else // We must be on index 0, and so we should update the GameConfig ComboBox in case the user has created a new config they want to run
            {
                populateMainConfigsBox();
            }

        } // End of tabControl_SelectedIndexChanged method

        private void stopAllSonification()
        {
            // This sets cancellation to pending, which we handle in the associated doWork method
            // to actually perform the cancellation.
            GameConfig.processConnectionBGW.CancelAsync();
            Program.sonificationBGW.CancelAsync();
            this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");
            running = false;
            Thread.Sleep(500);
            unregisterHotkeys(); // Unregister all hotkeys
            Program.irrKlang.UnloadAllSamples();
            Tolk.Output("SoniFight stopped", true);
        }

        // Stop button handler to stop the sonification background worker
        private void stopConfigButton_Click(object senderender, EventArgs e)
        {
            if (running)
            {
                // Play the stop config sample
                Program.irrKlang.PlayStartStopSample(false);

                stopAllSonification();
            }
        }

        // Method to refresh the main config selection dropdown menu
        private void refreshButton_Click(object senderender, EventArgs e)
        {
            populateMainConfigsBox();
        }

        // Method to enable or disable trigger UI elements based on the current trigger's settings
        // Note: This is a bit of a dogs breakfast because some conditions need to overwrite each other, and this probably could be simplified,
        //       but it's not performance critical and seems to do the job.
        private void updateTriggerUIElementStates()
        {
            // If we're a modifier or dependent trigger we disable the audio/sample UI elements, otherwise they're enabled
            if (currentTrigger.triggerType == Trigger.TriggerType.Dependent  ||
                currentTrigger.triggerType == Trigger.TriggerType.Modifier   )
            {
                sampleFilenameTB.Enabled = false;
                sampleFilenameButton.Enabled = false;                
                tolkCheckbox.Enabled = false;
                isClockCB.Enabled = false;
            }
            else
            {
                sampleFilenameTB.Enabled = true;
                sampleFilenameButton.Enabled = true;                
                tolkCheckbox.Enabled = true;
                isClockCB.Enabled = true;               
            }

            // Dependent triggers may not have their own dependent triggers because chains get complex - instead use a list of dependent triggers on a normal trigger
            if (currentTrigger.triggerType == Trigger.TriggerType.Dependent)
            {
                secondaryIdTB.Enabled = false;
            }
            else
            {
                secondaryIdTB.Enabled = true;
            }

            // If this trigger uses tolk then we can alter the sample speed or volume so we disable them, otherwise these UI elements are enabled
            if (currentTrigger.UseTolk || currentTrigger.triggerType == Trigger.TriggerType.Dependent)
            {
                sampleSpeedTB.Enabled = false;
                sampleVolumeTB.Enabled = false;
            }
            else
            {
                sampleSpeedTB.Enabled = true;
                sampleVolumeTB.Enabled = true;
            }

            // Change label text on sample filename field based on whether this trigger is using tolk or sample-based output
            if (currentTrigger.UseTolk)
            {
                sampleFilenameLabel.Text = Resources.ResourceManager.GetString("screenReaderTextLabelString");
            }
            else
            {
                sampleFilenameLabel.Text = Resources.ResourceManager.GetString("sampleFilenameLabelString");
            }

            // Display the appropriate label for the 'watch2' field and set whether the tolk checkbox is active or not
            switch (currentTrigger.triggerType)
            {
                case Trigger.TriggerType.Normal:
                case Trigger.TriggerType.Dependent:
                    secondaryIdLabel.Text = Resources.ResourceManager.GetString("dependentTriggerIdLabelString");
                    break;
                case Trigger.TriggerType.Continuous:
                    secondaryIdLabel.Text = Resources.ResourceManager.GetString("watch2IdLabelString");
                    break;
                case Trigger.TriggerType.Modifier:
                    secondaryIdLabel.Text = Resources.ResourceManager.GetString("continuousTriggerIdLabelString");
                    break;
            }

            // You can only change the trigger allowance type (InGame, InMenu or Any) for triggers which are NOT the clock.
            if (currentTrigger.IsClock)
            {
                triggerAllowanceComboBox.Enabled = false;
                sampleFilenameTB.Enabled = false;
                sampleFilenameButton.Enabled = false;
                sampleSpeedTB.Enabled = false;
                sampleVolumeTB.Enabled = false;
                tolkCheckbox.Enabled = false;
            }
            else // Current trigger is not the clock
            {
                triggerAllowanceComboBox.Enabled = true;

                sampleFilenameTB.Enabled = true;

                if (!currentTrigger.UseTolk)
                {
                    if (currentTrigger.triggerType == Trigger.TriggerType.Normal || currentTrigger.triggerType == Trigger.TriggerType.Continuous)
                    {
                        tolkCheckbox.Enabled = true;
                        sampleFilenameButton.Enabled = true;
                        sampleSpeedTB.Enabled = true;
                        sampleVolumeTB.Enabled = true;
                    }
                }
                else // If we're using tolk then the sample filename button, sample speed and sample volume UI elements should be disabled
                {
                    sampleFilenameButton.Enabled = false;
                    sampleSpeedTB.Enabled = false;
                    sampleVolumeTB.Enabled = false;
                }

                // No sample or text allowed for dependent or modifier triggers
                if (currentTrigger.triggerType == Trigger.TriggerType.Dependent || currentTrigger.triggerType == Trigger.TriggerType.Modifier)
                {
                    sampleFilenameTB.Enabled = false;
                }
            }

            // If we're a normal trigger which isn't the clock the tolk checkbox should be active, otherwise it should not
            if (currentTrigger.triggerType == Trigger.TriggerType.Normal && !currentTrigger.IsClock)
            {
                tolkCheckbox.Enabled = true;
            }
            else
            {
                tolkCheckbox.Enabled = false;
            }

            // Continuous triggers cannot use tolk so the sample filename button should always be available. We will also uncheck the useTolk checkbox at this point.
            // Note: We could optionally untick the useTolk checkbox here, but we won't.
            if (currentTrigger.triggerType == Trigger.TriggerType.Continuous)
            {
                sampleFilenameTB.Enabled = true;
                tolkCheckbox.Checked = false;
            }

        } // End of updateTriggerUIElementStates method
        
        // Method to rebuild the details panel depending on the selected node of the TreeView
        private void gcTreeView_AfterSelect(object senderender, TreeViewEventArgs tvea)
        {
            // Get the panel, clear it and set some layout properties
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);

            /*
            // Create a ToolTip and specify its settings
            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 200;
            toolTip.ShowAlways = false;
            // Then do stuff like
            dirLabel.AccessibleRole = AccessibleRole.ToolTip;
            toolTip.SetToolTip(dirLabel, dirLabel.Text.ToString());
            */

            panel.SuspendLayout();
            panel.Visible = false;

            // Because I moved the UI elements to be private members of this form so the enable/disable logic can all be put in a single location
            // (i.e. the above updateTriggerUIElementStates method), when clearing the panel the UI elements get disposed so must be re-new'd here
            // or we get a 'cannot access disposed element) error.
            compTypeCB = new ComboBox();
            watch1TB = new TextBox();
            secondaryIdLabel = new Label();
            secondaryIdTB = new TextBox();
            valueTB = new TextBox();
            sampleFilenameLabel = new Label();
            sampleFilenameTB = new TextBox();
            sampleFilenameButton = new Button();
            sampleVolumeTB = new TextBox();
            sampleSpeedTB = new TextBox();
            tolkLabel = new Label();
            tolkCheckbox = new CheckBox();
            isClockCB = new CheckBox();
            triggerTypeLabel = new Label();
            triggerAllowanceComboBox = new ComboBox();
            
            panel.Padding = padding;
            panel.AutoSize = true;
            panel.Anchor = AnchorStyles.Right;
            panel.Dock = DockStyle.Fill;

            // Update the current node to be the node which triggered this method
            currentTreeNode = tvea.Node;

            /* NOTE: This section was previously done with a switch statement, but they require constant values, and localised values aren't constant, hence the change to if/then/else. */

            // We always start at row zero
            int row = 0;

            // --- Recreate the panel based on the current node type ---

            // Recreate panel as main GameConfig panel
            if ( currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("gameConfigTagString")) )
            {            
                // Set main panel label
                currentUILabel.Text = Resources.ResourceManager.GetString("gameConfigMainUILabelString");

                // ----- Row 0 - Config directory -----                
				Label dirLabel = new Label();
				dirLabel.AutoSize = true;
				dirLabel.Text = Resources.ResourceManager.GetString("directoryLabelString");                
				dirLabel.Anchor = AnchorStyles.Right;
				dirLabel.Margin = padding;

				panel.Controls.Add(dirLabel, 0, row); // Control, Column, Row

				TextBox dirTB = new TextBox();
				dirTB.Text = gameConfig.ConfigDirectory;
				dirTB.TextChanged += (object sender, EventArgs ea) => { gameConfig.ConfigDirectory = dirTB.Text; };
				dirTB.Tag = "dirTB";
				dirTB.Anchor = AnchorStyles.Right;
				dirTB.Dock = DockStyle.Fill;
				dirTB.Margin = padding;

                // If we didn't get here from clicking the create new config button then the directory location should be read-only
                if (!creatingNewConfig)
                {
                    dirTB.ReadOnly = true;                    
                }

				panel.Controls.Add(dirTB, 1, row); // Control, Column, Row                        
				row++;

				// ----- Row 1 - Process name -----                
				Label processLabel = new Label();
				processLabel.AutoSize = true;
				processLabel.Text = Resources.ResourceManager.GetString("processNameLabelString");
				processLabel.Anchor = AnchorStyles.Right;
				processLabel.Margin = padding;
				panel.Controls.Add(processLabel, 0, row); // Control, Column, Row

				TextBox processTB = new TextBox();
				processTB.Text = gameConfig.ProcessName;
				processTB.TextChanged += (object sender, EventArgs ea) => { gameConfig.ProcessName = processTB.Text; };
				processTB.Anchor = AnchorStyles.Right;
				processTB.Dock = DockStyle.Fill;
				processTB.Margin = padding;

				panel.Controls.Add(processTB, 1, row); // Control, Column, Row
				row++;

				// ----- Row 2 - Poll Sleep -----                
				Label pollLabel = new Label();
				pollLabel.AutoSize = true;
				pollLabel.Text = Resources.ResourceManager.GetString("pollSleepLabelString");
				pollLabel.Anchor = AnchorStyles.Right;
				pollLabel.Margin = padding;

				panel.Controls.Add(pollLabel, 0, row); // Control, Column, Row

				TextBox pollTB = new TextBox();
				pollTB.Text = gameConfig.PollSleepMS.ToString();
				pollTB.TextChanged += (object sender, EventArgs ea) =>
				{
					int x;
					bool result = Int32.TryParse(pollTB.Text, out x);
					if (result)
					{
						gameConfig.PollSleepMS = x;
					}
					else
					{
						if (!string.IsNullOrEmpty(pollTB.Text.ToString()))
						{
							MessageBox.Show( Resources.ResourceManager.GetString("pollSleepWarningString") ); 
						}
						else // Field empty? Invalidate it so we can catch it in the save section
						{
							gameConfig.PollSleepMS = -1;
						}
					}
				};
				pollTB.Anchor = AnchorStyles.Right;
				pollTB.Dock = DockStyle.Fill;
				pollTB.Margin = padding;

				panel.Controls.Add(pollTB, 1, row); // Control, Column, Row
				row++;

				// ----- Row 3 - Clock Tick MS -----                
				Label clockTickLabel = new Label();
				clockTickLabel.AutoSize = true;
				clockTickLabel.Text = Resources.ResourceManager.GetString("clockTickLabelString");
				clockTickLabel.Anchor = AnchorStyles.Right;
				clockTickLabel.Margin = padding;

				panel.Controls.Add(clockTickLabel, 0, row); // Control, Column, Row

				TextBox clockTickTB = new TextBox();
				clockTickTB.Text = gameConfig.ClockTickMS.ToString();
				clockTickTB.TextChanged += (object sender, EventArgs ea) =>
				{
					int x;
					bool result = Int32.TryParse(clockTickTB.Text, out x);
					if (result)
					{
						gameConfig.ClockTickMS = x;
					}
					else
					{
						if (!string.IsNullOrEmpty(clockTickTB.Text.ToString()))
						{
							MessageBox.Show(Resources.ResourceManager.GetString("clockTickWarningString") );
						}
						else // Field empty? Invalidate it so we can catch it in the save section
						{
							gameConfig.ClockTickMS = -1;
						}
					}
				};
				clockTickTB.Anchor = AnchorStyles.Right;
				clockTickTB.Dock = DockStyle.Fill;
				clockTickTB.Margin = padding;

				panel.Controls.Add(clockTickTB, 1, row); // Control, Column, Row
				row++;

				// ----- Row 4 - Clock Max -----
				Label clockMaxLabel = new Label();
				clockMaxLabel.AutoSize = true;
				clockMaxLabel.Text = Resources.ResourceManager.GetString("clockMaxLabelString");
				clockMaxLabel.Anchor = AnchorStyles.Right;
				clockMaxLabel.Margin = padding;

				panel.Controls.Add(clockMaxLabel, 0, row); // Control, Column, Row

				TextBox clockMaxTB = new TextBox();
				clockMaxTB.Text = gameConfig.ClockMax.ToString();
				clockMaxTB.TextChanged += (object sender, EventArgs ea) =>
				{
					int x;
					bool result = Int32.TryParse(clockMaxTB.Text, out x);
					if (result)
					{
						gameConfig.ClockMax = x;
					}
					else
					{
						if (!string.IsNullOrEmpty(clockMaxTB.Text.ToString()))
						{
							MessageBox.Show( Resources.ResourceManager.GetString("clockMaxWarningString") );
						}
						else // Field empty? Set a sane default.
						{
							gameConfig.ClockMax = 99;
						}
					}
				};
				clockMaxTB.Anchor = AnchorStyles.Right;
				clockMaxTB.Dock = DockStyle.Fill;
				clockMaxTB.Margin = padding;

				panel.Controls.Add(clockMaxTB, 1, row); // Control, Column, Row
				row++;

                // ----- Row 5 - Normal trigger master volume ---
                Label normalTriggerMasterVolumeLabel = new Label();
                normalTriggerMasterVolumeLabel.AutoSize = true;
                normalTriggerMasterVolumeLabel.Text = Resources.ResourceManager.GetString("normalTriggerMasterVolumeString");
                normalTriggerMasterVolumeLabel.Anchor = AnchorStyles.Right;
                normalTriggerMasterVolumeLabel.Margin = padding;

                panel.Controls.Add(normalTriggerMasterVolumeLabel, 0, row); // Control, Column, Row

                TextBox normalTriggerMasterVolumeTB = new TextBox();
                normalTriggerMasterVolumeTB.Text = gameConfig.NormalTriggerMasterVolume.ToString();
                normalTriggerMasterVolumeTB.Anchor = AnchorStyles.Left;
                normalTriggerMasterVolumeTB.Dock = DockStyle.Fill;
                normalTriggerMasterVolumeTB.Margin = padding;

                normalTriggerMasterVolumeTB.TextChanged += (object sender, EventArgs ea) => {
                    float x;
                    bool result = float.TryParse(normalTriggerMasterVolumeTB.Text, out x);
                    if (result)
                    {
                        gameConfig.NormalTriggerMasterVolume = x;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(normalTriggerMasterVolumeTB.Text.ToString()))
                        {
                            MessageBox.Show(Resources.ResourceManager.GetString("normalTriggerMasterVolumeWarningString"));
                        }
                        else // Field empty? Set to 'full' volume of 1.0f
                        {
                            gameConfig.NormalTriggerMasterVolume = 1.0f;
                        }
                    }
                };

                panel.Controls.Add(normalTriggerMasterVolumeTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 6 - Continuous trigger master volume ---
                Label continuousTriggerMasterVolumeLabel = new Label();
                continuousTriggerMasterVolumeLabel.AutoSize = true;
                continuousTriggerMasterVolumeLabel.Text = Resources.ResourceManager.GetString("continuousTriggerMasterVolumeString");
                continuousTriggerMasterVolumeLabel.Anchor = AnchorStyles.Right;
                continuousTriggerMasterVolumeLabel.Margin = padding;

                panel.Controls.Add(continuousTriggerMasterVolumeLabel, 0, row); // Control, Column, Row

                TextBox continuousTriggerMasterVolumeTB = new TextBox();
                continuousTriggerMasterVolumeTB.Text = gameConfig.ContinuousTriggerMasterVolume.ToString();
                continuousTriggerMasterVolumeTB.Anchor = AnchorStyles.Left;
                continuousTriggerMasterVolumeTB.Dock = DockStyle.Fill;
                continuousTriggerMasterVolumeTB.Margin = padding;

                continuousTriggerMasterVolumeTB.TextChanged += (object sender, EventArgs ea) => {
                    float x;
                    bool result = float.TryParse(continuousTriggerMasterVolumeTB.Text, out x);
                    if (result)
                    {
                        gameConfig.ContinuousTriggerMasterVolume = x;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(continuousTriggerMasterVolumeTB.Text.ToString()))
                        {
                            MessageBox.Show(Resources.ResourceManager.GetString("continuousTriggerMasterVolumeWarningString"));
                        }
                        else // Field empty? Set to 'full' volume of 1.0f
                        {
                            gameConfig.ContinuousTriggerMasterVolume = 1.0f;
                        }
                    }
                };

                panel.Controls.Add(continuousTriggerMasterVolumeTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 7 - Uses tolk -----
                Label usesTolkLabel = new Label();
                usesTolkLabel.AutoSize = true;
                usesTolkLabel.Text = Resources.ResourceManager.GetString("usesTolkString");
                usesTolkLabel.Anchor = AnchorStyles.Right;
                usesTolkLabel.Margin = padding;
                panel.Controls.Add(usesTolkLabel, 0, row); // Control, Column, Row

                TextBox usesTolkTB = new TextBox();

                // Set the text to yes or no based on whether we have an active, tolk-using trigger in this config or not
                if ( Utils.configUsesTolk() )
                {
                    usesTolkTB.Text = Resources.ResourceManager.GetString("yesString");
                }
                else
                {
                    usesTolkTB.Text = Resources.ResourceManager.GetString("noString");
                }

                usesTolkTB.Anchor = AnchorStyles.Left;
                usesTolkTB.Dock = DockStyle.Fill;
                usesTolkTB.Margin = padding;
                usesTolkTB.ReadOnly = true;

                panel.Controls.Add(usesTolkTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 8 - Config description -----                
                Label descLabel = new Label();
				descLabel.AutoSize = true;
				descLabel.Text = Resources.ResourceManager.GetString("descriptionLabelString");
				descLabel.Anchor = AnchorStyles.None;
				descLabel.Margin = padding;

                panel.SetColumnSpan(descLabel, 2); // Span both colums 

                panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

				TextBox descTB = new TextBox();
				descTB.Multiline = true;
				descTB.Height = descTB.Font.Height * 10 + padding.Horizontal; // Set height to be enough for 15 lines

				// Replace all \n newlines with \r\n sp it properly linebreaks on returns
				gameConfig.Description = gameConfig.Description.Replace("\n", Environment.NewLine);

				descTB.Text = gameConfig.Description;
				descTB.TextChanged += (object sender, EventArgs ea) => { gameConfig.Description = descTB.Text; };
				descTB.Anchor = AnchorStyles.Right;
				descTB.Dock = DockStyle.Fill;
				descTB.Margin = padding;
                descTB.ScrollBars = ScrollBars.Vertical;
                panel.SetColumnSpan(descTB, 2); // Span both colums                 

                panel.Controls.Add(descTB, 1, row); // Control, Column, Row
				row++;
						
				// Set all columns and rows to autosize - this is hit on moving to the Edit tab so carrys over into all further UI setups because
				// we never destroy the panel, we just clear and re-populate it.
				panel.ColumnStyles.Clear();
				for (int i = 0; i < panel.ColumnCount; i++)
				{
					panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
				}

				panel.RowStyles.Clear();
				for (int i = 0; i < panel.RowCount; i++)
				{
					panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				}
			}
            // Recreate as watches description
            else if ( currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("watchesString")) )
            { 
                // Update panel UI label
                currentUILabel.Text = Resources.ResourceManager.GetString("watchDescriptionLabelString");

                TextBox watchDescriptionTB = new TextBox();
                watchDescriptionTB.ReadOnly = true;
                watchDescriptionTB.Multiline = true; // Must be enabled to have newlines in output
                watchDescriptionTB.WordWrap = true;
                watchDescriptionTB.Dock = DockStyle.Fill;
                watchDescriptionTB.Font = new Font(watchDescriptionTB.Font.FontFamily, 12); // Crank up the font size

                watchDescriptionTB.Text = Resources.ResourceManager.GetString("watchDescriptionString1"); 
                watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                watchDescriptionTB.Text += Resources.ResourceManager.GetString("watchDescriptionString2");
                watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                watchDescriptionTB.Text += Resources.ResourceManager.GetString("watchDescriptionString3");
                watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                watchDescriptionTB.Text += Resources.ResourceManager.GetString("watchDescriptionString4");

                // Add the textbox and make it span both columns of the TableLayoutPanel
                panel.Controls.Add(watchDescriptionTB);                        
                panel.SetColumnSpan(watchDescriptionTB, 2);
            }
            // Recreate as watch UI
            else if ( currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("watchString")) )
            {               
                currentUILabel.Text = Resources.ResourceManager.GetString("watchSettingsLabelString");

                // Get the current watch we're working from based on the index of the currently selected treenode
                // Note: Each child of a parent treenode starts at index 0, so we can use this index as the
                // index of the watch (in the watchList) that we're currently modifying.
                int watchIndex = currentTreeNode.Index;
                currentWatch = gameConfig.watchList[watchIndex];

                // ----- Row 0 - ID -----                
                Label idLabel = new Label();
                idLabel.AutoSize = true;
                idLabel.Text = Resources.ResourceManager.GetString("watchIdLabelString");
                idLabel.Anchor = AnchorStyles.Right;
                idLabel.Margin = padding;

                panel.Controls.Add(idLabel, 0, row); // Control, Column, Row

                TextBox idTB = new TextBox();
                idTB.Text = currentWatch.Id.ToString();
                idTB.TextChanged += (object o, EventArgs ea) =>
                {
                    int x;
                    bool result = Int32.TryParse(idTB.Text, out x);
                    if (result)
                    {
                        currentWatch.Id = x;
                        currentTreeNode.Text = currentWatch.Id + "-" + currentWatch.Name;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(idTB.Text.ToString()))
                        {
                            MessageBox.Show( Resources.ResourceManager.GetString("watchIdWarningString") );
                        }
                        else // Field empty? Invalidate it so we can catch it in the save section
                        {
                            currentWatch.Id = -1;
                        }
                    }
                };
                idTB.Anchor = AnchorStyles.Right;
                idTB.Dock = DockStyle.Fill;
                idTB.Margin = padding;

                panel.Controls.Add(idTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 1 - Name -----                
                Label nameLabel = new Label();
                nameLabel.AutoSize = true;
                nameLabel.Text = Resources.ResourceManager.GetString("watchNameLabelString");
                nameLabel.Anchor = AnchorStyles.Right;
                nameLabel.Margin = padding;

                panel.Controls.Add(nameLabel, 0, row); // Control, Column, Row

                TextBox nameTB = new TextBox();
                nameTB.Text = currentWatch.Name.ToString();
                nameTB.TextChanged += (object sender, EventArgs ea) =>
                {
                    currentWatch.Name = nameTB.Text;
                    currentTreeNode.Text = currentWatch.Id + "-" + currentWatch.Name;
                };
                nameTB.Anchor = AnchorStyles.Right;
                nameTB.Dock = DockStyle.Fill;
                nameTB.Margin = padding;

                panel.Controls.Add(nameTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 2 - Description -----                
                Label descLabel = new Label();
                descLabel.AutoSize = true;
                descLabel.Text = Resources.ResourceManager.GetString("watchDescriptionLabelString");
                descLabel.Anchor = AnchorStyles.Right;
                descLabel.Margin = padding;

                panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

                TextBox descTB = new TextBox();
                descTB.Text = currentWatch.Description.ToString();
                descTB.TextChanged += (object sender, EventArgs ea) => { currentWatch.Description = descTB.Text; };
                descTB.Anchor = AnchorStyles.Right;
                descTB.Dock = DockStyle.Fill;
                descTB.Margin = padding;

                panel.Controls.Add(descTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 3 - Pointer List -----            
                Label pointerLabel = new Label();
                pointerLabel.AutoSize = true;
                pointerLabel.Text = Resources.ResourceManager.GetString("pointerListLabelString");
                pointerLabel.Anchor = AnchorStyles.Right;
                pointerLabel.Margin = padding;

                panel.Controls.Add(pointerLabel, 0, row); // Control, Column, Row

                TextBox pointerTB = new TextBox();
                string plString = "";
                for (int tempLoop = 0; tempLoop < currentWatch.PointerList.Count; tempLoop++)
                {
                    plString += currentWatch.PointerList[tempLoop];
                    if (tempLoop != currentWatch.PointerList.Count - 1)
                    {
                        plString += ", ";
                    }
                }
                pointerTB.Text = plString;

                pointerTB.TextChanged += (object sender, EventArgs ea) =>
                {
                    List<string> tempPointerList = Utils.CommaSeparatedStringToStringList(pointerTB.Text);
                    int x;
                    foreach (string pointerValue in tempPointerList)
                    {
                        try
                        {
                            x = Convert.ToInt32(pointerValue, 16); // Convert from hex to int
                        }
                        catch (FormatException)
                        {
                            string s1 = Resources.ResourceManager.GetString("illegalPointerString1");
                            string s2 = Resources.ResourceManager.GetString("illegalPointerString2");
                            string s3 = Resources.ResourceManager.GetString("illegalPointerString3");
                            MessageBox.Show(s1 + pointerValue + s2 + currentWatch.Id + s3);
                            return;
                        }
                    }

                    currentWatch.PointerList = tempPointerList;
                };
                        
                pointerTB.Anchor = AnchorStyles.Right;
                pointerTB.Dock = DockStyle.Fill;
                pointerTB.Margin = padding;
                panel.Controls.Add(pointerTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 4 - Value Type -----            
                Label typeLabel = new Label();
                typeLabel.AutoSize = true;
                typeLabel.Text = Resources.ResourceManager.GetString("valueTypeLabelString");                        
                typeLabel.Anchor = AnchorStyles.Right;
                typeLabel.Margin = padding;

                panel.Controls.Add(typeLabel, 0, row); // Control, Column, Row

                ComboBox typeCB = new ComboBox();
                typeCB.DropDownStyle = ComboBoxStyle.DropDownList;
                typeCB.Items.AddRange(dataTypesArray);
                typeCB.SelectedIndex = Utils.GetIntFromValueType(currentWatch.valueType);
                typeCB.Anchor = AnchorStyles.Right;
                typeCB.Dock = DockStyle.Fill;
                typeCB.Margin = padding;

                // Update the value type when the dropdown changes
                typeCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                {
                    currentWatch.valueType = Utils.GetValueTypeFromInt(typeCB.SelectedIndex);
                };

                panel.Controls.Add(typeCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 5 - Active Flag -----            
                Label activeLabel = new Label();
                activeLabel.AutoSize = true;
                activeLabel.Text = Resources.ResourceManager.GetString("activeLabelString");
                activeLabel.Anchor = AnchorStyles.Right;
                activeLabel.Margin = padding;

                panel.Controls.Add(activeLabel, 0, row); // Control, Column, Row

                CheckBox activeCB = new CheckBox();
                activeCB.Checked = currentWatch.Active;
                activeCB.CheckedChanged += (object sender, EventArgs ea) => { currentWatch.Active = activeCB.Checked; };
                activeCB.Anchor = AnchorStyles.Right;
                activeCB.Dock = DockStyle.Fill;
                activeCB.Margin = padding;

                panel.Controls.Add(activeCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 6 - Description -----                
                Label triggersUsingLabel = new Label();
                triggersUsingLabel.AutoSize = true;
                triggersUsingLabel.Text = Resources.ResourceManager.GetString("triggersUsingThisWatchLabelString"); 
                triggersUsingLabel.Anchor = AnchorStyles.Right;
                triggersUsingLabel.Margin = padding;

                panel.Controls.Add(triggersUsingLabel, 0, row); // Control, Column, Row

                TextBox triggersUsingTB = new TextBox();

                // Add all triggers which use this watch to the textbox
                bool foundTriggerUsing = false;
                String s = "";
                for (int loop = 0; loop < gameConfig.triggerList.Count; ++loop)
                {
                    Trigger tempTrigger = gameConfig.triggerList[loop];

                    for (int watchIdLoop = 0; watchIdLoop < tempTrigger.WatchIdList.Count; ++watchIdLoop)
                    {

                        if (tempTrigger.WatchIdList[watchIdLoop] == currentWatch.Id)
                        {
                            s += Convert.ToString(gameConfig.triggerList[loop].Id) + ", ";
                            foundTriggerUsing = true;
                        }

                    } // End of loop over watch IDs in this trigger

                } // End of loop over triggers

                // Didn't find any triggers using this watch - fair enough. Say so.
                if (!foundTriggerUsing)
                {
                    s = Resources.ResourceManager.GetString("noneString");
                }
                else // Strip the final ", " from the end of the string
                {
                    s = s.Substring(0, s.Length - 2);
                }
                        
                triggersUsingTB.Anchor = AnchorStyles.Right;
                triggersUsingTB.Dock = DockStyle.Fill;
                triggersUsingTB.Margin = padding;
                triggersUsingTB.ReadOnly = true;
                triggersUsingTB.Multiline = true;
                triggersUsingTB.Height = triggersUsingTB.Font.Height * 4 + padding.Horizontal; // Set height to be enough for 4 lines, which should be plenty
                triggersUsingTB.Text = s;

                panel.Controls.Add(triggersUsingTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 7 - Delete Watch -----
                Button deleteWatchBtn = new Button();
                deleteWatchBtn.AutoSize = false;
                deleteWatchBtn.Text = Resources.ResourceManager.GetString("deleteWatchButtonString");
                deleteWatchBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Use bitwise-OR to anchor to top-right
                deleteWatchBtn.Margin = padding;
                deleteWatchBtn.BackColor = Color.Red;

                deleteWatchBtn.Click += (object sender, EventArgs ea) =>
                {
                    try
                    {
                        // Check if a hotkey depends on this watch, if so inform user hotkey will be deleted if they delete the watch
                        Hotkey htmp = null;
                        foreach (Hotkey h in gameConfig.hotkeyList)
                        {
                            if (h.targetType == Hotkey.TargetType.ExecutesWatch && h.watchOrTriggerID == currentWatch.Id)
                            {
                                string msg = "Warning: Hotkey " + h.Id + " depends on this Watch. Deleting the Watch will also delete the hotkey. Continue?";
                                DialogResult dr = MessageBox.Show(msg, "Dependent Hotkey Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (dr == DialogResult.No)
                                {
                                    return;
                                }
                                else
                                {
                                    htmp = h;
                                    break;
                                }
                            }
                        }
                        // If there was a dependent hotkey and user opted to still delete the watch
                        if (htmp != null)
                        {
                            // Find and remove the hotkey node
                            TreeNode tmpNode = Utils.FindNodeWithText(gcTreeView, htmp.name);
                            gcTreeView.Nodes.Remove(tmpNode);

                            // Remove the hotkey itself from the hotkeyList
                            gameConfig.hotkeyList.Remove(htmp);
                        }

                        // Remove the watch we're modifying
                        gameConfig.watchList.RemoveAt(watchIndex);

                        // Remove the treenode associated with that watch and give focus to the treeview
                        gcTreeView.Nodes.Remove(currentTreeNode);
                        gcTreeView.Focus();
                    }
                    catch (ArgumentOutOfRangeException aoore)
                    {
                        MessageBox.Show(aoore.Message);
                        return;
                    }
                };

                panel.Controls.Add(deleteWatchBtn, 1, row); // Control, Column, Row
                row++;
            }
            // Recreate as trigger description
            else if ( currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("triggersString")) )
            { 
                currentUILabel.Text = Resources.ResourceManager.GetString("triggerDescriptionLabelString");

                TextBox triggerDescriptionTB = new TextBox();
                triggerDescriptionTB.ReadOnly = true;
                triggerDescriptionTB.Multiline = true; // Must be enabled to have newlines in output
                triggerDescriptionTB.WordWrap = true;
                triggerDescriptionTB.Dock = DockStyle.Fill;
                triggerDescriptionTB.Font = new Font(triggerDescriptionTB.Font.FontFamily, 12); // Crank up the font size
                        
                triggerDescriptionTB.Text = Resources.ResourceManager.GetString("triggerDescriptionString1");
                triggerDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                triggerDescriptionTB.Text += Resources.ResourceManager.GetString("triggerDescriptionString2");
                triggerDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                triggerDescriptionTB.Text += Resources.ResourceManager.GetString("triggerDescriptionString3");
                triggerDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                triggerDescriptionTB.Text += Resources.ResourceManager.GetString("triggerDescriptionString4");

                // Add the textbox and make it span both columns in the TableLayoutPanel
                panel.Controls.Add(triggerDescriptionTB);                        
                panel.SetColumnSpan(triggerDescriptionTB, 2);
            }
            // Recreate as trigger UI
            else if ( currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("triggerString")) )
            {
                // Set main UI panel label
                currentUILabel.Text = Resources.ResourceManager.GetString("triggerSettingsLabelString");                

                // Get the current watch we're working from based on the index of the currently selected treenode
                // Note: Each child of a parent treenode starts at index 0, so we can use this index as the
                // index of the watch (in the watchList) that we're currently modifying.
                int triggerIndex = currentTreeNode.Index;
                currentTrigger = gameConfig.triggerList[triggerIndex];
                
                // ----- Row 0 - ID -----                
                Label idLabel = new Label();
                idLabel.AutoSize = true;
                idLabel.Text = Resources.ResourceManager.GetString("triggerIdLabelString");
                idLabel.Anchor = AnchorStyles.Right;
                idLabel.Margin = padding;

                panel.Controls.Add(idLabel, 0, row); // Control, Column, Row

                TextBox idTB = new TextBox();
                idTB.Text = currentTrigger.Id.ToString();

                idTB.TextChanged += (object sender, EventArgs ea) =>
                {
                    int x;
                    bool result = Int32.TryParse(idTB.Text, out x);
                    if (result)
                    {
                        currentTrigger.Id = x;
                        currentTreeNode.Text = currentTrigger.Id + "-" + currentTrigger.Name;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(idTB.Text.ToString()))
                        {
                            MessageBox.Show( Resources.ResourceManager.GetString("triggerIdWarningString") );
                        }
                        else // Field empty? Invalidate it so we can catch it in the save section
                        {
                            currentTrigger.Id = -1;
                        }
                    }
                };

                idTB.Anchor = AnchorStyles.Right;
                idTB.Dock = DockStyle.Fill;
                idTB.Margin = padding;

                panel.Controls.Add(idTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 1 - Name -----                
                Label nameLabel = new Label();
                nameLabel.AutoSize = true;
                nameLabel.Text = Resources.ResourceManager.GetString("triggerNameLabelString");
                nameLabel.Anchor = AnchorStyles.Right;
                nameLabel.Margin = padding;
                panel.Controls.Add(nameLabel, 0, row); // Control, Column, Row

                TextBox nameTB = new TextBox();
                nameTB.Text = currentTrigger.Name.ToString();
                nameTB.TextChanged += (object sender, EventArgs ea) =>
                {
                    currentTrigger.Name = nameTB.Text;
                    currentTreeNode.Text = currentTrigger.Id + "-" + currentTrigger.Name;
                };
                nameTB.Tag = "nameTB";
                nameTB.Anchor = AnchorStyles.Right;
                nameTB.Dock = DockStyle.Fill;
                nameTB.Margin = padding;

                panel.Controls.Add(nameTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 2 - Description -----                
                Label descLabel = new Label();
                descLabel.AutoSize = true;
                descLabel.Text = Resources.ResourceManager.GetString("triggerDescriptionLabelString"); 
                descLabel.Anchor = AnchorStyles.Right;
                descLabel.Margin = padding;

                panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

                TextBox descTB = new TextBox();
                descTB.Text = currentTrigger.Description.ToString();
                descTB.TextChanged += (object sender, EventArgs ea) => { currentTrigger.Description = descTB.Text; };
                descTB.Anchor = AnchorStyles.Right;
                descTB.Dock = DockStyle.Fill;
                descTB.Margin = padding;

                panel.Controls.Add(descTB, 1, row); // Control, Column, Row
                row++;

                // Add a simulated horizontal rule between this section and the next

                // Create a label to be used as a simulated hrule
                Label hrule = new Label();
                hrule.AutoSize = false;
                hrule.Anchor = AnchorStyles.Right;
                hrule.Dock = DockStyle.Fill;
                hrule.Height = 2;
                hrule.BorderStyle = BorderStyle.Fixed3D;
                panel.SetColumnSpan(hrule, 2);
                panel.Controls.Add(hrule, 0, row);
                row++;

                // -----  Row 3 - Trigger type (Normal, Dependent, Continuous, Modifier) -----                
                triggerTypeLabel.AutoSize = true;
                triggerTypeLabel.Text = Resources.ResourceManager.GetString("triggerTypeLabelString");
                triggerTypeLabel.Anchor = AnchorStyles.Right;
                triggerTypeLabel.Margin = padding;

                panel.Controls.Add(triggerTypeLabel, 0, row); // Control, Column, Row
                
                ComboBox triggerTypeComboBox = new ComboBox();
                triggerTypeComboBox.Tag = "triggerTypeCB";
                triggerTypeComboBox.Anchor = AnchorStyles.Left;
                triggerTypeComboBox.Dock = DockStyle.Fill;
                triggerTypeComboBox.Margin = padding;
                triggerTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                triggerTypeComboBox.Items.AddRange(triggerTypesArray);
                triggerTypeComboBox.SelectedIndex = Utils.GetIntFromTriggerType(currentTrigger.triggerType);

                // Trigger type index changed event handler
                triggerTypeComboBox.SelectedIndexChanged += (object o, EventArgs ae) =>
                {
                    currentTrigger.triggerType = Utils.GetTriggerTypeFromInt(triggerTypeComboBox.SelectedIndex);

                    updateTriggerUIElementStates();
                };
                
                panel.Controls.Add(triggerTypeComboBox, 1, row); // Control, Column, Row
                row++;

                // ----- Row 4 - Comparison type ----- 
                Label compTypeLabel = new Label();
                compTypeLabel.AutoSize = true;
                compTypeLabel.Text = Resources.ResourceManager.GetString("comparisonTypeLabelString");
                compTypeLabel.Anchor = AnchorStyles.Right;
                compTypeLabel.Margin = padding;

                panel.Controls.Add(compTypeLabel, 0, row); // Control, Column, Row

                compTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
                compTypeCB.Items.AddRange(comparisonTypesArray);
                compTypeCB.SelectedIndex = Utils.GetIntFromComparisonType(currentTrigger.comparisonType);

                compTypeCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                {
                    currentTrigger.comparisonType = Utils.GetComparisonTypeFromInt(compTypeCB.SelectedIndex);
                };

                compTypeCB.Anchor = AnchorStyles.Left;
                compTypeCB.Dock = DockStyle.Fill;
                compTypeCB.Margin = padding;

                panel.Controls.Add(compTypeCB, 1, row); // Control, Column, Row
                row++;

                // Row 5 - Watch ID 1
                Label watch1Label = new Label();
                watch1Label.AutoSize = true;
                watch1Label.Text = Resources.ResourceManager.GetString("watch1LabelString");
                watch1Label.Anchor = AnchorStyles.Right;
                watch1Label.Margin = padding;

                panel.Controls.Add(watch1Label, 0, row); // Control, Column, Row

                // Set the text on the textbox to be a space separated version of the watch ID list
                watch1TB.Text = string.Join(" ", currentTrigger.WatchIdList);

                watch1TB.Anchor = AnchorStyles.Left;
                watch1TB.Dock = DockStyle.Fill;
                watch1TB.Margin = padding;

                watch1TB.TextChanged += (object sender, EventArgs ea) =>
                {
                    // Take a copy of the existing watch list before we try to parse the string
                    List<int> tempWatchList = new List<int>();
                    for (int loop = 0; loop < currentTrigger.WatchIdList.Count; ++loop)
                    {
                        tempWatchList.Add(currentTrigger.WatchIdList[loop]);
                    }

                    // Try to parse string. Returns null on fail (also warns user via messagebox)
                    currentTrigger.WatchIdList = Utils.stringToIntList(watch1TB.Text.ToString());

                    // Failed to parse? Replace the WatchIdList with the copy just took!
                    if (currentTrigger.WatchIdList == null)
                    {
                        currentTrigger.WatchIdList = tempWatchList;

                        // Now replace the text on the textbox with that legal version so we know what the actual data in the watch ID list is...
                        watch1TB.Text = string.Join(" ", currentTrigger.WatchIdList);

                        // ...and move the text carrat to the end of the line so the user can try again
                        watch1TB.Select(watch1TB.Text.Length, 0);
                    }
                    else // Watch list is valid - so check each watch exists and warn if not.
                    {
                        for (int loop = 0; loop < currentTrigger.WatchIdList.Count; ++loop)
                        {
                            Watch tempW = Utils.getWatchWithId(currentTrigger.WatchIdList[loop]);

                            if (tempW == null)
                            {
                                MessageBox.Show("Warning: Trigger " + currentTrigger.Id + " uses watch with ID " + currentTrigger.WatchIdList[loop] + " but such watch exists!");
                            }
                        }
                    }
                };

                panel.Controls.Add(watch1TB, 1, row); // Control, Column, Row
                row++;

                // Row 6 - Watch ID 2 - this is used for dependent triggers for normal triggers, a secondary watch for continuous triggers and continuous trigger id for modifier triggers
                secondaryIdLabel.AutoSize = true;

                // Display the appropriate label for the secondary ID label
                switch (currentTrigger.triggerType)
                {
                    case Trigger.TriggerType.Normal:
                    case Trigger.TriggerType.Dependent:
                        secondaryIdLabel.Text = Resources.ResourceManager.GetString("secondaryIdLabelDependentIdString");
                        
                        // Set the text on the textbox to be a space separated version of the secondary Id List
                        secondaryIdTB.Text = string.Join(" ", currentTrigger.SecondaryIdList);

                        /*string dependentTriggerString = "";
                        for (int loop = 0; loop < currentTrigger.SecondaryIdList.Count; ++loop)
                        {
                            dependentTriggerString += currentTrigger.SecondaryIdList[loop];
                            if (loop != currentTrigger.SecondaryIdList.Count - 1)
                            {
                                dependentTriggerString += " ";
                            }
                        }
                        secondaryIdTB.Text = dependentTriggerString;*/
                        break;
                    case Trigger.TriggerType.Continuous:
                        secondaryIdLabel.Text = Resources.ResourceManager.GetString("secondaryIdLabelWatch2IdString");
                        secondaryIdTB.Text = currentTrigger.SecondaryIdList[0].ToString();
                        break;
                    case Trigger.TriggerType.Modifier:
                        secondaryIdLabel.Text = Resources.ResourceManager.GetString("secondaryIdLabelContinuousTriggerIdString");
                        secondaryIdTB.Text = currentTrigger.SecondaryIdList[0].ToString();
                        break;
                }

                secondaryIdLabel.Anchor = AnchorStyles.Right;
                secondaryIdLabel.Margin = padding;
                panel.Controls.Add(secondaryIdLabel, 0, row); // Control, Column, Row
                
                // Set the text on the secondary ID list textbox to be a string version of the secondary ID list if the list isn't empty
                /*if (currentTrigger.SecondaryIdList.Count > 0)
                {
                    secondaryIdTB.Text = currentTrigger.SecondaryIdList[0].ToString();
                }*/

                secondaryIdTB.Anchor = AnchorStyles.Left;
                secondaryIdTB.Dock = DockStyle.Fill;
                secondaryIdTB.Margin = padding;

                secondaryIdTB.TextChanged += (object sender, EventArgs ea) =>
                {
                    // Normal triggers may have multiple dependent triggers
                    if (currentTrigger.triggerType == Trigger.TriggerType.Normal)
                    {
                        if ( string.IsNullOrEmpty( secondaryIdTB.Text.ToString() ) || secondaryIdTB.Text.Equals("-") )
                        {
                            currentTrigger.SecondaryIdList = new List<int>();
                            currentTrigger.SecondaryIdList.Add(-1);
                        }
                        else // Parse string to int list
                        {
                            // Take a copy of the secondary ID list incase the user has broken it with invalid input so we can put it back as it was
                            List<int> tempList = new List<int>();
                            for (int loop = 0; loop < currentTrigger.SecondaryIdList.Count; ++loop)
                            {
                                tempList.Add(currentTrigger.SecondaryIdList[loop]);
                            }

                            currentTrigger.SecondaryIdList = Utils.stringToIntList(secondaryIdTB.Text.ToString());

                            // Failed to parse? Replace the WatchIdList with the copy just took!
                            if (currentTrigger.SecondaryIdList == null)
                            {
                                currentTrigger.SecondaryIdList = tempList;

                                // Now replace the text on the textbox with that legal version so we know what the actual data in the watch ID list is...
                                secondaryIdTB.Text = string.Join(" ", currentTrigger.SecondaryIdList);

                                // ...and move the text carrat to the end of the line so the user can try again
                                secondaryIdTB.Select(secondaryIdTB.Text.Length, 0);
                            }
                            else // Valid list of ints - but does each trigger exist? Warn user if not.
                            {
                                for (int loop = 0; loop < currentTrigger.SecondaryIdList.Count; ++loop)
                                {
                                    if (currentTrigger.SecondaryIdList[loop] != -1)
                                    {
                                        Trigger tempT = Utils.getTriggerWithId(currentTrigger.SecondaryIdList[loop]);

                                        if (tempT == null)
                                        {
                                            MessageBox.Show("Warning: Trigger " + currentTrigger.Id + " has a dependency on trigger " + currentTrigger.SecondaryIdList[loop] + " but no such trigger exists!");
                                        }
                                    }

                                } // End of loop block

                            } // End of secondaryIdList was not null section

                        } // End of parse string to int list block
                    }
                    else // Triggers which are of dependent, modifier or continuous types may only have a single value in this field
                    {
                        int x;
                        bool result = Int32.TryParse(secondaryIdTB.Text, out x);
                        if (result)
                        {
                            currentTrigger.SecondaryIdList[0] = x;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(secondaryIdTB.Text.ToString()) && !string.Equals(secondaryIdTB.Text.ToString(), "-"))
                            {
                                MessageBox.Show(Resources.ResourceManager.GetString("secondaryIdWarningString"));
                            }
                            else // Field empty? Invalidate it so we can catch it in the save section
                            {
                                if (currentTrigger.SecondaryIdList == null || currentTrigger.SecondaryIdList.Count == 0)
                                {
                                    currentTrigger.SecondaryIdList = new List<int>();
                                    currentTrigger.SecondaryIdList.Add(-1);
                                }
                            }

                        } // End of if parse to single int failed section

                    } // End of if we're a dependent, modifier or continuous trigger section

                }; // End of secondaryIdTB text changed event handler

                panel.Controls.Add(secondaryIdTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 7 - Trigger Value -----
                Label valueLabel = new Label();
                valueLabel.AutoSize = true;
                valueLabel.Text = Resources.ResourceManager.GetString("triggerValueMaxRangeLabelString");
                valueLabel.Anchor = AnchorStyles.Right;
                valueLabel.Margin = padding;
                panel.Controls.Add(valueLabel, 0, row); // Control, Column, Row

                // Note: valueTB is created above so we can access it in the comparison type dropdown
                // Also: This TextBox will toggle to ReadOnly if the comparison type is distance, and editable if it's
                //       anything else. See the above trigger type ComboBox row.
                valueTB.Text = currentTrigger.Value.ToString();

                // Set a max of 33 chars
                valueTB.MaxLength = Program.TEXT_COMPARISON_CHAR_LIMIT;

                // Comparison TextBox handler
                valueTB.TextChanged += (object sender, EventArgs ea) => {

                    // Trim whitespace
                    currentTrigger.Value = valueTB.Text.TrimEnd();

                };
                
                valueTB.Anchor = AnchorStyles.Left;
                valueTB.Dock = DockStyle.Fill;
                valueTB.Margin = padding;
                panel.Controls.Add(valueTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 9 - Sample filename / tolk output text row -----                
                sampleFilenameLabel.AutoSize = true;
                sampleFilenameLabel.Text = Resources.ResourceManager.GetString("sampleFilenameLabelString");
                sampleFilenameLabel.Anchor = AnchorStyles.Right;
                sampleFilenameLabel.Margin = padding;
                panel.Controls.Add(sampleFilenameLabel, 0, row); // Control, Column, Row

                // If we want to add two controls to a cell in a TableLayoutPanel we have to put a panel in that cell
                // then add the controls to that panel!
                Panel sampleSelectionPanel = new Panel();
                sampleSelectionPanel.Height = compTypeCB.Height;
                sampleSelectionPanel.Dock = DockStyle.Fill;
                sampleSelectionPanel.Margin = padding;

                sampleFilenameTB.Text = currentTrigger.SampleFilename;
                sampleFilenameTB.Anchor = AnchorStyles.Right;
                sampleFilenameTB.Dock = DockStyle.Fill;
                sampleFilenameTB.Margin = new System.Windows.Forms.Padding(0);
                
                sampleFilenameTB.TextChanged += (object sender, EventArgs ea) => { currentTrigger.SampleFilename = sampleFilenameTB.Text; };

                sampleSelectionPanel.Controls.Add(sampleFilenameTB);

                // ----- Use Tolk label & checkbox -----

                tolkLabel.Text = Resources.ResourceManager.GetString("useTolkLabelString");
                tolkLabel.AutoSize = true;
                tolkLabel.Anchor = AnchorStyles.Right;
                tolkLabel.Dock = DockStyle.Right;
                tolkLabel.Padding = padding;
                sampleSelectionPanel.Controls.Add(tolkLabel);

                tolkCheckbox.Anchor = AnchorStyles.Right;
                tolkCheckbox.Dock = DockStyle.Right;
                tolkCheckbox.AutoSize = true;
                tolkCheckbox.Padding = padding;
                tolkCheckbox.Checked = currentTrigger.UseTolk;

                tolkCheckbox.CheckedChanged += (object sender, EventArgs ea) => {
                    // Update the new isClock status on our trigger
                    currentTrigger.UseTolk = tolkCheckbox.Checked;

                    updateTriggerUIElementStates();
                };

                // Add the tolk checkbox
                sampleSelectionPanel.Controls.Add(tolkCheckbox);
                
                sampleFilenameButton.AutoSize = true;
                sampleFilenameButton.Anchor = AnchorStyles.Right;
                sampleFilenameButton.Dock = DockStyle.Right;
                sampleFilenameButton.Margin = new System.Windows.Forms.Padding(0);
                sampleFilenameButton.Text = Resources.ResourceManager.GetString("selectSampleButtonString");
                sampleFilenameButton.Click += (object o, EventArgs ae) =>
                {
                    OpenFileDialog file = new OpenFileDialog();

                    file.InitialDirectory = Environment.CurrentDirectory + "\\Configs\\" + gameConfig.ConfigDirectory; // Open dialog in gameconfig directory

                    if (file.ShowDialog() == DialogResult.OK)
                    {
                        // Note: Filename gives you the full path to the file, SafeFilename gives you ONLY the filename including extension, which is what we want
                        currentTrigger.SampleFilename = file.SafeFileName;
                        sampleFilenameTB.Text = file.SafeFileName;
                    }
                };
                sampleSelectionPanel.Controls.Add(sampleFilenameButton);
                
                // Now we can add the sample selection panel to the cell!
                panel.Controls.Add(sampleSelectionPanel, 1, row);
                row++;

                // ----- Row 10 - Trigger sample volume ---
                Label sampleVolumeLabel = new Label();
                sampleVolumeLabel.AutoSize = true;
                sampleVolumeLabel.Text = Resources.ResourceManager.GetString("sampleVolumeLabelString");
                sampleVolumeLabel.Anchor = AnchorStyles.Right;
                sampleVolumeLabel.Margin = padding;

                panel.Controls.Add(sampleVolumeLabel, 0, row); // Control, Column, Row

                sampleVolumeTB.Text = currentTrigger.SampleVolume.ToString();
                sampleVolumeTB.Anchor = AnchorStyles.Left;
                sampleVolumeTB.Dock = DockStyle.Fill;
                sampleVolumeTB.Margin = padding;

                // Disable sample volume field if we're the clock trigger, or using tolk, or are a dependent trigger
                if (currentTrigger.IsClock || currentTrigger.UseTolk || currentTrigger.triggerType == Trigger.TriggerType.Dependent)
                {   
                    sampleVolumeTB.Enabled = false;
                    sampleVolumeTB.Update();
                }

                sampleVolumeTB.TextChanged += (object sender, EventArgs ea) => {
                    float x;
                    bool result = float.TryParse(sampleVolumeTB.Text, out x);
                    if (result)
                    {
                        currentTrigger.SampleVolume = x;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sampleVolumeTB.Text.ToString()))
                        {
                            MessageBox.Show( Resources.ResourceManager.GetString("sampleVolumeWarningString") );
                        }
                        else // Field empty? Invalidate it so we can catch it in the save section
                        {
                            currentTrigger.SampleVolume = -1.0f;
                        }
                    }
                };

                panel.Controls.Add(sampleVolumeTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 11 - Trigger sample rate ---
                Label sampleSpeedLabel = new Label();
                sampleSpeedLabel.AutoSize = true;
                sampleSpeedLabel.Text = Resources.ResourceManager.GetString("sampleSpeedLabelString");
                sampleSpeedLabel.Anchor = AnchorStyles.Right;
                sampleSpeedLabel.Margin = padding;
                panel.Controls.Add(sampleSpeedLabel, 0, row); // Control, Column, Row

                sampleSpeedTB.Text = currentTrigger.SampleSpeed.ToString();
                sampleSpeedTB.Anchor = AnchorStyles.Left;
                sampleSpeedTB.Dock = DockStyle.Fill;
                sampleSpeedTB.Margin = padding;

                sampleSpeedTB.TextChanged += (object sender, EventArgs ea) => {
                    float x;
                    bool result = float.TryParse(sampleSpeedTB.Text, out x);
                    if (result)
                    {
                        // Cap if necessary and set sample speed
                        if (x > GameConfig.MAX_SAMPLE_PLAYBACK_SPEED) { x = GameConfig.MAX_SAMPLE_PLAYBACK_SPEED; }
                        if (x < GameConfig.MIN_SAMPLE_PLAYBACK_SPEED) { x = GameConfig.MIN_SAMPLE_PLAYBACK_SPEED; }
                        currentTrigger.SampleSpeed = x;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sampleSpeedTB.Text.ToString()))
                        {
                            string s1 = Resources.ResourceManager.GetString("sampleSpeedWarningString1");
                            string s2 = Resources.ResourceManager.GetString("sampleSpeedWarningString2");
                            string s3 = Resources.ResourceManager.GetString("sampleSpeedWarningString3");
                            MessageBox.Show(s1 + GameConfig.MIN_SAMPLE_PLAYBACK_SPEED + s2 + GameConfig.MAX_SAMPLE_PLAYBACK_SPEED + s3);
                        }
                        else // Field empty? Invalidate it so we can catch it in the save section
                        {
                            currentTrigger.SampleSpeed = -1.0f;
                        }
                    }
                };

                panel.Controls.Add(sampleSpeedTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 12 - isClock Flag -----            
                Label isClockLabel = new Label();
                isClockLabel.AutoSize = true;
                isClockLabel.Text = Resources.ResourceManager.GetString("isClockLabelString");
                isClockLabel.Anchor = AnchorStyles.Right;
                isClockLabel.Margin = padding;
                panel.Controls.Add(isClockLabel, 0, row); // Control, Column, Row
                               
                isClockCB.Checked = currentTrigger.IsClock;                

                isClockCB.CheckedChanged += (object sender, EventArgs ea) => {
                    // Update the new isClock status on our trigger
                    currentTrigger.IsClock = isClockCB.Checked;

                    updateTriggerUIElementStates();
                };

                isClockCB.Anchor = AnchorStyles.Right;
                isClockCB.Dock = DockStyle.Fill;
                isClockCB.Margin = padding;

                panel.Controls.Add(isClockCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 13 - Allowance Type (Any / InGame / InMenu) row -----
                Label triggerAllowanceLabel = new Label();
                triggerAllowanceLabel.AutoSize = true;
                triggerAllowanceLabel.Text = Resources.ResourceManager.GetString("allowanceTypeLabelString");
                triggerAllowanceLabel.Anchor = AnchorStyles.Right;
                triggerAllowanceLabel.Margin = padding;
                panel.Controls.Add(triggerAllowanceLabel, 0, row); // Allowance, Column, Row

                
                triggerAllowanceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                triggerAllowanceComboBox.Items.AddRange(allowanceTypesArray);
                triggerAllowanceComboBox.SelectedIndex = Utils.GetIntFromAllowanceType(currentTrigger.allowanceType);

                triggerAllowanceComboBox.SelectedIndexChanged += (object o, EventArgs ae) =>
                {
                    currentTrigger.allowanceType = Utils.GetAllowanceTypeFromInt(triggerAllowanceComboBox.SelectedIndex);                    
                };

                triggerAllowanceComboBox.Anchor = AnchorStyles.Left;
                triggerAllowanceComboBox.Dock = DockStyle.Fill;
                triggerAllowanceComboBox.Margin = padding;

                panel.Controls.Add(triggerAllowanceComboBox, 1, row); // Control, Column, Row
                row++;

                // ----- Row 14 - Active Flag -----            
                Label activeLabel = new Label();
                activeLabel.AutoSize = true;
                activeLabel.Text = Resources.ResourceManager.GetString("activeLabelString");
                activeLabel.Anchor = AnchorStyles.Right;
                activeLabel.Margin = padding;

                panel.Controls.Add(activeLabel, 0, row); // Control, Column, Row

                CheckBox activeCB = new CheckBox();
                activeCB.Checked = currentTrigger.Active;
                activeCB.CheckedChanged += (object sender, EventArgs ea) => { currentTrigger.Active = activeCB.Checked; };
                activeCB.Anchor = AnchorStyles.Right;
                activeCB.Dock = DockStyle.Fill;
                activeCB.Margin = padding;

                panel.Controls.Add(activeCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 15 - Delete Trigger -----
                Button deleteTriggerBtn = new Button();
                deleteTriggerBtn.AutoSize = true;
                deleteTriggerBtn.Text = Resources.ResourceManager.GetString("deleteTriggerButtonString");
                deleteTriggerBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Use bitwise-OR to anchor to top-right
                deleteTriggerBtn.Margin = padding;
                deleteTriggerBtn.BackColor = Color.Red;

                deleteTriggerBtn.Click += (object sender, EventArgs ea) =>
                {
                    try
                    {
                        // Check if a hotkey depends on this trigger, if so inform user hotkey will be deleted if they delete the trigger
                        Hotkey htmp = null;
                        foreach (Hotkey h in gameConfig.hotkeyList)
                        {
                            if (h.targetType == Hotkey.TargetType.ExecutesTrigger && h.watchOrTriggerID == currentTrigger.Id)
                            {
                                string msg = "Warning: Hotkey " + h.Id + " depends on this Trigger. Deleting the Trigger will also delete the hotkey. Continue?";
                                DialogResult dr = MessageBox.Show(msg, "Dependent Hotkey Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (dr == DialogResult.No)
                                {
                                    return;
                                }
                                else
                                {
                                    htmp = h;
                                    break;
                                }
                            }
                        }
                        // If there was a dependent hotkey and user opted to still delete the trigger
                        if (htmp != null)
                        {
                            // Find and remove the hotkey node
                            TreeNode tmpNode = Utils.FindNodeWithText(gcTreeView, htmp.name);
                            gcTreeView.Nodes.Remove(tmpNode);

                            // Remove the hotkey itself from the hotkeyList
                            gameConfig.hotkeyList.Remove(htmp);
                        }

                        // Remove the trigger we're modifying
                        Trigger trigTmp = Utils.getTriggerWithId(triggerIndex);
                        gameConfig.triggerList.Remove(trigTmp);

                        // Remove the treenode associated with that watch and give focus to the treeview
                        gcTreeView.Nodes.Remove(currentTreeNode);
                        gcTreeView.Focus();
                    }
                    catch (ArgumentOutOfRangeException aoore)
                    {
                        MessageBox.Show("Argument out of range: " + aoore.Message);
                        return;
                    }
                };

                panel.Controls.Add(deleteTriggerBtn, 1, row); // Control, Column, Row
                row++;

                updateTriggerUIElementStates();
            }
            // Recreate as hotkeys description
            else if (currentTreeNode.Tag.ToString().Equals("Hotkeys"))
            {
                currentUILabel.Text = "Hotkey Description"; //Resources.ResourceManager.GetString("triggerDescriptionLabelString");

                TextBox hotkeysDescriptionTB = new TextBox();
                hotkeysDescriptionTB.ReadOnly = true;
                hotkeysDescriptionTB.Multiline = true; // Must be enabled to have newlines in output
                hotkeysDescriptionTB.WordWrap = true;
                hotkeysDescriptionTB.Dock = DockStyle.Fill;
                hotkeysDescriptionTB.Font = new Font(hotkeysDescriptionTB.Font.FontFamily, 12); // Crank up the font size

                hotkeysDescriptionTB.Text = "Hotkeys allow you to activate a Watch or Trigger in a given config on demand by pressing a key or key combination.";
                hotkeysDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                hotkeysDescriptionTB.Text += "You do not need to provide a modifier for a hotkey - for example, you can just have the key F9 or 'J' or such if you want. ";
                hotkeysDescriptionTB.Text += "Alternatively, you can assign any combination of Alt, Control, and/or Shift along with your activation key. It is up to you to avoid choosing a hotkey activation sequence that clashes with an existing global hotkey outside of SoniFight.";
                hotkeysDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                hotkeysDescriptionTB.Text += "To create a Hotkey, click the 'Add Hotkey' button. Each Hotkey will be assigned its own unique ID value, and only one hotkey can exist for any given Watch or Trigger. ";
                hotkeysDescriptionTB.Text += "With a new Hotkey created, choose whether it should activate a Watch or a Trigger from the dropdown menu, then enter the Watch or Trigger's ID number into the 'Watch or Trigger ID' textbox. ";
                hotkeysDescriptionTB.Text += "Finally, with the 'Hotkey Activation Sequence' textbox focused press your hotkey combination of choice, then save the current config.";
                hotkeysDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                hotkeysDescriptionTB.Text += "Hotkeys are registered (i.e. made active) when you run the config, and unregistered when the config is stopped. ";
                hotkeysDescriptionTB.Text += "Hotkeys may be enabled or disabled via each Hotkey's 'Enabled' checkbox.";
                hotkeysDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                hotkeysDescriptionTB.Text += "For further details on Hotkeys please see the SoniFight User Guide.";

                // Add the textbox and make it span both columns in the TableLayoutPanel
                panel.Controls.Add(hotkeysDescriptionTB);
                panel.SetColumnSpan(hotkeysDescriptionTB, 2);
            }
            // Recreate as hotkey creation UI
            else if (currentTreeNode.Tag.ToString().Equals("Hotkey"))
            {
                currentUILabel.Text = "Add Hotkey";

                // If we can find a hotkey with the Id as extracted from the currentTreeNode text then grab it and assign it as our current hotkey
                Hotkey veryTemp = Utils.getHotkeyWithId(Utils.getStartingNumber(currentTreeNode.Text));
                if (veryTemp == null)
                {
                    MessageBox.Show("Could not get hotkey with ID: " + Utils.getStartingNumber(currentTreeNode.Text) + " so this hotkey is going to be blank");                    
                }
                else
                {
                    //MessageBox.Show("Found hotkey with ID: " + Utils.getStartingNumber(currentTreeNode.Text));
                    currentHotkey = veryTemp;
                }
                //MessageBox.Show("Deets: " + currentHotkey.ToString());

                // Let's start by just creating all the UI panel elements first


                // ----- Row 0 - Activates Watch or Trigger Dropdown -----
                Label typeLabel = new Label();
                typeLabel.AutoSize = true;
                typeLabel.Text = "Hotkey Activation Target"; // Resources.ResourceManager.GetString("valueTypeLabelString");
                typeLabel.Anchor = AnchorStyles.Right;
                typeLabel.Margin = padding;
                panel.Controls.Add(typeLabel, 0, row); // Control, Column, Row

                // Create and configure dropdown
                ComboBox targetCB = new ComboBox();
                targetCB.DropDownStyle = ComboBoxStyle.DropDownList;
                targetCB.Anchor = AnchorStyles.Right;
                targetCB.Dock = DockStyle.Fill;
                targetCB.Margin = padding;

                // Copy watch and trigger strings into array and add them to the ComboBox then select the one at index 0
                int elementCount = gameConfig.watchList.Count + gameConfig.triggerList.Count; // How many watches and triggers combined are there?
                string[] combinedWatchAndTriggerStringArray = new string[elementCount];       // Then that's how many strings we'll need

                int index = 0;
                foreach (Watch w in gameConfig.watchList)
                { 
                    combinedWatchAndTriggerStringArray[index] = "Watch-" + w.Id + "-" + w.Name;
                    ++index;
                }
                foreach (Trigger t in gameConfig.triggerList)
                {
                    combinedWatchAndTriggerStringArray[index] = "Trigger-" + t.Id + "-" + t.Name;
                    ++index;
                }
                targetCB.Items.AddRange(combinedWatchAndTriggerStringArray);

                panel.Controls.Add(targetCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 1 - Hotkey Activation Sequence List -----            
                Label activationSeqLabel = new Label();
                activationSeqLabel.AutoSize = true;
                activationSeqLabel.Text = "Hotkey Activation Key Sequence"; // Resources.ResourceManager.GetString("pointerListLabelString");
                activationSeqLabel.Anchor = AnchorStyles.Right;
                activationSeqLabel.Margin = padding;

                panel.Controls.Add(activationSeqLabel, 0, row); // Control, Column, Row

                TextBox activationTB = new TextBox();
                activationTB.Anchor = AnchorStyles.Right;
                activationTB.Dock = DockStyle.Fill;
                activationTB.Margin = padding;

                // Set text on activation sequence text box based on whether this is a new hotkey...
                if (currentTreeNode.Text.ToString().Contains("New Hotkey"))
                {
                    activationTB.Text = "Click on this text box and press your hotkey sequence";
                    targetCB.SelectedIndex = 0;
                }
                else // ...or whether it's an existing hotkey in which 
                {
                    activationTB.Text = currentHotkey.getActivationSequenceString();

                    //MessageBox.Show("Looking for: " + currentHotkey.getHotkeyTargetString());

                    targetCB.SelectedIndex = targetCB.FindString(currentHotkey.getHotkeyTargetString());


                    //MessageBox.Show("Looking for hotkey with id: " + Utils.getStartingNumber(currentTreeNode.Text));

                    //currentHotkey = Utils.getHotkeyWithId(Utils.getStartingNumber(currentTreeNode.Text));
                }



                panel.Controls.Add(activationTB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 5 - Enabled Flag -----            
                Label enabledLabel = new Label();
                enabledLabel.AutoSize = true;
                enabledLabel.Text = "Enabled"; // Resources.ResourceManager.GetString("activeLabelString");
                enabledLabel.Anchor = AnchorStyles.Right;
                enabledLabel.Margin = padding;

                panel.Controls.Add(enabledLabel, 0, row); // Control, Column, Row

                CheckBox activeCB = new CheckBox();
                activeCB.Checked = currentHotkey.enabled;
                activeCB.CheckedChanged += (object sender, EventArgs ea) => { currentHotkey.enabled = activeCB.Checked; };
                activeCB.Anchor = AnchorStyles.Right;
                activeCB.Dock = DockStyle.Fill;
                activeCB.Margin = padding;

                panel.Controls.Add(activeCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 7 - Delete Hotkey -----
                Button deleteHotkeyBtn = new Button();
                deleteHotkeyBtn.AutoSize = false;
                deleteHotkeyBtn.Text = Resources.ResourceManager.GetString("deleteWatchButtonString");
                deleteHotkeyBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Use bitwise-OR to anchor to top-right
                deleteHotkeyBtn.Margin = padding;
                deleteHotkeyBtn.BackColor = Color.Red;                

                panel.Controls.Add(deleteHotkeyBtn, 1, row); // Control, Column, Row
                row++;

                // ---------- Alright - now we'll handle all the live changes to UI elements ----------

                targetCB.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    //string currentTargetDescription = "COULD NOT FIND!";

                    // Grab the watch or trigger ID and set it on our current hotkey
                    currentHotkey.watchOrTriggerID = extractIntFromString(targetCB.SelectedItem.ToString());

                    // Set the current hotkey type based on the name of the currently selected activation item
                    if (targetCB.SelectedItem.ToString().StartsWith("Watch"))
                    {
                        currentHotkey.targetType = Hotkey.TargetType.ExecutesWatch;
                        //currentTargetDescription = Utils.getWatchWithId(currentHotkey.watchOrTriggerID).Description;
                    }
                    else
                    {
                        currentHotkey.targetType = Hotkey.TargetType.ExecutesTrigger;
                        //currentTargetDescription = Utils.getTriggerWithId(currentHotkey.watchOrTriggerID).Description;
                    }

                    // Update the current treenode's text with these details
                    currentTreeNode.Text = currentHotkey.Id + "-" + currentHotkey.getTargetTypeString() + "-" + currentHotkey.watchOrTriggerID + "-" + currentHotkey.getActivationSequenceString();

                    // Update the hotkey's name
                    currentHotkey.generateAndSetHotkeyName();
                };

                activationTB.KeyDown += (object sender, KeyEventArgs kea) =>
                {
                    // Get the modifier code
                    int modCode = currentHotkey.GenerateModifierCodeFromFlags(kea.Alt, kea.Control, kea.Shift);

                    // Get the activation code (activation key without modifiers)
                    // Note: The ~ operator provides a bitwise complement to the bit value, e.g. ~101 would be 010.
                    int actKey = (int)(kea.KeyData & ~Keys.Control & ~Keys.Shift & ~Keys.Alt);

                    // Set the modifier and activation codes on the hotkey
                    currentHotkey.SetKeyCombination(modCode, actKey);

                    // Apply the activation text to the textbox, hotkey node, and name of the hotkey. Note: Only the tree node text and the hotkey name have the id prepended to it.
                    activationTB.Text = currentHotkey.getActivationSequenceString();

                    //currentTreeNode.Text = currentHotkey.name = currentHotkey.Id.ToString() + "-" + currentHotkey.getActivationSequenceString();
                    currentTreeNode.Text = currentHotkey.generateAndSetHotkeyName();

                    // Prevent the KeyEventArgs from travelling any further
                    kea.SuppressKeyPress = true;
                };

                deleteHotkeyBtn.Click += (object sender, EventArgs ea) =>
                {
                    try
                    {
                        // Find the index of the hotkey and remove it from the hotkey list
                        int hotkeyIdToDelete = Utils.getStartingNumber(currentTreeNode.Text.ToString());
                        int tmpIndex = 0;                        
                        foreach (Hotkey h in gameConfig.hotkeyList)
                        {
                            if (hotkeyIdToDelete == h.Id)
                            {
                                break;
                            }
                            ++tmpIndex;
                        }
                        gameConfig.hotkeyList.RemoveAt(tmpIndex);

                        //MessageBox.Show("Just deleted hotkey at index: " + tmpIndex);
                        //int x = Utils.getNextHotkeyIndex(gameConfig.hotkeyList);
                        //MessageBox.Show("Next ID is: " + x);

                        // Remove the treenode associated with that watch and give focus to the treeview
                        gcTreeView.Nodes.Remove(currentTreeNode);
                        gcTreeView.Focus();
                    }
                    catch (ArgumentOutOfRangeException aoore)
                    {
                        MessageBox.Show(aoore.Message);
                        return;
                    }

                }; // End of delete button click handler

            }
            else // Didn't recognise tree node tag? Moan!
            {   
                MessageBox.Show( Resources.ResourceManager.GetString("badTreeNodeTagWarningString") + currentTreeNode.Tag.ToString() );
            }

            // Resume the layout logic of the panel and make it visible
            panel.ResumeLayout();
            panel.Visible = true;

        } // End of gcTreeView_AfterSelect method       

        // Method to extract the first integer from the name of a Hotkey
        // Reminder: Hotkey names are things like "1-Ctrl+Shift+T" etc.
        private int extractIntFromString(string s)
        {   
            string intString = "";
            bool foundDigits = false;
            for (int i = 0; i < s.Length; ++i)
            {
                if ( Char.IsDigit(s[i]) )
                {
                    intString += s[i];
                    foundDigits = true;
                }
                else
                {
                    if (foundDigits) { break; }
                }
            }

            if (intString.Length == 0)
            {
                MessageBox.Show("Error: Could not extract int from string =(");
                return 0;
            }

            return int.Parse(intString);
        }

        private void addWatchButton_Click(object senderender, EventArgs e)
        {
            currentUILabel.Text = Resources.ResourceManager.GetString("addWatchLabelString");

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);
            panel.Padding = padding;

            currentWatch = new Watch();
            currentWatch.Id = Utils.getNextWatchIndex(gameConfig.watchList);
            currentWatch.Name = Resources.ResourceManager.GetString("changeMeString");
            currentWatch.Description = Resources.ResourceManager.GetString("changeMeString");
            currentWatch.PointerList = new List<string>();
            currentWatch.valueType = Watch.ValueType.IntType;
            gameConfig.watchList.Add(currentWatch);

            // Add a new watch entry as a child node to the "Watches" node
            TreeView tv = this.gcTreeView;
            TreeNode watchesNode = Utils.FindNodeWithText(tv, Resources.ResourceManager.GetString("watchesString"));
            tv.BeginUpdate();
            currentTreeNode = watchesNode.Nodes.Add( currentWatch.Id + Resources.ResourceManager.GetString("newWatchString") );
            currentTreeNode.Tag = Resources.ResourceManager.GetString("watchString");
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        private void addTriggerButton_Click(object senderender, EventArgs e)
        {
            currentUILabel.Text = Resources.ResourceManager.GetString("addTriggerLabelString");

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);
            panel.Padding = padding;

            // Create a new trigger and add it to the list
            currentTrigger = new Trigger();
            currentTrigger.Id = Utils.getNextTriggerIndex(gameConfig.triggerList);
            gameConfig.triggerList.Add(currentTrigger);

            // Add a new Trigger entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode triggersNode = Utils.FindNodeWithText( tv, Resources.ResourceManager.GetString("triggersString") );
            tv.BeginUpdate();
            currentTreeNode = triggersNode.Nodes.Add( currentTrigger.Id + Resources.ResourceManager.GetString("newTriggerString") );
            currentTreeNode.Tag = Resources.ResourceManager.GetString("triggerString");
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        // Method to clone the currently selected trigger
        private void cloneTriggerButton_Click(object senderender, EventArgs e)
        {
            currentUILabel.Text = Resources.ResourceManager.GetString("cloneTriggerLabelString");

            // Ensure the user has a Trigger selected before starting the clone process
            if ( !currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("triggerString")) )
            {
                MessageBox.Show( Resources.ResourceManager.GetString("triggerCloneWarningString") );
                return;
            }

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);
            panel.Padding = padding;

            // Create a new trigger and use the copy constructor to make it a deep-copy of the existing current trigger
            currentTrigger = new Trigger(currentTrigger);

            // Update the id to the next available free id value and add the trigger to the trigger list
            currentTrigger.Id = Utils.getNextTriggerIndex(gameConfig.triggerList);
            gameConfig.triggerList.Add(currentTrigger);

            // Add a new trigger entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode triggersNode = Utils.FindNodeWithText( tv, Resources.ResourceManager.GetString("triggersString") );
            tv.BeginUpdate();
            currentTreeNode = triggersNode.Nodes.Add(currentTrigger.Id + "-" + currentTrigger.Name);
            currentTreeNode.Tag = Resources.ResourceManager.GetString("triggerString");
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        // Method to clone the currently selected watch
        private void cloneWatchButton_Click(object senderender, EventArgs e)
        {
            currentUILabel.Text = Resources.ResourceManager.GetString("cloneWatchLabelString");

            // Ensure the user has a Trigger selected before starting the clone process
            if ( !currentTreeNode.Tag.ToString().Equals(Resources.ResourceManager.GetString("watchString")) ) 
            {
                MessageBox.Show( Resources.ResourceManager.GetString("cloneWatchWarningString") );
                return;
            }

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);
            panel.Padding = padding;

            // Create a new trigger and use the one parameter Trigger constructor to make it a deep-copy of the existing current trigger
            currentWatch = new Watch(currentWatch);

            currentWatch.Id = Utils.getNextWatchIndex(gameConfig.watchList);
            gameConfig.watchList.Add(currentWatch);

            // Add a new watch entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode triggersNode = Utils.FindNodeWithText(tv, Resources.ResourceManager.GetString("watchesString") );
            tv.BeginUpdate();
            currentTreeNode = triggersNode.Nodes.Add(currentWatch.Id + "-" + currentWatch.Name);
            currentTreeNode.Tag = Resources.ResourceManager.GetString("watchString");
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        // Method to read, display and output the config notes via Tolk on click
        private void readConfigNotesButton_Click(object sender, EventArgs e)
        {
            // We'll hide the picture box and replace it with the config notes
            pictureBox1.Hide();
            
            // To display the config notes we have to load the config
            loadGameConfig();
                       
            // Set the text on the textbox
            configNotesTB.Text = gameConfig.Description;

            // If the config doesn't have a description then add some boilerplate text informing user of this, and how they can add details should they want to.
            if (configNotesTB.Text.Length == 0)
            {
                configNotesTB.Text = "No config notes exist in the config file. You can add some by editing the config description in the Edit Config tab and then saving the config.";
            }

            // If the config notes textbox hasn't already been added to the main page tab then add it
            if (!mainTabPage.Contains(configNotesTB))
            {
                mainTabPage.Controls.Add(configNotesTB);
            }

            // Output the config notes to screenreader via tolk, interupting any currently spoken output
            Tolk.Output(configNotesTB.Text, true);

            // Display the config notes textbox
            //configNotesTB.Show();
        }

        private void addHotkeyButton_Click(object senderender, EventArgs e)
        {
            currentUILabel.Text = "Add Hotkey"; // Resources.ResourceManager.GetString("addTriggerLabelString");

            // If there are no watches or triggers then moan and bail
            if (gameConfig.watchList.Count == 0 && gameConfig.triggerList.Count == 0)
            {
                MessageBox.Show("Add at least one Watch or Trigger before attempting to assign a hotkey to one.");
                return;
            }

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);
            panel.Padding = padding;

            // Create a new Hotkey and set its Id to be the next available
            currentHotkey = new Hotkey();            
            currentHotkey.Id = Utils.getNextHotkeyIndex(gameConfig.hotkeyList);
            //MessageBox.Show("Adding a new hotkey with id: " + Utils.getNextHotkeyIndex(gameConfig.hotkeyList));

            // Set the hotkey's target type and watchOrTriggerID to be the first watch or trigger
            if (gameConfig.watchList.Count > 0)
            {
                currentHotkey.targetType = Hotkey.TargetType.ExecutesWatch;
                currentHotkey.watchOrTriggerID = gameConfig.watchList[0].Id;
            }
            else
            {
                currentHotkey.targetType = Hotkey.TargetType.ExecutesTrigger;
                currentHotkey.watchOrTriggerID = gameConfig.triggerList[0].Id;
            }
            
            
            // Note: Activation key will be Keys.None as an int (e.g. 0) by default

            currentHotkey.enabled = true; // If you're creating a new hotkey we'll assume you want it enabled!

            // Add the hotkey to the hotkey list
            gameConfig.hotkeyList.Add(currentHotkey);

            // Add a new Trigger entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode hotkeysNode = Utils.FindNodeWithText(tv, "Hotkeys"); // Resources.ResourceManager.GetString("triggersString"));
            tv.BeginUpdate();
                currentTreeNode = hotkeysNode.Nodes.Add(currentHotkey.Id + "-New Hotkey"); // Trigger.Id + Resources.ResourceManager.GetString("newTriggerString"));
                currentTreeNode.Tag = "Hotkey"; // Resources.ResourceManager.GetString("triggerString");
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }
    } // End of MainForm partial class

} // End of namespace