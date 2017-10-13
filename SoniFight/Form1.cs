using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using au.edu.federation.SoniFight.Properties;

namespace au.edu.federation.SoniFight
{
    public partial class MainForm : Form
    {
        // Used in window title
        private static string formTitle = "SoniFight v1.0 "; // Do not localise this - SoniFight is SoniFight.

        // Are we current running a gameconfig? Initially no!
        bool running = false;

        // The GameConfig object represented by the UI
        public static GameConfig gameConfig;

        // String versions of the type of data watches can use
        string[] dataTypesArray = { Resources.ResourceManager.GetString("integerString"),     // "Integer"
                                    Resources.ResourceManager.GetString("shortString"),       // "Short"
                                    Resources.ResourceManager.GetString("longString"),        // "Long"
                                    Resources.ResourceManager.GetString("floatString"),       // "Float"
                                    Resources.ResourceManager.GetString("doubleString"),      // "Double"
                                    Resources.ResourceManager.GetString("booleanString"),     // "Boolean"
                                    Resources.ResourceManager.GetString("stringUTF8String"),  // "String (UTF-8)"
                                    Resources.ResourceManager.GetString("stringUTF16String")  // "String (UTF-16)"
                                  };

        // String versions of the comparison types we can use
        string[] comparisonTypesArray = { Resources.ResourceManager.GetString("equalToString"),                   // "Equal To"
                                          Resources.ResourceManager.GetString("lessThanString"),                  // "Less Than"
                                          Resources.ResourceManager.GetString("lessThanOrEqualToString"),         // "Less Than Or Equal To"
                                          Resources.ResourceManager.GetString("greaterThanString"),               // "Greater Than"
                                          Resources.ResourceManager.GetString("greaterThanOrEqualToString"),      // "Greater Than Or Equal To"
                                          Resources.ResourceManager.GetString("notEqualToString"),                // "Not Equal To"
                                          Resources.ResourceManager.GetString("changedString"),                   // "Changed"
                                          Resources.ResourceManager.GetString("distanceVolumeDescendingString"),  // "Distance - Volume Descending (Cont. Only)"
                                          Resources.ResourceManager.GetString("distanceVolumeAscendingString"),   // "Distance - Volume Ascending (Cont. Only)"
                                          Resources.ResourceManager.GetString("distancePitchDescendingString"),   // "Distance - Pitch Descending (Cont. Only)"
                                          Resources.ResourceManager.GetString("distancePitchAscendingString")     // "Distance - Pitch Ascending (Cont. Only)"
                                        };

        // String versions of the trigger types
        string[] triggerTypesArray = { Resources.ResourceManager.GetString("triggerTypeNormalString"),     // "Normal",
                                       Resources.ResourceManager.GetString("triggerTypeContinuousString"), // "Continuous"
                                       Resources.ResourceManager.GetString("triggerTypeModifierString")    // "Modifier"
                                     };

        // String versions of the trigger allowance types
        string[] allowanceTypesArray = { Resources.ResourceManager.GetString("allowanceTypeAnyString"),     // "Any"
                                         Resources.ResourceManager.GetString("allowanceTypeInGameString"),  // "In-Game"
                                         Resources.ResourceManager.GetString("allowanceTypeInMenuString")   // "In-Menu"
                                       };

        // Initial config dropdown index
        static int selectedConfigDropdownIndex = 0;

        // Details panel settings
        private Padding padding = new System.Windows.Forms.Padding(5);

        // Flag for when to create a new config rather than attempt to load one from a config folder on tab index changed
        bool creatingNewConfig = false;

        // Used to keep track of what node is currently selected in the treeview...
        static TreeNode currentTreeNode;

        // ...which will typically relate to a specific watch or trigger, if it's not the standard GameConfig settings or a brief text description.
        static Watch currentWatch;
        static Trigger currentTrigger;

        // Constructor
        public MainForm()
        {
            InitializeComponent();
            gameConfig = new GameConfig();

            // Initially we aren't running so we add that the current status is stopped to the window title
            this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");

            populateMainConfigsBox();
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
                gameConfig.PollSleepMS = 10;
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

            // ...then change to the Edit tab!
            this.tabControl.SelectedIndex = 1;
        }

