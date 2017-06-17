using System;

namespace SoniFight
{

    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.PictureBox pictureBox3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Watches");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Triggers");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("GameConfig", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.appTitleLable = new System.Windows.Forms.Label();
            this.configsComboBox = new System.Windows.Forms.ComboBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.mainTabPanel = new System.Windows.Forms.TableLayoutPanel();
            this.quitButton = new System.Windows.Forms.Button();
            this.createNewConfigButton = new System.Windows.Forms.Button();
            this.stopConfigButton = new System.Windows.Forms.Button();
            this.runConfigButton = new System.Windows.Forms.Button();
            this.mainTabDropdownPanel = new System.Windows.Forms.Panel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.mainTabPage = new System.Windows.Forms.TabPage();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.editTabPage = new System.Windows.Forms.TabPage();
            this.cloneWatchButton = new System.Windows.Forms.Button();
            this.cloneTriggerButton = new System.Windows.Forms.Button();
            this.currentUILabel = new System.Windows.Forms.Label();
            this.addWatchButton = new System.Windows.Forms.Button();
            this.addTriggerButton = new System.Windows.Forms.Button();
            this.gcPanel = new System.Windows.Forms.TableLayoutPanel();
            this.gcTreeView = new System.Windows.Forms.TreeView();
            this.saveConfigButton = new System.Windows.Forms.Button();
            pictureBox3 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(pictureBox3)).BeginInit();
            this.mainTabPanel.SuspendLayout();
            this.mainTabDropdownPanel.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.mainTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.editTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox3
            // 
            pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            pictureBox3.Location = new System.Drawing.Point(718, 479);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new System.Drawing.Size(200, 70);
            pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox3.TabIndex = 7;
            pictureBox3.TabStop = false;
            // 
            // appTitleLable
            // 
            this.appTitleLable.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.appTitleLable.Font = new System.Drawing.Font("Calibri", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appTitleLable.Location = new System.Drawing.Point(64, 3);
            this.appTitleLable.Name = "appTitleLable";
            this.appTitleLable.Size = new System.Drawing.Size(780, 63);
            this.appTitleLable.TabIndex = 0;
            this.appTitleLable.Text = "SoniFight v0.6";
            this.appTitleLable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.appTitleLable.Click += new System.EventHandler(this.appTitleLable_Click);
            // 
            // configsComboBox
            // 
            this.configsComboBox.AccessibleDescription = "The game config dropdown allows you to select a game config to run for the game y" +
    "ou wish to sonify.";
            this.configsComboBox.AccessibleName = "Game Config Drop Down List";
            this.configsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.configsComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configsComboBox.FormattingEnabled = true;
            this.configsComboBox.Location = new System.Drawing.Point(3, 3);
            this.configsComboBox.Name = "configsComboBox";
            this.configsComboBox.Size = new System.Drawing.Size(900, 32);
            this.configsComboBox.TabIndex = 0;
            this.configsComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // refreshButton
            // 
            this.refreshButton.AccessibleDescription = "The refresh button refreshes the Config Dropdown List and can be used when new co" +
    "nfig directories have been added without having to restart the SoniFight program" +
    ".";
            this.refreshButton.AccessibleName = "Refresh button";
            this.refreshButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.refreshButton.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refreshButton.Location = new System.Drawing.Point(626, 41);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(277, 46);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // mainTabPanel
            // 
            this.mainTabPanel.AutoSize = true;
            this.mainTabPanel.ColumnCount = 1;
            this.mainTabPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTabPanel.Controls.Add(this.quitButton, 0, 4);
            this.mainTabPanel.Controls.Add(this.createNewConfigButton, 0, 3);
            this.mainTabPanel.Controls.Add(this.stopConfigButton, 0, 2);
            this.mainTabPanel.Controls.Add(this.runConfigButton, 0, 1);
            this.mainTabPanel.Controls.Add(this.mainTabDropdownPanel, 0, 0);
            this.mainTabPanel.Location = new System.Drawing.Point(6, 69);
            this.mainTabPanel.Name = "mainTabPanel";
            this.mainTabPanel.RowCount = 5;
            this.mainTabPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            this.mainTabPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.18182F));
            this.mainTabPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.18182F));
            this.mainTabPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.18182F));
            this.mainTabPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.18182F));
            this.mainTabPanel.Size = new System.Drawing.Size(912, 401);
            this.mainTabPanel.TabIndex = 3;
            // 
            // quitButton
            // 
            this.quitButton.AccessibleDescription = "The quit button exits the SoniFight application.";
            this.quitButton.AccessibleName = "Quit button";
            this.quitButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.quitButton.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.quitButton.Location = new System.Drawing.Point(3, 340);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(311, 46);
            this.quitButton.TabIndex = 4;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.exitButton_Click_1);
            // 
            // createNewConfigButton
            // 
            this.createNewConfigButton.AccessibleDescription = "Opens a form which allows users to create a new config to apply sonification to a" +
    " game";
            this.createNewConfigButton.AccessibleName = "Create new config button";
            this.createNewConfigButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createNewConfigButton.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.createNewConfigButton.Location = new System.Drawing.Point(3, 266);
            this.createNewConfigButton.Name = "createNewConfigButton";
            this.createNewConfigButton.Size = new System.Drawing.Size(311, 46);
            this.createNewConfigButton.TabIndex = 3;
            this.createNewConfigButton.Text = "Create New Config";
            this.createNewConfigButton.UseVisualStyleBackColor = true;
            this.createNewConfigButton.Click += new System.EventHandler(this.createNewConfigButton_Click);
            // 
            // stopConfigButton
            // 
            this.stopConfigButton.AccessibleDescription = "The stop running config button stops any running config so that it no longer prov" +
    "ides sonification.";
            this.stopConfigButton.AccessibleName = "Stop Running Config";
            this.stopConfigButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.stopConfigButton.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopConfigButton.Location = new System.Drawing.Point(3, 194);
            this.stopConfigButton.Name = "stopConfigButton";
            this.stopConfigButton.Size = new System.Drawing.Size(311, 46);
            this.stopConfigButton.TabIndex = 2;
            this.stopConfigButton.Text = "Stop Running Config";
            this.stopConfigButton.UseVisualStyleBackColor = true;
            this.stopConfigButton.Click += new System.EventHandler(this.stopConfigButton_Click);
            // 
            // runConfigButton
            // 
            this.runConfigButton.AccessibleDescription = "The run selected config button runs the config which is currently selected to pro" +
    "vide sonification events.";
            this.runConfigButton.AccessibleName = "Run Selected Config";
            this.runConfigButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.runConfigButton.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runConfigButton.Location = new System.Drawing.Point(3, 122);
            this.runConfigButton.Name = "runConfigButton";
            this.runConfigButton.Size = new System.Drawing.Size(311, 46);
            this.runConfigButton.TabIndex = 1;
            this.runConfigButton.Text = "Run Selected Config";
            this.runConfigButton.UseVisualStyleBackColor = true;
            this.runConfigButton.Click += new System.EventHandler(this.runConfig_Click);
            // 
            // mainTabDropdownPanel
            // 
            this.mainTabDropdownPanel.Controls.Add(this.refreshButton);
            this.mainTabDropdownPanel.Controls.Add(this.configsComboBox);
            this.mainTabDropdownPanel.Location = new System.Drawing.Point(3, 3);
            this.mainTabDropdownPanel.Name = "mainTabDropdownPanel";
            this.mainTabDropdownPanel.Size = new System.Drawing.Size(906, 98);
            this.mainTabDropdownPanel.TabIndex = 4;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.mainTabPage);
            this.tabControl.Controls.Add(this.editTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.MinimumSize = new System.Drawing.Size(800, 400);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(931, 580);
            this.tabControl.TabIndex = 6;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // mainTabPage
            // 
            this.mainTabPage.Controls.Add(pictureBox3);
            this.mainTabPage.Controls.Add(this.pictureBox2);
            this.mainTabPage.Controls.Add(this.pictureBox1);
            this.mainTabPage.Controls.Add(this.appTitleLable);
            this.mainTabPage.Controls.Add(this.mainTabPanel);
            this.mainTabPage.Location = new System.Drawing.Point(4, 22);
            this.mainTabPage.Name = "mainTabPage";
            this.mainTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.mainTabPage.Size = new System.Drawing.Size(923, 554);
            this.mainTabPage.TabIndex = 0;
            this.mainTabPage.Text = "Main";
            this.mainTabPage.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(295, 476);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(344, 70);
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(467, 176);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(340, 314);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // editTabPage
            // 
            this.editTabPage.AccessibleDescription = "Tab containing controls where you can create, edit and save new game configs.";
            this.editTabPage.AccessibleName = "Edit Config Tab";
            this.editTabPage.AutoScroll = true;
            this.editTabPage.Controls.Add(this.cloneWatchButton);
            this.editTabPage.Controls.Add(this.cloneTriggerButton);
            this.editTabPage.Controls.Add(this.currentUILabel);
            this.editTabPage.Controls.Add(this.addWatchButton);
            this.editTabPage.Controls.Add(this.addTriggerButton);
            this.editTabPage.Controls.Add(this.gcPanel);
            this.editTabPage.Controls.Add(this.gcTreeView);
            this.editTabPage.Controls.Add(this.saveConfigButton);
            this.editTabPage.Location = new System.Drawing.Point(4, 22);
            this.editTabPage.Name = "editTabPage";
            this.editTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.editTabPage.Size = new System.Drawing.Size(923, 554);
            this.editTabPage.TabIndex = 1;
            this.editTabPage.Text = "Edit Config";
            this.editTabPage.UseVisualStyleBackColor = true;
            // 
            // cloneWatchButton
            // 
            this.cloneWatchButton.AccessibleDescription = "This button clones the currently selected watch, providing a unique ID and append" +
    "ing the word CLONE to the watch name.";
            this.cloneWatchButton.AccessibleName = "Clone Watch Button";
            this.cloneWatchButton.Location = new System.Drawing.Point(7, 379);
            this.cloneWatchButton.Name = "cloneWatchButton";
            this.cloneWatchButton.Size = new System.Drawing.Size(319, 35);
            this.cloneWatchButton.TabIndex = 14;
            this.cloneWatchButton.Text = "Clone Current Watch";
            this.cloneWatchButton.UseVisualStyleBackColor = true;
            this.cloneWatchButton.Click += new System.EventHandler(this.cloneWatchButton_Click);
            // 
            // cloneTriggerButton
            // 
            this.cloneTriggerButton.AccessibleDescription = "This button clones the current trigger to a new trigger, appending the word clone" +
    " to the title of the new trigger.  Use this to quickly create new triggers that " +
    "depend on the same watch.";
            this.cloneTriggerButton.AccessibleName = "Clone Trigger Button";
            this.cloneTriggerButton.Location = new System.Drawing.Point(7, 461);
            this.cloneTriggerButton.Name = "cloneTriggerButton";
            this.cloneTriggerButton.Size = new System.Drawing.Size(319, 35);
            this.cloneTriggerButton.TabIndex = 13;
            this.cloneTriggerButton.Text = "Clone Current Trigger";
            this.cloneTriggerButton.UseVisualStyleBackColor = true;
            this.cloneTriggerButton.Click += new System.EventHandler(this.cloneTriggerButton_Click);
            // 
            // currentUILabel
            // 
            this.currentUILabel.AutoSize = true;
            this.currentUILabel.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentUILabel.Location = new System.Drawing.Point(329, 6);
            this.currentUILabel.Margin = new System.Windows.Forms.Padding(0);
            this.currentUILabel.Name = "currentUILabel";
            this.currentUILabel.Padding = new System.Windows.Forms.Padding(3);
            this.currentUILabel.Size = new System.Drawing.Size(168, 35);
            this.currentUILabel.TabIndex = 12;
            this.currentUILabel.Text = "CurrentUILabel";
            // 
            // addWatchButton
            // 
            this.addWatchButton.Location = new System.Drawing.Point(7, 338);
            this.addWatchButton.Name = "addWatchButton";
            this.addWatchButton.Size = new System.Drawing.Size(319, 35);
            this.addWatchButton.TabIndex = 11;
            this.addWatchButton.Text = "Add Watch";
            this.addWatchButton.UseVisualStyleBackColor = true;
            this.addWatchButton.Click += new System.EventHandler(this.addWatchButton_Click);
            // 
            // addTriggerButton
            // 
            this.addTriggerButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addTriggerButton.Location = new System.Drawing.Point(7, 420);
            this.addTriggerButton.Name = "addTriggerButton";
            this.addTriggerButton.Size = new System.Drawing.Size(319, 35);
            this.addTriggerButton.TabIndex = 10;
            this.addTriggerButton.Text = "Add Trigger";
            this.addTriggerButton.UseVisualStyleBackColor = true;
            this.addTriggerButton.Click += new System.EventHandler(this.addTriggerButton_Click);
            // 
            // gcPanel
            // 
            this.gcPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.gcPanel.AutoSize = true;
            this.gcPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gcPanel.BackColor = System.Drawing.Color.AntiqueWhite;
            this.gcPanel.ColumnCount = 2;
            this.gcPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.gcPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 335F));
            this.gcPanel.Location = new System.Drawing.Point(253, 44);
            this.gcPanel.Name = "gcPanel";
            this.gcPanel.RowCount = 1;
            this.gcPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gcPanel.Size = new System.Drawing.Size(335, 0);
            this.gcPanel.TabIndex = 9;
            // 
            // gcTreeView
            // 
            this.gcTreeView.Location = new System.Drawing.Point(6, 6);
            this.gcTreeView.Name = "gcTreeView";
            treeNode1.Name = "WatchesNode";
            treeNode1.Tag = "Watches";
            treeNode1.Text = "Watches";
            treeNode2.Name = "TriggersNode";
            treeNode2.Tag = "Triggers";
            treeNode2.Text = "Triggers";
            treeNode3.Checked = true;
            treeNode3.Name = "gcRootNode";
            treeNode3.Tag = "GameConfig";
            treeNode3.Text = "GameConfig";
            this.gcTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3});
            this.gcTreeView.ShowPlusMinus = false;
            this.gcTreeView.Size = new System.Drawing.Size(320, 326);
            this.gcTreeView.TabIndex = 2;
            this.gcTreeView.Tag = "gcTreeView";
            this.gcTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.gcTreeView_AfterSelect);
            // 
            // saveConfigButton
            // 
            this.saveConfigButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.saveConfigButton.Location = new System.Drawing.Point(7, 502);
            this.saveConfigButton.Name = "saveConfigButton";
            this.saveConfigButton.Size = new System.Drawing.Size(319, 35);
            this.saveConfigButton.TabIndex = 8;
            this.saveConfigButton.Text = "Save GameConfig";
            this.saveConfigButton.UseVisualStyleBackColor = true;
            this.saveConfigButton.Click += new System.EventHandler(this.saveConfig_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(931, 580);
            this.Controls.Add(this.tabControl);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "Fair Fight v0.1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(pictureBox3)).EndInit();
            this.mainTabPanel.ResumeLayout(false);
            this.mainTabDropdownPanel.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.mainTabPage.ResumeLayout(false);
            this.mainTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.editTabPage.ResumeLayout(false);
            this.editTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        private void configNameLabel_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.Label appTitleLable;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.TableLayoutPanel mainTabPanel;
        private System.Windows.Forms.Button runConfigButton;
        private System.Windows.Forms.ComboBox configsComboBox;
        private System.Windows.Forms.Button stopConfigButton;
        private System.Windows.Forms.Button createNewConfigButton;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.TabPage mainTabPage;
        private System.Windows.Forms.TabPage editTabPage;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Panel mainTabDropdownPanel;
        private System.Windows.Forms.Button saveConfigButton;
        private System.Windows.Forms.TreeView gcTreeView;
        private System.Windows.Forms.TableLayoutPanel gcPanel;
        private System.Windows.Forms.Button addTriggerButton;
        private System.Windows.Forms.Button addWatchButton;
        private System.Windows.Forms.Label currentUILabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button cloneTriggerButton;
        private System.Windows.Forms.Button cloneWatchButton;
        private System.Windows.Forms.PictureBox pictureBox2;
        //private System.Windows.Forms.TableLayoutPanel innerWatchTLPanel;
    }
}


