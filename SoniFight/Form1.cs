using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace SoniFight
{
    public partial class MainForm : Form
    {
        private static string formTitle = "SoniFight v0.6";

        bool running = false;

        public static GameConfig gameConfig;

        string[] dataTypesArray = { "Integer", "Short", "Float", "Double", "Boolean", "String (UTF-16)" };

        string[] comparisonTypesArray = { "EqualTo", "LessThan", "LessThanOrEqualTo", "GreaterThan", "GreaterThanOrEqualTo", "NotEqualTo", "Changed", "DistanceBetween" };

        string[] triggerTypesArray = { "Once", "Recurring", "Continuous" };

        string[] controlTypesArray = { "Normal", "Reset", "Mute" };

        string[] allowanceTypesArray = { "Any", "In game", "In menu" };

        static int selectedConfigDropdownIndex = 0;

        private int detailsPanelX = 340;
        private int detailsPanelY = 50;
        private int detailsPanelWidth = 400;
        private Padding padding = new System.Windows.Forms.Padding(10);

        // Flag for when to create a new config rather than attempt to load one from a config folder on tab index changed
        static bool creatingNewConfig = false;

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

            this.Text = formTitle + " Status: Stopped";
        }

        // Setup the form
        private void MainForm_Load(object sender, EventArgs e)
        {   
            populateMainConfigsBox();
        }

        // Read all the directories in the Configs folder and use each directory text as an item in the main GameConfig ComboBox
        private void populateMainConfigsBox()
        {
            configsComboBox.Items.Clear();

            string[] subdirectoryArray = null;
            string configPath = ".\\Configs";
            try
            {
                subdirectoryArray = System.IO.Directory.GetDirectories(configPath);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                MessageBox.Show("Error: Configs directory does not exist in same folder as SoniFight executable - click OK to create folder.");
                System.IO.Directory.CreateDirectory(configPath);
                try
                {
                    subdirectoryArray = System.IO.Directory.GetDirectories(configPath);
                }
                catch (DirectoryNotFoundException dnfe2)
                {
                    MessageBox.Show("Error: Configs directory creation failed - please manually create a directory called \"Configs\" in the same folder as the SoniFight executable.");
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
                string s = "No configs found in Configs directory!";
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
                //configsComboBox.SelectedIndex = 0;
                

                // Re-select the previously selected index
                this.configsComboBox.SelectedIndex = selectedConfigDropdownIndex;

                MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);
            } 
        }

        // Update selected config string (we'll use this string to load the 'config.xml' file from within this directory.
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {            
            // Set the config directory to the text on the dropdown
            // Note: The config is activated when the 'activateAndValidate' method is called
            MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);

            //Console.WriteLine("Selected index text / config dir is now: " + MainForm.gameConfig.ConfigDirectory);

            // Update our configs selected index so we can move back to it if the user goes from the edit to the main tabs
            selectedConfigDropdownIndex = this.configsComboBox.SelectedIndex;

            SoundPlayer.UnloadAllSamples();     
        }

        private void exitButton_Click_1(object sender, EventArgs e)
        {
            // Close this form   
            this.Close();

            // Note: Once this happens SoundPlayer.ShutDown() will be called from the main method because we've been stuck in this form loop up until then.
        }

        private void createNewConfigButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("CreateNewConfig button was clicked!");

            creatingNewConfig = true;

            // Reset the static GameConfig object so that it gets re-created when we hit the Edit tab...
            gameConfig.ConfigDirectory = "";
            gameConfig.Description = "";
            gameConfig.ProcessName = "";
            gameConfig.PollSleepMS = 10;
            gameConfig.watchList.Clear();
            gameConfig.triggerList.Clear();

            // ...then change to the Edit tab!
            this.tabControl.SelectedIndex = 1;
        }

        // Method to load the config and start polling
        private void runConfig_Click(object sender, EventArgs e)
        {
            // Inflate game config from XML file
            string pathToConfig = ".\\Configs\\" + MainForm.gameConfig.ConfigDirectory + "\\config.xml";
            gameConfig = Utils.ReadFromXmlFile<GameConfig>(pathToConfig);

            if (gameConfig == null) { MessageBox.Show("After loading gameconfig object - it's still null!");  }

            // IMPORTANT: Because loading a GameConfig object from file overwrites all properties, and
            //            the ConfigDirectory is not stored in the object, we need to reset it to the
            //            directory name via the selection in the configsComboBox dropdown menu!
            MainForm.gameConfig.ConfigDirectory = this.configsComboBox.GetItemText(this.configsComboBox.SelectedItem);
            
            // Validate and activate our gameconfig
            bool configIsValid = gameConfig.validate();
            bool configCanBeActivated = gameConfig.activate();

            // Start our sonification background worker, which calls the 'performSonification' method
            if (configIsValid && configCanBeActivated)
            {
                Program.sonificationBGW.RunWorkerAsync();

                this.Text = formTitle + " Status: Running. Config: " + gameConfig.ConfigDirectory;
                running = true;
            }

        } // End of runConfig_Click method

        private void saveConfig_Click(object sender, EventArgs e)
        {
            Console.WriteLine("--- Validating GameConfig ---");

            //Console.WriteLine("Before validation config dir is: " + gameConfig.ConfigDirectory);

            bool configIsValid = gameConfig.validate();
            if (!configIsValid)
            {
                // Note: We don't display a MessageBox here as an appropriate one will be generated in the
                //       validate method - so we don't need to double up.

                Console.WriteLine("Validation failed! GameConfig not saved.");

                return;
            }

            //Console.WriteLine("After validation config dir is: " + gameConfig.ConfigDirectory);

            string configDir  = ".\\Configs\\" + gameConfig.ConfigDirectory;
            string configPath = configDir + "\\config.xml";


            //Console.WriteLine("Config directory is: " + gameConfig.ConfigDirectory);

            //Console.WriteLine("But configDir is: " + configDir);

            // Try to create the directory. Note: If the directory already exists then this does nothing.
            Directory.CreateDirectory(configDir);

            // Finally, if the GameConfig is valid write it to file
            Utils.WriteToXmlFile(configPath, gameConfig);
        }

        // Remove all the controls from a TableLayoutPanel
        private void clearPanel(TableLayoutPanel p)
        {
            p.Visible = false;

            for (int i = (p.Controls.Count) - 1; i >= 0; --i)
            {                
                p.Controls[i].Dispose();
            }

            p.Visible = true;
        }

        private void RebuildTreeViewFromGameConfig()
        {
            // Get the tree
            TreeView tv = this.gcTreeView;

            tv.BeginUpdate();

                // Remove all nodes
                tv.Nodes.Clear();

                // Add the root node
                TreeNode tn = tv.Nodes.Add("Game Config");
                tn.Tag = "GameConfig";
                tv.SelectedNode = tn;
            
                // Add the "Watches" and "Triggers" nodes
                TreeNode watchNode = tn.Nodes.Add("Watches");
                watchNode.Tag = "Watches";
                TreeNode triggerNode = tn.Nodes.Add("Triggers");
                triggerNode.Tag = "Triggers";

                // Add all watch nodes
                TreeNode tempNode;
                foreach (Watch w in gameConfig.watchList)
                {
                    string s = w.Id + "-" + w.Name;
                    tempNode = watchNode.Nodes.Add(s);
                    tempNode.Tag = "Watch";
                }

                // Add all trigger nodes
                foreach (Trigger t in gameConfig.triggerList)
                {
                    string s = t.id + "-" + t.name;
                    tempNode = triggerNode.Nodes.Add(s);
                    tempNode.Tag = "Trigger";
                }

            tv.EndUpdate();

            tv.ExpandAll();
            tv.Focus();
        }


        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("1 - Selected index changed. Config dir is: " + gameConfig.ConfigDirectory);

            // On edit tab?
            if (this.tabControl.SelectedIndex == 1)
            {
                //Console.WriteLine("2 - Selected index changed. Config dir is: " + gameConfig.ConfigDirectory);

                // Cancel sonification if we move to the edit tab while running
                if (running)
                {
                    // This sets cancellation to pending, which we handle in the associated doWork method
                    // to actually perform the cancellation.
                    Program.sonificationBGW.CancelAsync();

                    Console.WriteLine("Sonification stopped.");

                    this.Text = formTitle + " Status: Stopped";
                    running = false;
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

                    // Maintain the config directory as the actual directory we're in!
                    String s = gameConfig.ConfigDirectory;
                    gameConfig = Utils.ReadFromXmlFile<GameConfig>(pathToConfig);
                    gameConfig.ConfigDirectory = s;


                }

                RebuildTreeViewFromGameConfig();

                TreeView tv = this.gcTreeView;
            }
            else // We must be on index 0, and so we should update the GameConfig ComboBox incase the user has created a new config they want to run
            {
                populateMainConfigsBox();

                
            }

        } // End of tabControl_SelectedIndexChanged method

        // Stop button handler to stop the sonification background worker
        private void stopConfigButton_Click(object sender, EventArgs e)
        {
            // This sets cancellation to pending, which we handle in the associated doWork method
            // to actually perform the cancellation.
            GameConfig.processConnectionBW.CancelAsync();
            Program.sonificationBGW.CancelAsync();

            this.Text = formTitle + " Status: Stopped";
            running = false;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            populateMainConfigsBox();
        }

        private void gcTreeView_AfterSelect(object sender, TreeViewEventArgs tvea)
        {
            // Clear the panel
            TableLayoutPanel panel = this.gcPanel;

            panel.Location = new Point(detailsPanelX, detailsPanelY);

            clearPanel(panel);

            panel.Visible = false;


            //panel.Location = Size.
            //panel.AutoScroll = true;
            //panel.AutoSizeMode = AutoSizeMode.GrowAndShrink; <-- this one

            // Update the current node to be the node which triggered this method
            currentTreeNode = tvea.Node;
            
            // Recreate the panel based on the current node tyoe
            switch (currentTreeNode.Tag.ToString())
            {
                case "GameConfig":
                    {
                        currentUILabel.Text = "GameConfig General Settings";

                        // Recreate as GameConfig panel

                        int row = 0;
                        // ----- Row 0 - Config directory -----                
                        Label dirLabel = new Label();
                        dirLabel.AutoSize = true;
                        dirLabel.Text = "Directory";
                        dirLabel.Tag = "DirectoryLabel";
                        dirLabel.Anchor = AnchorStyles.Right;
                        dirLabel.Margin = padding;
                        
                        panel.Controls.Add(dirLabel, 0, row); // Control, Column, Row

                        TextBox dirTB = new TextBox();
                        dirTB.Text = gameConfig.ConfigDirectory;
                        dirTB.TextChanged += (object s, EventArgs ea) => { gameConfig.ConfigDirectory = dirTB.Text; };
                        dirTB.Tag = "dirTB";
                        dirTB.Anchor = AnchorStyles.Right;
                        dirTB.Dock = DockStyle.Fill;
                        dirTB.Margin = padding;
                        panel.Controls.Add(dirTB, 1, row); // Control, Column, Row                        
                        row++;

                        // ----- Row 1 - Config description -----                
                        Label descLabel = new Label();
                        descLabel.AutoSize = true;
                        descLabel.Text = "Description";
                        descLabel.Tag = "DescriptionLabel";
                        descLabel.Anchor = AnchorStyles.Right;
                        descLabel.Margin = padding;
                        panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

                        TextBox descTB = new TextBox();
                        descTB.Text = gameConfig.Description;
                        descTB.TextChanged += (object s, EventArgs ea) => { gameConfig.Description = descTB.Text; };
                        descTB.Tag = "descTB";
                        descTB.Anchor = AnchorStyles.Right;
                        descTB.Dock = DockStyle.Fill;
                        descTB.Margin = padding;
                        panel.Controls.Add(descTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 2 - Process name -----                
                        Label processLabel = new Label();
                        processLabel.AutoSize = true;
                        processLabel.Text = "Process Name";
                        processLabel.Tag = "processLabel";
                        processLabel.Anchor = AnchorStyles.Right;
                        processLabel.Margin = padding;
                        panel.Controls.Add(processLabel, 0, row); // Control, Column, Row

                        TextBox processTB = new TextBox();
                        processTB.Text = gameConfig.ProcessName;
                        processTB.TextChanged += (object s, EventArgs ea) => { gameConfig.ProcessName = processTB.Text; };
                        processTB.Tag = "processTB";
                        processTB.Anchor = AnchorStyles.Right;
                        processTB.Dock = DockStyle.Fill;
                        processTB.Margin = padding;
                        panel.Controls.Add(processTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 3 - Poll Sleep -----                
                        Label pollLabel = new Label();
                        pollLabel.AutoSize = true;
                        pollLabel.Text = "Poll Sleep (Milliseconds)";
                        pollLabel.Tag = "pollLabel";
                        pollLabel.Anchor = AnchorStyles.Right;
                        pollLabel.Margin = padding;
                        panel.Controls.Add(pollLabel, 0, row); // Control, Column, Row

                        TextBox pollTB = new TextBox();
                        pollTB.Text = gameConfig.PollSleepMS.ToString();
                        pollTB.TextChanged += (object s, EventArgs ea) =>
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
                                    MessageBox.Show("Warning: Poll sleep MS must be an integer value.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    gameConfig.PollSleepMS = -1;
                                }
                            }                                
                        };
                        pollTB.Tag = "pollTB";
                        pollTB.Anchor = AnchorStyles.Right;
                        pollTB.Dock = DockStyle.Fill;
                        pollTB.Margin = padding;
                        panel.Controls.Add(pollTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 4 - Clock Tick MS -----                
                        Label clockTickLabel = new Label();
                        clockTickLabel.AutoSize = true;
                        clockTickLabel.Text = "Clock Tick (Milliseconds)";
                        clockTickLabel.Tag = "clockTickLabel";
                        clockTickLabel.Anchor = AnchorStyles.Right;
                        clockTickLabel.Margin = padding;
                        panel.Controls.Add(clockTickLabel, 0, row); // Control, Column, Row

                        TextBox clockTickTB = new TextBox();
                        clockTickTB.Text = gameConfig.ClockTickMS.ToString();
                        clockTickTB.TextChanged += (object s, EventArgs ea) =>
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
                                    MessageBox.Show("Warning: Clock Tick MS must be an integer value expressed in milliseconds.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    gameConfig.ClockTickMS = -1;
                                }
                            }
                        };
                        clockTickTB.Tag = "clockTickTB";
                        clockTickTB.Anchor = AnchorStyles.Right;
                        clockTickTB.Dock = DockStyle.Fill;
                        clockTickTB.Margin = padding;
                        panel.Controls.Add(clockTickTB, 1, row); // Control, Column, Row
                        row++;

                        break;
                    }
                case "Watches":
                    {
                        currentUILabel.Text = "Watch Description";

                        TextBox watchDescriptionTB = new TextBox();
                        watchDescriptionTB.ReadOnly = true;

                        watchDescriptionTB.Multiline = true; // Must be enabled to have newlines in output
                        watchDescriptionTB.Font = new Font(watchDescriptionTB.Font.FontFamily, 12); // Crank up the font size

                        watchDescriptionTB.Text = "A watch is a memory location expressed as a complex pointer address and the type of data to read from that address.";
                        watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        watchDescriptionTB.Text += "You do not need to provide the process base address, only a comma-separated list of pointers starting with the base-offset ";
                        watchDescriptionTB.Text += "which will be used to read the data at that address.";
                        watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        watchDescriptionTB.Text += "Each watch must have a unique watch id specified as an integer greater than or equal to zero, and may optionally have a name and description.";
                        watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        watchDescriptionTB.Text += "Only watches that are marked as active will be used - so you can disable a watch while still keeping the data around in your GameConfig.";
                        watchDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        watchDescriptionTB.Text += "Please read the documentation to learn more about how to identify consistent pointer trails for use as watches.";

                        // Make the description span both columns in the TableLayoutPanel
                        watchDescriptionTB.WordWrap = true;
                        watchDescriptionTB.Width = 550;
                        watchDescriptionTB.Height = 400;
                        watchDescriptionTB.Dock = DockStyle.Fill;
                        panel.Controls.Add(watchDescriptionTB);
                        panel.SetCellPosition(watchDescriptionTB, new TableLayoutPanelCellPosition(0, 1));
                        panel.SetColumnSpan(watchDescriptionTB, 2);
                                                
                        break;
                    }   
                case "Watch":
                    {
                        currentUILabel.Text = "Watch Settings";

                        // Get the current watch we're working from based on the index of the currently selected treenode
                        // Note: Each child of a parent treenode starts at index 0, so we can use this index as the
                        // index of the watch (in the watchList) that we're currently modifying.
                        int watchIndex = currentTreeNode.Index;
                        currentWatch = gameConfig.watchList[watchIndex];
                        
                        int row = 0;
                        // ----- Row 0 - ID -----                
                        Label idLabel = new Label();
                        idLabel.AutoSize = true;
                        idLabel.Text = "Watch ID (unique int zero or greater)";
                        idLabel.Tag = "idLabel";
                        idLabel.Anchor = AnchorStyles.Right;
                        idLabel.Margin = padding;
                        panel.Controls.Add(idLabel, 0, row); // Control, Column, Row

                        TextBox idTB = new TextBox();
                        idTB.Text = currentWatch.Id.ToString();
                        idTB.TextChanged += (object s, EventArgs ea) =>
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
                                    MessageBox.Show("Warning: Watch ID must be an integer value.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    currentWatch.Id = -1;
                                }
                            }
                        };
                        idTB.Tag = "idTB";
                        idTB.Anchor = AnchorStyles.Right;
                        idTB.Dock = DockStyle.Fill;
                        idTB.Margin = padding;
                        panel.Controls.Add(idTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 1 - Name -----                
                        Label nameLabel = new Label();
                        nameLabel.AutoSize = true;
                        nameLabel.Text = "Watch Name";
                        nameLabel.Tag = "nameLabel";
                        nameLabel.Anchor = AnchorStyles.Right;
                        nameLabel.Margin = padding;
                        panel.Controls.Add(nameLabel, 0, row); // Control, Column, Row

                        TextBox nameTB = new TextBox();
                        nameTB.Text = currentWatch.Name.ToString();
                        nameTB.TextChanged += (object s, EventArgs ea) =>
                        {
                            currentWatch.Name = nameTB.Text;
                            currentTreeNode.Text = currentWatch.Id + "-" + currentWatch.Name;
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
                        descLabel.Text = "Watch Description";
                        descLabel.Tag = "descLabel";
                        descLabel.Anchor = AnchorStyles.Right;
                        descLabel.Margin = padding;
                        panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

                        TextBox descTB = new TextBox();
                        descTB.Text = currentWatch.Description.ToString();
                        descTB.TextChanged += (object s, EventArgs ea) => { currentWatch.Description = descTB.Text; };
                        descTB.Tag = "descTB";
                        descTB.Anchor = AnchorStyles.Right;
                        descTB.Dock = DockStyle.Fill;
                        descTB.Margin = padding;
                        panel.Controls.Add(descTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 3 - Pointer List -----            
                        Label pointerLabel = new Label();
                        pointerLabel.AutoSize = true;
                        pointerLabel.Text = "Pointer List (comma-separated hex, no prefixes)";
                        pointerLabel.Tag = "pointerLabel";
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
                        pointerTB.TextChanged += (object s, EventArgs ea) =>
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
                                    MessageBox.Show("Illegal Pointer Value " + pointerValue + " in watch with id " + currentWatch.Id + " cannot be cast to int. Do not prefix pointer hops with 0x or such.");
                                    return;
                                }
                            }

                            currentWatch.PointerList = tempPointerList;
                        };

                        pointerTB.Tag = "pointerTB";
                        pointerTB.Anchor = AnchorStyles.Right;
                        pointerTB.Dock = DockStyle.Fill;
                        pointerTB.Margin = padding;
                        panel.Controls.Add(pointerTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 4 - Value Type -----            
                        Label typeLabel = new Label();
                        typeLabel.AutoSize = true;
                        typeLabel.Text = "Value Type";
                        typeLabel.Tag = "typeLabel";
                        typeLabel.Anchor = AnchorStyles.Right;
                        typeLabel.Margin = padding;
                        panel.Controls.Add(typeLabel, 0, row); // Control, Column, Row
                        
                        ComboBox typeCB = new ComboBox();
                        typeCB.DropDownStyle = ComboBoxStyle.DropDownList;
                        typeCB.Items.AddRange(dataTypesArray);
                        typeCB.SelectedIndex = Utils.GetIntFromValueType(currentWatch.valueType);
                        typeCB.Tag = "typeCB";
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
                        activeLabel.Text = "Active";
                        activeLabel.Tag = "activeLabel";
                        activeLabel.Anchor = AnchorStyles.Right;
                        activeLabel.Margin = padding;
                        panel.Controls.Add(activeLabel, 0, row); // Control, Column, Row

                        CheckBox activeCB = new CheckBox();
                        activeCB.Checked = currentWatch.Active;
                        activeCB.CheckedChanged += (object s, EventArgs ea) => { currentWatch.Active = activeCB.Checked; };
                        activeCB.Tag = "activeCB";
                        activeCB.Anchor = AnchorStyles.Right;
                        activeCB.Dock = DockStyle.Fill;
                        activeCB.Margin = padding;
                        panel.Controls.Add(activeCB, 1, row); // Control, Column, Row
                        row++;
                        
                        // ----- Row 6 - Delete Watch -----
                        Button deleteWatchBtn = new Button();
                        deleteWatchBtn.AutoSize = true;
                        deleteWatchBtn.Text = "Delete Watch";
                        deleteWatchBtn.Tag = "deleteWatchButton";
                        deleteWatchBtn.Anchor = AnchorStyles.Right;
                        deleteWatchBtn.Margin = padding;
                        deleteWatchBtn.BackColor = Color.Red;
                        deleteWatchBtn.Click += (object s, EventArgs ea) =>
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

                        break;
                    }
                case "Triggers":
                    {
                        currentUILabel.Text = "Trigger Description";

                        TextBox triggerDescriptionTB = new TextBox();
                        triggerDescriptionTB.ReadOnly = true;

                        triggerDescriptionTB.Multiline = true; // Must be enabled to have newlines in output
                        triggerDescriptionTB.Font = new Font(triggerDescriptionTB.Font.FontFamily, 12); // Crank up the font size

                        triggerDescriptionTB.Text = "A trigger is a condition which can trigger a sonification event when the condition is met.";
                        triggerDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        triggerDescriptionTB.Text += "Triggers use watches, as identified by their numeric id values, to read data and compare that data to certain conditions.";
                        triggerDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        triggerDescriptionTB.Text += "Each trigger must have a unique trigger id specified as an integer greater than or equal to zero, and a condition to match against.";
                        triggerDescriptionTB.Text += Environment.NewLine + Environment.NewLine;
                        triggerDescriptionTB.Text += "Please read the documentation to learn more about how to set and use triggers.";

                        // Make the description span both columns in the TableLayoutPanel
                        triggerDescriptionTB.WordWrap = true;
                        triggerDescriptionTB.Width = 550;
                        triggerDescriptionTB.Height = 400;
                        triggerDescriptionTB.Dock = DockStyle.Fill;
                        panel.Controls.Add(triggerDescriptionTB);
                        panel.SetCellPosition(triggerDescriptionTB, new TableLayoutPanelCellPosition(0, 1));
                        panel.SetColumnSpan(triggerDescriptionTB, 2);

                        break;
                    }                    
                case "Trigger":
                    {
                        currentUILabel.Text = "Trigger Settings";

                        // Get the current watch we're working from based on the index of the currently selected treenode
                        // Note: Each child of a parent treenode starts at index 0, so we can use this index as the
                        // index of the watch (in the watchList) that we're currently modifying.
                        int triggerIndex = currentTreeNode.Index;
                        currentTrigger = gameConfig.triggerList[triggerIndex];

                        int row = 0;
                        // ----- Row 0 - ID -----                
                        Label idLabel = new Label();
                        idLabel.AutoSize = true;
                        idLabel.Text = "Trigger ID (unique int zero or greater)";
                        idLabel.Tag = "idLabel";
                        idLabel.Anchor = AnchorStyles.Right;
                        idLabel.Margin = padding;;
                        panel.Controls.Add(idLabel, 0, row); // Control, Column, Row

                        TextBox idTB = new TextBox();
                        idTB.Text = currentTrigger.id.ToString();
                        idTB.TextChanged += (object s, EventArgs ea) =>
                        {
                            int x;
                            bool result = Int32.TryParse(idTB.Text, out x);
                            if (result)
                            {
                                currentTrigger.id = x;
                                currentTreeNode.Text = currentTrigger.id + "-" + currentTrigger.name;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(idTB.Text.ToString()))
                                {
                                    MessageBox.Show("Warning: Trigger ID must be an integer value.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    currentTrigger.id = -1;
                                }
                            }
                        };
                        idTB.Tag = "idTB";
                        idTB.Anchor = AnchorStyles.Right;
                        idTB.Dock = DockStyle.Fill;
                        idTB.Margin = padding;
                        panel.Controls.Add(idTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 1 - Name -----                
                        Label nameLabel = new Label();
                        nameLabel.AutoSize = true;
                        nameLabel.Text = "Trigger Name";
                        nameLabel.Tag = "triggerLabel";
                        nameLabel.Anchor = AnchorStyles.Right;
                        nameLabel.Margin = padding;
                        panel.Controls.Add(nameLabel, 0, row); // Control, Column, Row

                        TextBox nameTB = new TextBox();                        
                        nameTB.Text = currentTrigger.name.ToString();
                        nameTB.TextChanged += (object s, EventArgs ea) =>
                        {
                            currentTrigger.name = nameTB.Text;
                            currentTreeNode.Text = currentTrigger.id + "-" + currentTrigger.name;
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
                        descLabel.Text = "Trigger Description";
                        descLabel.Tag = "descLabel";
                        descLabel.Anchor = AnchorStyles.Right;
                        descLabel.Margin = padding;
                        panel.Controls.Add(descLabel, 0, row); // Control, Column, Row

                        TextBox descTB = new TextBox();
                        descTB.Text = currentTrigger.description.ToString();
                        descTB.TextChanged += (object s, EventArgs ea) => { currentTrigger.description = descTB.Text; };
                        descTB.Tag = "descTB";
                        descTB.Anchor = AnchorStyles.Right;
                        descTB.Dock = DockStyle.Fill;
                        descTB.Margin = padding;
                        panel.Controls.Add(descTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 3 - Comparison type ----- 
                        Label compTypeLabel = new Label();
                        compTypeLabel.AutoSize = true;
                        compTypeLabel.Text = "Comparison Type";
                        compTypeLabel.Tag = "compTypeLabel";
                        compTypeLabel.Anchor = AnchorStyles.Right;
                        compTypeLabel.Margin = padding;
                        panel.Controls.Add(compTypeLabel, 0, row); // Control, Column, Row

                        // Define the value TextBox here so we can disable it if the comparison type is 'DistanceBetween' (in which case we use a min-max range instead of a value)
                        TextBox valueTB = new TextBox();

                        TextBox watch2TB = new TextBox();

                        ComboBox compTypeCB = new ComboBox();
                        compTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
                        compTypeCB.Items.AddRange(comparisonTypesArray);
                        compTypeCB.SelectedIndex = Utils.GetIntFromComparisonType(currentTrigger.comparisonType);
                        compTypeCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                        {
                            currentTrigger.comparisonType = Utils.GetComparisonTypeFromInt(compTypeCB.SelectedIndex);

                            //Console.WriteLine("current trigger comparison type is now: " + currentTrigger.comparisonType);



                            // If the trigger comparison type is distance then we use the value property as 'max range'.
                            // If the comaprison type is not distance we disable the 2nd watch id because we're only using a single watch.
                            if (currentTrigger.comparisonType == Trigger.ComparisonType.DistanceBetween)
                            {
                                //valueTB.Enabled = false; // Value is what we're triggering on i.e. > 55 or such.
                                watch2TB.Enabled = true; 
                            }
                            else
                            {
                               // valueTB.Enabled = true;
                                watch2TB.Enabled = false;
                            }
                        };
                        compTypeCB.Tag = "compTypeCB";
                        compTypeCB.Anchor = AnchorStyles.Left;
                        compTypeCB.Dock = DockStyle.Fill;
                        compTypeCB.Margin = padding;
                        panel.Controls.Add(compTypeCB, 1, row); // Control, Column, Row
                        row++;

                        // Row 4 - Watch ID 1
                        Label watch1Label = new Label();
                        watch1Label.AutoSize = true;
                        watch1Label.Text = "Watch 1 ID";
                        watch1Label.Anchor = AnchorStyles.Right;
                        watch1Label.Margin = padding;
                        panel.Controls.Add(watch1Label, 0, row); // Control, Column, Row

                        TextBox watch1TB = new TextBox();
                        watch1TB.Text = currentTrigger.watchOneId.ToString();
                        watch1TB.Tag = "watch1TB";
                        watch1TB.Anchor = AnchorStyles.Left;
                        watch1TB.Dock = DockStyle.Fill;
                        watch1TB.Margin = padding;
                        watch1TB.TextChanged += (object s, EventArgs ea) =>
                        {
                            int x;
                            bool result = Int32.TryParse(watch1TB.Text, out x);
                            if (result)
                            {
                                currentTrigger.watchOneId = x;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(watch1TB.Text.ToString()))
                                {
                                    MessageBox.Show("Warning: Watch 1 ID must be an integer value.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    currentTrigger.watchOneId = -1;
                                }
                            }
                        };

                        panel.Controls.Add(watch1TB, 1, row); // Control, Column, Row
                        row++;
                        
                        // Row 5 - Watch ID 2 (only editable if distance type)                        
                        Label watch2Label = new Label();
                        watch2Label.AutoSize = true;
                        watch2Label.Text = "Watch 2 ID";
                        watch2Label.Anchor = AnchorStyles.Right;
                        watch2Label.Margin = padding;
                        panel.Controls.Add(watch2Label, 0, row); // Control, Column, Row

                        //TextBox watch2TB = new TextBox(); DEFINED ABOVE SO CAN CHANGE FROM ComparisonType dropdown
                        watch2TB.Text = currentTrigger.watchTwoId.ToString();
                        watch2TB.Tag = "watch2TB";
                        watch2TB.Anchor = AnchorStyles.Left;
                        watch2TB.Dock = DockStyle.Fill;
                        watch2TB.Margin = padding;

                        // Only enable the 2nd watch if we're comparing distance of two watches
                        if (currentTrigger.comparisonType != Trigger.ComparisonType.DistanceBetween)
                        {
                            watch2TB.Enabled = false;
                        }
                        else
                        {
                            watch2TB.Enabled = true;
                        }

                        watch2TB.TextChanged += (object s, EventArgs ea) =>
                        {
                            int x;
                            bool result = Int32.TryParse(watch2TB.Text, out x);
                            if (result)
                            {
                                currentTrigger.watchTwoId = x;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(watch2TB.Text.ToString()))
                                {
                                    MessageBox.Show("Warning: Watch 2 ID must be an integer value.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    currentTrigger.watchTwoId = -1;
                                }
                            }
                        };

                        panel.Controls.Add(watch2TB, 1, row); // Control, Column, Row
                        row++;

                        // -----  Row 6 - Trigger type (Once, Recurring, Continuous) -----
                        Label triggerTypeLabel = new Label();
                        triggerTypeLabel.AutoSize = true;
                        triggerTypeLabel.Text = "Trigger Type";
                        triggerTypeLabel.Tag = "triggerTypeLabel";
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
                        };
                        triggerTypeCB.Tag = "triggerTypeCB";
                        triggerTypeCB.Anchor = AnchorStyles.Left;
                        triggerTypeCB.Dock = DockStyle.Fill;
                        triggerTypeCB.Margin = padding;
                        panel.Controls.Add(triggerTypeCB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 7 - Trigger Value -----
                        Label valueLabel = new Label();
                        valueLabel.AutoSize = true;
                        valueLabel.Text = "Trigger Value / Max Range";
                        valueLabel.Tag = "valueLabel";
                        valueLabel.Anchor = AnchorStyles.Right;
                        valueLabel.Margin = padding;
                        panel.Controls.Add(valueLabel, 0, row); // Control, Column, Row

                        // Note: valueTB is created above so we can access it in the comparison type dropdown
                        // Also: This TextBox will toggle to ReadOnly if the comparison type is distance, and editable if it's
                        //       anything else. See the above trigger type ComboBox row.
                        valueTB.Text = currentTrigger.value.ToString();

                        // Set a max of 35 chars
                        valueTB.MaxLength = Program.TEXT_COMPARISON_CHAR_LIMIT;

                        // Comparison TextBox handler
                        valueTB.TextChanged += (object s, EventArgs ea) => {

                            // Trim whitespace
                            currentTrigger.value = valueTB.Text.TrimEnd();

                            // ...then set!
                            //currentTrigger.value = valueTB.Text;

                            // If we've maxed out our 35 chars move the carat to the end and select nothing.
                            /*if (valueTB.Text.Length = Program.TEXT_COMPARISON_CHAR_LIMIT)
                            {
                                valueTB.Select(valueTB.Text.Length, 0);
                            }*/
                        };

                        valueTB.Tag = "valueTB";
                        valueTB.Anchor = AnchorStyles.Left;
                        valueTB.Dock = DockStyle.Fill;
                        valueTB.Margin = padding;
                        panel.Controls.Add(valueTB, 1, row); // Control, Column, Row
                        row++;


                        // UI elements to disable if this trigger is a reset trigger (which is controlled by the row & checkbox below)                        
                        TextBox sampleFilenameTB = new TextBox();
                        Button sampleFilenameButton = new Button();
                        TextBox sampleVolumeTB = new TextBox();
                        TextBox sampleSpeedTB = new TextBox();

                        // ----- Row 8 - Control Type (normal / reset / mute) row -----
                        Label triggerControlLabel = new Label();
                        triggerControlLabel.AutoSize = true;
                        triggerControlLabel.Text = "Control Type";
                        triggerControlLabel.Anchor = AnchorStyles.Right;
                        triggerControlLabel.Margin = padding;
                        panel.Controls.Add(triggerControlLabel, 0, row); // Control, Column, Row


                        ComboBox triggerControlCB = new ComboBox();
                        triggerControlCB.DropDownStyle = ComboBoxStyle.DropDownList;
                        triggerControlCB.Items.AddRange(controlTypesArray);
                        triggerControlCB.SelectedIndex = Utils.GetIntFromControlType(currentTrigger.controlType);
                        if (triggerControlCB.SelectedIndex != 0) { triggerTypeCB.Enabled = false; } // Disable trigger type dropdown if we're not a 'Normal' trigger (i.e. Reset or Mute type)
                        triggerControlCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                        {
                            currentTrigger.controlType = Utils.GetControlTypeFromInt(triggerControlCB.SelectedIndex);

                            // If this trigger's control type is anything but Normal we don't care about samples & trigger type so disable those UI elements...
                            if (currentTrigger.controlType != Trigger.ControlType.Normal)
                            {
                                sampleFilenameTB.Enabled = false;
                                sampleFilenameButton.Enabled = false;
                                sampleVolumeTB.Enabled = false;
                                sampleSpeedTB.Enabled = false;

                                triggerTypeCB.Enabled = false;
                            }
                            else // ...otherwise for a Normal trigger we do care so keep them enabled.
                            {
                                sampleFilenameTB.Enabled = true;
                                sampleFilenameButton.Enabled = true;
                                sampleVolumeTB.Enabled = true;
                                sampleSpeedTB.Enabled = true;

                                triggerTypeCB.Enabled = true;
                            }
                        };
                        triggerControlCB.Tag = "triggerControlCB";
                        triggerControlCB.Anchor = AnchorStyles.Left;
                        triggerControlCB.Dock = DockStyle.Fill;
                        triggerControlCB.Margin = padding;
                        panel.Controls.Add(triggerControlCB, 1, row); // Control, Column, Row
                        row++;



                        // ----- Row 9 - Trigger sample row -----
                        Label sampleFilenameLabel = new Label();
                        sampleFilenameLabel.AutoSize = true;
                        sampleFilenameLabel.Text = "Sample Filename (without path)";
                        sampleFilenameLabel.Anchor = AnchorStyles.Right;
                        sampleFilenameLabel.Margin = padding;
                        panel.Controls.Add(sampleFilenameLabel, 0, row); // Control, Column, Row

                        // If we want to add two controls to a cell in a TableLayoutPanel we have to put a panel in that cell
                        // then add the controls to that panel!
                        Panel sampleSelectionPanel = new Panel();
                        sampleSelectionPanel.Height = compTypeCB.Height;
                        sampleSelectionPanel.Dock = DockStyle.Fill;
                        sampleSelectionPanel.Margin = padding;                       

                        
                        sampleFilenameTB.Text = currentTrigger.sampleFilename;
                        sampleFilenameTB.Tag = "sampleFilenameTB";
                        sampleFilenameTB.Anchor = AnchorStyles.Left;
                        sampleFilenameTB.Dock = DockStyle.Fill;
                        sampleFilenameTB.Margin = new System.Windows.Forms.Padding(0);

                        if (triggerControlCB.SelectedIndex != 0) { sampleFilenameTB.Enabled = false; } // Disable sample field if we're not a 'Normal' trigger (i.e. Reset or Mute type)

                        sampleFilenameTB.TextChanged += (object s, EventArgs ea) => { currentTrigger.sampleFilename = sampleFilenameTB.Text; };

                        sampleSelectionPanel.Controls.Add(sampleFilenameTB);
                                                
                        sampleFilenameButton.AutoSize = true;
                        sampleFilenameButton.Anchor = AnchorStyles.Right;
                        sampleFilenameButton.Dock = DockStyle.Right;
                        sampleFilenameButton.Margin = new System.Windows.Forms.Padding(0);
                        sampleFilenameButton.Text = "Select Sample File";
                        sampleFilenameButton.Click += (object o, EventArgs ae) =>
                        {
                            OpenFileDialog file = new OpenFileDialog();
                            file.InitialDirectory = Application.StartupPath + "\\Configs\\" + gameConfig.ConfigDirectory; // Open dialog in gameconfig directory
                            if (file.ShowDialog() == DialogResult.OK)
                            {
                                // Note: Filename gives you the full path to the file, SafeFilename gives you ONLY the filename including extension, which is what we want
                                currentTrigger.sampleFilename = file.SafeFileName;
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
                        sampleVolumeLabel.Text = "Sample Volume (Range: 0.0 to 1.0)";
                        sampleVolumeLabel.Anchor = AnchorStyles.Right;
                        sampleVolumeLabel.Margin = padding;                        
                        panel.Controls.Add(sampleVolumeLabel, 0, row); // Control, Column, Row

                        
                        sampleVolumeTB.Text = currentTrigger.sampleVolume.ToString();
                        sampleVolumeTB.Tag = "sampleVolumeTB";
                        sampleVolumeTB.Anchor = AnchorStyles.Left;
                        sampleVolumeTB.Dock = DockStyle.Fill;
                        sampleVolumeTB.Margin = padding;

                        if (triggerControlCB.SelectedIndex != 0) { sampleVolumeTB.Enabled = false; } // Disable sample volume field if we're not a 'Normal' trigger (i.e. Reset or Mute type)

                        sampleVolumeTB.TextChanged += (object s, EventArgs ea) => {
                            float x;
                            bool result = float.TryParse(sampleVolumeTB.Text, out x);
                            if (result)
                            {
                                currentTrigger.sampleVolume = x;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(sampleVolumeTB.Text.ToString()))
                                {
                                    MessageBox.Show("Warning: Sample volume must be a value between 0.0 and 1.0.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    currentTrigger.sampleVolume = -1.0f;
                                }
                            }
                        };

                        panel.Controls.Add(sampleVolumeTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 11 - Trigger sample rate ---
                        Label sampleSpeedLabel = new Label();
                        sampleSpeedLabel.AutoSize = true;
                        sampleSpeedLabel.Text = "Sample Rate (Range: 0.5 to 4.0)";
                        sampleSpeedLabel.Anchor = AnchorStyles.Right;
                        sampleSpeedLabel.Margin = padding;
                        panel.Controls.Add(sampleSpeedLabel, 0, row); // Control, Column, Row

                       
                        sampleSpeedTB.Text = currentTrigger.sampleSpeed.ToString();
                        sampleSpeedTB.Tag = "sampleRateTB";
                        sampleSpeedTB.Anchor = AnchorStyles.Left;
                        sampleSpeedTB.Dock = DockStyle.Fill;
                        sampleSpeedTB.Margin = padding;

                        if (triggerControlCB.SelectedIndex != 0) { sampleSpeedTB.Enabled = false; } // Disable sample speed field if we're not a 'Normal' trigger (i.e. Reset or Mute type)

                        sampleSpeedTB.TextChanged += (object s, EventArgs ea) =>
                        {
                            float x;
                            bool result = float.TryParse(sampleSpeedTB.Text, out x);
                            if (result)
                            {
                                currentTrigger.sampleSpeed = (float)x;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(sampleSpeedTB.Text.ToString()))
                                {
                                    MessageBox.Show("Warning: Sample speed must be a value between 0.5 and 4.0.");
                                }
                                else // Field empty? Invalidate it so we can catch it in the save section
                                {
                                    currentTrigger.sampleSpeed = -1.0f;
                                }
                            }
                        };

                        panel.Controls.Add(sampleSpeedTB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 12 - Active Flag -----            
                        Label isClockLabel = new Label();
                        isClockLabel.AutoSize = true;
                        isClockLabel.Text = "Is Clock?";
                        isClockLabel.Tag = "isClockLabel";
                        isClockLabel.Anchor = AnchorStyles.Right;
                        isClockLabel.Margin = padding;
                        panel.Controls.Add(isClockLabel, 0, row); // Control, Column, Row

                        CheckBox isClockCB = new CheckBox();
                        isClockCB.Checked = currentTrigger.isClock;
                        isClockCB.CheckedChanged += (object s, EventArgs ea) => { currentTrigger.isClock = isClockCB.Checked; };
                        isClockCB.Tag = "isClockCB";
                        isClockCB.Anchor = AnchorStyles.Right;
                        isClockCB.Dock = DockStyle.Fill;
                        isClockCB.Margin = padding;
                        panel.Controls.Add(isClockCB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 13 - Allowance Type (Any / InGame / InMenu) row -----
                        Label triggerAllowanceLabel = new Label();
                        triggerAllowanceLabel.AutoSize = true;
                        triggerAllowanceLabel.Text = "Allowance Type";
                        triggerAllowanceLabel.Anchor = AnchorStyles.Right;
                        triggerAllowanceLabel.Margin = padding;
                        panel.Controls.Add(triggerAllowanceLabel, 0, row); // Allowance, Column, Row
                        
                        ComboBox triggerAllowanceCB = new ComboBox();
                        triggerAllowanceCB.DropDownStyle = ComboBoxStyle.DropDownList;
                        triggerAllowanceCB.Items.AddRange(allowanceTypesArray);
                        triggerAllowanceCB.SelectedIndex = Utils.GetIntFromAllowanceType(currentTrigger.allowanceType);
                        //if (triggerAllowanceCB.SelectedIndex != 0) { triggerTypeCB.Enabled = false; } // Disable trigger type dropdown if we're not a 'Normal' trigger (i.e. Reset or Mute type)
                        triggerAllowanceCB.SelectedIndexChanged += (object o, EventArgs ae) =>
                        {
                            currentTrigger.allowanceType = Utils.GetAllowanceTypeFromInt(triggerAllowanceCB.SelectedIndex);                            
                        };
                        triggerAllowanceCB.Tag = "triggerAllowanceCB";
                        triggerAllowanceCB.Anchor = AnchorStyles.Left;
                        triggerAllowanceCB.Dock = DockStyle.Fill;
                        triggerAllowanceCB.Margin = padding;
                        panel.Controls.Add(triggerAllowanceCB, 1, row); // Control, Column, Row

                        if (currentTrigger.isClock)
                        {
                            triggerAllowanceCB.Enabled = false;
                        }
                        else
                        {
                            triggerAllowanceCB.Enabled = true;
                        }
                        row++;

                        // ----- Row 14 - Active Flag -----            
                        Label activeLabel = new Label();
                        activeLabel.AutoSize = true;
                        activeLabel.Text = "Active";
                        activeLabel.Tag = "activeLabel";
                        activeLabel.Anchor = AnchorStyles.Right;
                        activeLabel.Margin = padding;
                        panel.Controls.Add(activeLabel, 0, row); // Control, Column, Row

                        CheckBox activeCB = new CheckBox();
                        activeCB.Checked = currentTrigger.active;
                        activeCB.CheckedChanged += (object s, EventArgs ea) => { currentTrigger.active = activeCB.Checked; };
                        activeCB.Tag = "activeCB";
                        activeCB.Anchor = AnchorStyles.Right;
                        activeCB.Dock = DockStyle.Fill;
                        activeCB.Margin = padding;
                        panel.Controls.Add(activeCB, 1, row); // Control, Column, Row
                        row++;

                        // ----- Row 15 - Delete Trigger -----
                        Button deleteTriggerBtn = new Button();
                        deleteTriggerBtn.AutoSize = true;
                        deleteTriggerBtn.Text = "Delete Trigger";
                        deleteTriggerBtn.Tag = "deleteTriggerButton";
                        deleteTriggerBtn.Anchor = AnchorStyles.Right;
                        deleteTriggerBtn.Margin = padding;
                        deleteTriggerBtn.BackColor = Color.Red;
                        deleteTriggerBtn.Click += (object s, EventArgs ea) =>
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
                    break;

                default:
                    MessageBox.Show("Warning: Got node with unrecognised Tag of: " + currentTreeNode.Tag.ToString());
                    break;

            } // End of switch

            panel.Visible = true;

        } // End of gcTreeView_AfterSelect method



        private void addWatchButton_Click(object sender, EventArgs e)
        {
            currentUILabel.Text = "Add Watch";

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            clearPanel(panel);

            panel.Width = 400;// = new System.Windows.Forms.Padding(100);

            currentWatch = new Watch();
            //currentWatch.Id = gameConfig.watchList.Count;
            currentWatch.Id = Utils.getNextWatchIndex(gameConfig.watchList);
            currentWatch.Name = "CHANGE_ME";
            currentWatch.Description = "CHANGE_ME";
            currentWatch.PointerList = new List<string>();
            currentWatch.valueType = Watch.ValueType.IntType;
            gameConfig.watchList.Add(currentWatch);
            
            // Add a new watch entry as a child node to the "Watches" node
            TreeView tv = this.gcTreeView;
            TreeNode watchesNode = Utils.FindNodeWithText(tv, "Watches");            
            tv.BeginUpdate();                
                currentTreeNode = watchesNode.Nodes.Add(currentWatch.Id + "-NEW_WATCH");
                currentTreeNode.Tag = "Watch";
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        private void addTriggerButton_Click(object sender, EventArgs e)
        {
            currentUILabel.Text = "Add Trigger";

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            clearPanel(panel);
            panel.Width = 400;

            // Create a new trigger and add it to the list
            currentTrigger = new Trigger();
            //currentTrigger.id = gameConfig.triggerList.Count;
            currentTrigger.id = Utils.getNextTriggerIndex(gameConfig.triggerList);
            gameConfig.triggerList.Add(currentTrigger);

            // Add a new watch entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode triggersNode = Utils.FindNodeWithText(tv, "Triggers");
            tv.BeginUpdate();
            currentTreeNode = triggersNode.Nodes.Add(currentTrigger.id + "-NEW_TRIGGER");
            currentTreeNode.Tag = "Trigger";
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        private void cloneTriggerButton_Click(object sender, EventArgs e)
        {
            currentUILabel.Text = "Clone Trigger";

            // Ensure the user has a Trigger selected before starting the clone process
            if (currentTreeNode.Tag.ToString() != "Trigger")
            {
                MessageBox.Show("You must select a trigger before opting to clone that trigger.");
                return;
            }           

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            clearPanel(panel);
            panel.Width = detailsPanelX;

            // Create a new trigger and use the one parameter Trigger constructor to make it a deep-copy of the existing current trigger
            currentTrigger = new Trigger(currentTrigger);

            //currentTrigger.id = gameConfig.triggerList.Count;
            currentTrigger.id = Utils.getNextTriggerIndex(gameConfig.triggerList);
            gameConfig.triggerList.Add(currentTrigger);

            // Add a new watch entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode triggersNode = Utils.FindNodeWithText(tv, "Triggers");
            tv.BeginUpdate();
            currentTreeNode = triggersNode.Nodes.Add(currentTrigger.id + "-" + currentTrigger.name);
            currentTreeNode.Tag = "Trigger";
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        private void cloneWatchButton_Click(object sender, EventArgs e)
        {
            currentUILabel.Text = "Clone Watch";

            // Ensure the user has a Trigger selected before starting the clone process
            if (currentTreeNode.Tag.ToString() != "Watch")
            {
                MessageBox.Show("You must select a watch before opting to clone that watch.");
                return;
            }

            // Get the panel and remove all controls
            TableLayoutPanel panel = this.gcPanel;
            clearPanel(panel);
            panel.Width = detailsPanelWidth;

            // Create a new trigger and use the one parameter Trigger constructor to make it a deep-copy of the existing current trigger
            currentWatch = new Watch(currentWatch);

            //currentTrigger.id = gameConfig.triggerList.Count;
            currentWatch.Id = Utils.getNextWatchIndex(gameConfig.watchList);
            gameConfig.watchList.Add(currentWatch);

            // Add a new watch entry as a child node to the "Triggers" node
            TreeView tv = this.gcTreeView;
            TreeNode triggersNode = Utils.FindNodeWithText(tv, "Watches");
            tv.BeginUpdate();
            currentTreeNode = triggersNode.Nodes.Add(currentWatch.Id + "-" + currentWatch.name);
            currentTreeNode.Tag = "Watch";
            tv.EndUpdate();
            tv.ExpandAll();

            // Update our the tree's selected node and 'highlight' it like it had been clicked on
            // Note: When this node becomes selected the UI for the type of node is constructed in the 'gcTreeView_AfterSelect' method
            tv.SelectedNode = currentTreeNode;
            tv.Focus();
        }

        private void appTitleLable_Click(object sender, EventArgs e)
        {

        }
    } // End of MainForm partial class

}