        // Method to load the config and start polling
        private void runConfig_Click(object senderender, EventArgs e)
        {
            if (!running)
            {
                Program.irrKlang = new SoundPlayer();

                // Inflate game config from XML file
                string pathToConfig = ".\\Configs\\" + MainForm.gameConfig.ConfigDirectory + "\\config.xml";
                Console.WriteLine("About to read config: " + pathToConfig);
                gameConfig = Utils.ReadFromXmlFile<GameConfig>(pathToConfig);

                if (gameConfig == null)
                {
                    string s1 = Resources.ResourceManager.GetString("deserialiseFailString1");
                    string s2 = Resources.ResourceManager.GetString("deserialiseFailString2");
                    MessageBox.Show(s1 + gameConfig.ConfigDirectory + s2);
                    return;
                }

                // IMPORTANT: Because loading a GameConfig object from file overwrites all properties, and
                //            the ConfigDirectory is not stored in the object, we need to reset it to the
                //            directory name via the selection in the configsComboBox dropdown menu!
                MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);

                // Validate and activate our gameconfig
                gameConfig.Valid = gameConfig.validate();
                gameConfig.Active = gameConfig.activate();

                // If we have a valid, active config and we're not already running then start our sonification background worker,
                // which calls the 'performSonification' method.
                if (gameConfig.Valid && gameConfig.Active)
                {
                    Program.sonificationBGW.RunWorkerAsync();
                    this.Text = formTitle + Resources.ResourceManager.GetString("statusRunningString") + gameConfig.ConfigDirectory;
                    running = true;
                }
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
            Utils.WriteToXmlFile(configPath, gameConfig);
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
                    // This sets cancellation to pending, which we handle in the associated doWork method
                    // to actually perform the cancellation.
                    Program.sonificationBGW.CancelAsync();

                    Console.WriteLine( Resources.ResourceManager.GetString("sonificationStoppedString") );

                    this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");
                    running = false;

                    Thread.Sleep(500);
                }

                if (creatingNewConfig)
                {
                    //MessageBox.Show("Creating new config via flag!");
                    creatingNewConfig = false;
                }
                else // Loading existing config?
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
            else // We must be on index 0, and so we should update the GameConfig ComboBox incase the user has created a new config they want to run
            {
                populateMainConfigsBox();
            }

        } // End of tabControl_SelectedIndexChanged method

        // Stop button handler to stop the sonification background worker
        private void stopConfigButton_Click(object senderender, EventArgs e)
        {
            // This sets cancellation to pending, which we handle in the associated doWork method
            // to actually perform the cancellation.
            GameConfig.processConnectionBGW.CancelAsync();
            Program.sonificationBGW.CancelAsync();
            this.Text = formTitle + Resources.ResourceManager.GetString("statusStoppedString");
            running = false;
            Thread.Sleep(500);

            Program.irrKlang.UnloadAllSamples();
        }

        // Method to refresh the main config selection dropdown menu
        private void refreshButton_Click(object senderender, EventArgs e)
        {
            populateMainConfigsBox();
        }

        private void gcTreeView_AfterSelect(object senderender, TreeViewEventArgs tvea)
        {
            // Get the panel, clear it and set some layout properties
            TableLayoutPanel panel = this.gcPanel;
            Utils.clearTableLayoutPanel(panel);
            panel.Padding = padding;
            panel.AutoSize = true;
            panel.Anchor = AnchorStyles.Right;
            panel.Dock = DockStyle.Fill;

            // Update the current node to be the node which triggered this method
            currentTreeNode = tvea.Node;

            /* NOTE: This section was previously done with a switch statement, but they require constant values, and localised values aren't constant, hence the change to if/then/else. */

            // We always start at rown zero
            int row = 0;

            // --- Recreate the panel based on the current node type ---

            // Recreate panel as GameConfig panel
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

                // ----- Row 7 - Config description -----                
                Label descLabel = new Label();
				descLabel.AutoSize = true;
				descLabel.Text = Resources.ResourceManager.GetString("descriptionLabelString");
				descLabel.Anchor = AnchorStyles.None;
				descLabel.Margin = padding;

                panel.SetColumnSpan(descLabel, 2); // Span both colums 

                panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

				TextBox descTB = new TextBox();
				descTB.Multiline = true;
				descTB.Height = descTB.Font.Height * 15 + padding.Horizontal; // Set height to be enough for 15 lines

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
                    if (gameConfig.triggerList[loop].WatchOneId == currentWatch.Id)
                    {
                        s += Convert.ToString(gameConfig.triggerList[loop].Id) + ", ";
                        foundTriggerUsing = true;
                    }
                }

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

                // Prior declarations of UI elements so we can modify or disable them if required based on other trigger settings
                ComboBox compTypeCB = new ComboBox();
                TextBox watch1TB = new TextBox();
                Label secondaryIdLabel = new Label();
                TextBox secondaryIdTB = new TextBox();
                TextBox valueTB = new TextBox();
                TextBox sampleFilenameTB = new TextBox();
                Button sampleFilenameButton = new Button();
                TextBox sampleVolumeTB = new TextBox();
                TextBox sampleSpeedTB = new TextBox();
                Label tolkLabel = new Label();
                CheckBox tolkCheckbox = new CheckBox();

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

                // -----  Row 3 - Trigger type (Once, Recurring, Continuous) -----
                Label triggerTypeLabel = new Label();
                triggerTypeLabel.AutoSize = true;
                triggerTypeLabel.Text = Resources.ResourceManager.GetString("triggerTypeLabelString");
                triggerTypeLabel.Anchor = AnchorStyles.Right;
                triggerTypeLabel.Margin = padding;

                panel.Controls.Add(triggerTypeLabel, 0, row); // Control, Column, Row

                ComboBox triggerTypeCB = new ComboBox();
                triggerTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
                triggerTypeCB.Items.AddRange(triggerTypesArray);
                triggerTypeCB.SelectedIndex = Utils.GetIntFromTriggerType(currentTrigger.triggerType);
                triggerTypeCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                {
                    currentTrigger.triggerType = Utils.GetTriggerTypeFromInt(triggerTypeCB.SelectedIndex);

                    // If we're a modifier trigger we disable the sample name textbox, otherwise it's left or made active
                    if (currentTrigger.triggerType == Trigger.TriggerType.Modifier)
                    {
                        sampleFilenameTB.Enabled = false;
                        sampleFilenameButton.Enabled = false;
                    }
                    else
                    {
                        sampleFilenameTB.Enabled = true;
                        sampleFilenameButton.Enabled = true;
                    }

                    // Display the appropriate label for the 'watch2' field and set whether the tolk checkbox is active or not
                    switch (currentTrigger.triggerType)
                    {
                        case Trigger.TriggerType.Normal:
                            secondaryIdLabel.Text = Resources.ResourceManager.GetString("dependentTriggerIdLabelString");
                            tolkCheckbox.Enabled = true;
                            break;
                        case Trigger.TriggerType.Continuous:
                            secondaryIdLabel.Text = Resources.ResourceManager.GetString("watch2IdLabelString");
                            tolkCheckbox.Enabled = false;
                            break;
                        case Trigger.TriggerType.Modifier:
                            secondaryIdLabel.Text = Resources.ResourceManager.GetString("continuousTriggerIdLabelString");
                            tolkCheckbox.Enabled = false;
                            break;
                    }
                };
                
                // Set the current trigger type based on the selected index of the trigger type dropdown
                currentTrigger.triggerType = Utils.GetTriggerTypeFromInt(triggerTypeCB.SelectedIndex);

                triggerTypeCB.Tag = "triggerTypeCB";
                triggerTypeCB.Anchor = AnchorStyles.Left;
                triggerTypeCB.Dock = DockStyle.Fill;
                triggerTypeCB.Margin = padding;

                panel.Controls.Add(triggerTypeCB, 1, row); // Control, Column, Row
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

                watch1TB.Text = currentTrigger.WatchOneId.ToString();
                watch1TB.Anchor = AnchorStyles.Left;
                watch1TB.Dock = DockStyle.Fill;
                watch1TB.Margin = padding;

                watch1TB.TextChanged += (object sender, EventArgs ea) =>
                {
                    int x;
                    bool result = Int32.TryParse(watch1TB.Text, out x);
                    if (result)
                    {
                        currentTrigger.WatchOneId = x;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(watch1TB.Text.ToString()))
                        {
                            MessageBox.Show( Resources.ResourceManager.GetString("watch1IdWarningString") );
                        }
                        else // Field empty? Invalidate it so we can catch it in the save section
                        {
                            currentTrigger.WatchOneId = -1;
                        }
                    }
                };

                panel.Controls.Add(watch1TB, 1, row); // Control, Column, Row
                row++;

                // Row 6 - Watch ID 2 - this is used for dependent triggers for normal triggers, secondary watch for continuous triggers and continuous trigger id for modifier triggers
                secondaryIdLabel.AutoSize = true;

                // Display the appropriate label for the 'watch2' field
                switch (currentTrigger.triggerType)
                {
                    case Trigger.TriggerType.Normal:
                        secondaryIdLabel.Text = Resources.ResourceManager.GetString("secondaryIdLabelDependentIdString");
                        break;
                    case Trigger.TriggerType.Continuous:
                        secondaryIdLabel.Text = Resources.ResourceManager.GetString("secondaryIdLabelWatch2IdString");
                        break;
                    case Trigger.TriggerType.Modifier:
                        secondaryIdLabel.Text = Resources.ResourceManager.GetString("secondaryIdLabelContinuousTriggerIdString");
                        break;
                }

                secondaryIdLabel.Anchor = AnchorStyles.Right;
                secondaryIdLabel.Margin = padding;
                panel.Controls.Add(secondaryIdLabel, 0, row); // Control, Column, Row

                secondaryIdTB.Text = currentTrigger.SecondaryId.ToString();
                secondaryIdTB.Anchor = AnchorStyles.Left;
                secondaryIdTB.Dock = DockStyle.Fill;
                secondaryIdTB.Margin = padding;

                secondaryIdTB.TextChanged += (object sender, EventArgs ea) =>
                {
                    int x;
                    bool result = Int32.TryParse(secondaryIdTB.Text, out x);
                    if (result)
                    {
                        currentTrigger.SecondaryId = x;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(secondaryIdTB.Text.ToString()))
                        {
                            MessageBox.Show( Resources.ResourceManager.GetString("secondaryIdWarningString") );
                        }
                        else // Field empty? Invalidate it so we can catch it in the save section
                        {
                            currentTrigger.SecondaryId = -1;
                        }
                    }
                };

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

                // ----- Row 8 - Trigger sample row -----
                Label sampleFilenameLabel = new Label();
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

                // Disable sample field if we're the clock trigger
                if (currentTrigger.IsClock) { sampleFilenameTB.Enabled = false; }

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

                // If we're the clock disable the sample textbox and button + the value textbox (unused for clock triggers - criteria is 'did it change?')
                if (currentTrigger.UseTolk)
                {
                    sampleFilenameButton.Enabled = false;
                    sampleVolumeTB.Enabled = false;
                    sampleSpeedTB.Enabled = false;
                    sampleFilenameLabel.Text = Resources.ResourceManager.GetString("screenReaderTextLabelString");
                }
                else // Not using tolk? Enable filename button, volume and speed
                {
                    sampleFilenameButton.Enabled = true;
                    sampleVolumeTB.Enabled = true;
                    sampleSpeedTB.Enabled = true;
                    sampleFilenameLabel.Text = Resources.ResourceManager.GetString("sampleFilenameLabelString");
                }

                // Tolk is only available for normal triggers
                if (currentTrigger.triggerType != Trigger.TriggerType.Normal)
                {
                    tolkCheckbox.Enabled = false;
                }
                else
                {
                    tolkCheckbox.Enabled = true;
                }

                tolkCheckbox.CheckedChanged += (object sender, EventArgs ea) => {
                    // Update the new isClock status on our trigger
                    currentTrigger.UseTolk = tolkCheckbox.Checked;

                    // If we're the clock disable the sample textbox and button + the value textbox (unused for clock triggers - criteria is 'did it change?')
                    if (currentTrigger.UseTolk)
                    {
                        sampleFilenameButton.Enabled = false;
                        sampleVolumeTB.Enabled = false;
                        sampleSpeedTB.Enabled = false;
                        sampleFilenameLabel.Text = Resources.ResourceManager.GetString("screenReaderTextLabelString");
                    }
                    else // Not using tolk? Enable filename button, volume and speed
                    {
                        sampleFilenameButton.Enabled = true;
                        sampleVolumeTB.Enabled = true;
                        sampleSpeedTB.Enabled = true;
                        sampleFilenameLabel.Text = Resources.ResourceManager.GetString("sampleFilenameLabelString");
                    }
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

                    file.InitialDirectory = ".\\Configs\\" + gameConfig.ConfigDirectory; // Open dialog in gameconfig directory

                    if (file.ShowDialog() == DialogResult.OK)
                    {
                        // Note: Filename gives you the full path to the file, SafeFilename gives you ONLY the filename including extension, which is what we want
                        currentTrigger.SampleFilename = file.SafeFileName;
                        sampleFilenameTB.Text = file.SafeFileName;
                    }
                };
                sampleSelectionPanel.Controls.Add(sampleFilenameButton);

                // If we're a modifier trigger we disable the sample name textbox and sample selection button, otherwise it's left or made active
                if (currentTrigger.triggerType == Trigger.TriggerType.Modifier)
                {
                    sampleFilenameTB.Enabled = false;
                    sampleFilenameButton.Enabled = false;
                }
                else
                {
                    sampleFilenameTB.Enabled = true;
                    sampleFilenameButton.Enabled = true;
                }

                if (currentTrigger.UseTolk)
                {
                    sampleFilenameButton.Enabled = false;
                }

                // Now we can add the sample selection panel to the cell!
                panel.Controls.Add(sampleSelectionPanel, 1, row);
                row++;

                // ----- Row 9 - Trigger sample volume ---
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

                // Disable sample volume field if we're the clock trigger or using tolk
                if (currentTrigger.IsClock || currentTrigger.UseTolk)
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

                // ----- Row 10 - Trigger sample rate ---
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

                // Disable sample speed field if we're not the clock trigger
                if (currentTrigger.IsClock || currentTrigger.UseTolk) { sampleSpeedTB.Enabled = false; }

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

                // ----- Row 11 - Active Flag -----            
                Label isClockLabel = new Label();
                isClockLabel.AutoSize = true;
                isClockLabel.Text = Resources.ResourceManager.GetString("isClockLabelString");
                isClockLabel.Anchor = AnchorStyles.Right;
                isClockLabel.Margin = padding;
                panel.Controls.Add(isClockLabel, 0, row); // Control, Column, Row

                CheckBox isClockCB = new CheckBox();
                isClockCB.Checked = currentTrigger.IsClock;

                // If we're the clock disable the sample textbox and button + the value textbox (unused for clock triggers - criteria is 'did it change?')
                if (currentTrigger.IsClock)
                {
                    sampleFilenameTB.Enabled = false;
                    sampleFilenameButton.Enabled = false;
                    valueTB.Enabled = false;
                    sampleVolumeTB.Enabled = false;
                    sampleSpeedTB.Enabled = false;
                    tolkCheckbox.Enabled = false;
                }
                else // We are not the clock trigger
                {
                    // Re-enable the value checkbox
                    valueTB.Enabled = true;

                    // If we are NOT using tolk then we enable volume, speed and file buttons
                    if (!currentTrigger.UseTolk)
                    {
                        sampleVolumeTB.Enabled = true;
                        sampleSpeedTB.Enabled = true;

                        // Modifier triggers do not use samples, so we do not enable the sample filename text box or button
                        if (currentTrigger.triggerType != Trigger.TriggerType.Modifier)
                        {
                            sampleFilenameTB.Enabled = true;
                            sampleFilenameButton.Enabled = true;
                        }
                    }
                    else // We ARE using tolk, so the volume, speed and file button should NOT be active
                    {
                        sampleVolumeTB.Enabled = false;
                        sampleSpeedTB.Enabled = false;
                        sampleFilenameButton.Enabled = false;
                    }
                }

                isClockCB.CheckedChanged += (object sender, EventArgs ea) => {
                    // Update the new isClock status on our trigger
                    currentTrigger.IsClock = isClockCB.Checked;

                    // If we're the clock disable the sample textbox and button + the value textbox (unused for clock triggers - criteria is 'did it change?')
                    if (currentTrigger.IsClock)
                    {
                        sampleFilenameTB.Enabled = false;
                        sampleFilenameButton.Enabled = false;
                        valueTB.Enabled = false;
                        sampleVolumeTB.Enabled = false;
                        sampleSpeedTB.Enabled = false;
                        tolkCheckbox.Enabled = false;
                    }
                    else // This is not the clock trigger
                    {
                        // Re-enable the secondary Id textbox along with the sample volume and speed textboxes
                        valueTB.Enabled = true;

                        // Re-enabled tolk checkbox
                        tolkCheckbox.Enabled = true;

                        // // If we are NOT using tolk then we enable volume, speed and file buttons
                        if (!currentTrigger.UseTolk)
                        {
                            sampleVolumeTB.Enabled = true;
                            sampleSpeedTB.Enabled = true;

                            // Modifier triggers do not use samples, so we do not enable the sample filename text box or button
                            if (currentTrigger.triggerType != Trigger.TriggerType.Modifier)
                            {
                                sampleFilenameTB.Enabled = true;
                                sampleFilenameButton.Enabled = true;
                            }
                        }
                        else // We ARE using tolk, so the volume, speed and file button should NOT be active
                        {
                            sampleVolumeTB.Enabled = false;
                            sampleSpeedTB.Enabled = false;
                            sampleFilenameButton.Enabled = false;
                        }
                    }
                };

                isClockCB.Anchor = AnchorStyles.Right;
                isClockCB.Dock = DockStyle.Fill;
                isClockCB.Margin = padding;

                panel.Controls.Add(isClockCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 12 - Allowance Type (Any / InGame / InMenu) row -----
                Label triggerAllowanceLabel = new Label();
                triggerAllowanceLabel.AutoSize = true;
                triggerAllowanceLabel.Text = Resources.ResourceManager.GetString("allowanceTypeLabelString");
                triggerAllowanceLabel.Anchor = AnchorStyles.Right;
                triggerAllowanceLabel.Margin = padding;
                panel.Controls.Add(triggerAllowanceLabel, 0, row); // Allowance, Column, Row

                ComboBox triggerAllowanceCB = new ComboBox();
                triggerAllowanceCB.DropDownStyle = ComboBoxStyle.DropDownList;
                triggerAllowanceCB.Items.AddRange(allowanceTypesArray);
                triggerAllowanceCB.SelectedIndex = Utils.GetIntFromAllowanceType(currentTrigger.allowanceType);

                triggerAllowanceCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                {
                    currentTrigger.allowanceType = Utils.GetAllowanceTypeFromInt(triggerAllowanceCB.SelectedIndex);
                };
                
                triggerAllowanceCB.Anchor = AnchorStyles.Left;
                triggerAllowanceCB.Dock = DockStyle.Fill;
                triggerAllowanceCB.Margin = padding;                                

                if (currentTrigger.IsClock)
                {
                    triggerAllowanceCB.Enabled = false;
                }
                else
                {
                    triggerAllowanceCB.Enabled = true;
                }

                panel.Controls.Add(triggerAllowanceCB, 1, row); // Control, Column, Row
                row++;

                // ----- Row 13 - Active Flag -----            
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

                // ----- Row 14 - Delete Trigger -----
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
                        // Remove the watch we're modifying
                        gameConfig.triggerList.RemoveAt(triggerIndex);

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

                panel.Controls.Add(deleteTriggerBtn, 1, row); // Control, Column, Row
                row++;
            }
            else // Didn't recognise tree node tag? Moan!
            {   
                MessageBox.Show( Resources.ResourceManager.GetString("badTreeNodeTagWarningString") + currentTreeNode.Tag.ToString() );
            } // End of switch

            panel.Visible = true;

        } // End of gcTreeView_AfterSelect method

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

            // Add a new watch entry as a child node to the "Triggers" node
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

            // Add a new watch entry as a child node to the "Triggers" node
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
        
    } // End of MainForm partial class

} // End of namespace