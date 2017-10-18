namespace au.edu.federation.SoniFight
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
            System.Windows.Forms.PictureBox irrKlangPictureBox;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.PictureBox fedUniPictureBox;
            this.tabControl = new System.Windows.Forms.TabControl();
            this.mainTabPage = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.quitButton = new System.Windows.Forms.Button();
            this.createConfigButton = new System.Windows.Forms.Button();
            this.stopConfigButton = new System.Windows.Forms.Button();
            this.runConfigButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.configsComboBox = new System.Windows.Forms.ComboBox();
            this.appTitleLabel = new System.Windows.Forms.Label();
            this.editTabPage = new System.Windows.Forms.TabPage();
            this.holderPanel = new System.Windows.Forms.Panel();
            this.gcPanel = new System.Windows.Forms.TableLayoutPanel();
            this.saveConfigButton = new System.Windows.Forms.Button();
            this.cloneTriggerButton = new System.Windows.Forms.Button();
            this.addTriggerButton = new System.Windows.Forms.Button();
            this.cloneWatchButton = new System.Windows.Forms.Button();
            this.addWatchButton = new System.Windows.Forms.Button();
            this.currentUILabel = new System.Windows.Forms.Label();
            this.gcTreeView = new System.Windows.Forms.TreeView();
            irrKlangPictureBox = new System.Windows.Forms.PictureBox();
            fedUniPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(irrKlangPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(fedUniPictureBox)).BeginInit();
            this.tabControl.SuspendLayout();
            this.mainTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.editTabPage.SuspendLayout();
            this.holderPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // irrKlangPictureBox
            // 
            resources.ApplyResources(irrKlangPictureBox, "irrKlangPictureBox");
            irrKlangPictureBox.Name = "irrKlangPictureBox";
            irrKlangPictureBox.TabStop = false;
            // 
            // fedUniPictureBox
            // 
            resources.ApplyResources(fedUniPictureBox, "fedUniPictureBox");
            fedUniPictureBox.Name = "fedUniPictureBox";
            fedUniPictureBox.TabStop = false;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.mainTabPage);
            this.tabControl.Controls.Add(this.editTabPage);
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // mainTabPage
            // 
            this.mainTabPage.Controls.Add(this.pictureBox1);
            this.mainTabPage.Controls.Add(fedUniPictureBox);
            this.mainTabPage.Controls.Add(irrKlangPictureBox);
            this.mainTabPage.Controls.Add(this.quitButton);
            this.mainTabPage.Controls.Add(this.createConfigButton);
            this.mainTabPage.Controls.Add(this.stopConfigButton);
            this.mainTabPage.Controls.Add(this.runConfigButton);
            this.mainTabPage.Controls.Add(this.refreshButton);
            this.mainTabPage.Controls.Add(this.configsComboBox);
            this.mainTabPage.Controls.Add(this.appTitleLabel);
            resources.ApplyResources(this.mainTabPage, "mainTabPage");
            this.mainTabPage.Name = "mainTabPage";
            this.mainTabPage.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // quitButton
            // 
            resources.ApplyResources(this.quitButton, "quitButton");
            this.quitButton.Name = "quitButton";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // createConfigButton
            // 
            resources.ApplyResources(this.createConfigButton, "createConfigButton");
            this.createConfigButton.Name = "createConfigButton";
            this.createConfigButton.UseVisualStyleBackColor = true;
            this.createConfigButton.Click += new System.EventHandler(this.createNewConfigButton_Click);
            // 
            // stopConfigButton
            // 
            resources.ApplyResources(this.stopConfigButton, "stopConfigButton");
            this.stopConfigButton.Name = "stopConfigButton";
            this.stopConfigButton.UseVisualStyleBackColor = true;
            this.stopConfigButton.Click += new System.EventHandler(this.stopConfigButton_Click);
            // 
            // runConfigButton
            // 
            resources.ApplyResources(this.runConfigButton, "runConfigButton");
            this.runConfigButton.Name = "runConfigButton";
            this.runConfigButton.UseVisualStyleBackColor = true;
            this.runConfigButton.Click += new System.EventHandler(this.runConfig_Click);
            // 
            // refreshButton
            // 
            resources.ApplyResources(this.refreshButton, "refreshButton");
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // configsComboBox
            // 
            this.configsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.configsComboBox, "configsComboBox");
            this.configsComboBox.FormattingEnabled = true;
            this.configsComboBox.Name = "configsComboBox";
            this.configsComboBox.SelectedIndexChanged += new System.EventHandler(this.configsComboBox_SelectedIndexChanged);
            // 
            // appTitleLabel
            // 
            resources.ApplyResources(this.appTitleLabel, "appTitleLabel");
            this.appTitleLabel.Name = "appTitleLabel";
            // 
            // editTabPage
            // 
            this.editTabPage.Controls.Add(this.holderPanel);
            this.editTabPage.Controls.Add(this.saveConfigButton);
            this.editTabPage.Controls.Add(this.cloneTriggerButton);
            this.editTabPage.Controls.Add(this.addTriggerButton);
            this.editTabPage.Controls.Add(this.cloneWatchButton);
            this.editTabPage.Controls.Add(this.addWatchButton);
            this.editTabPage.Controls.Add(this.currentUILabel);
            this.editTabPage.Controls.Add(this.gcTreeView);
            resources.ApplyResources(this.editTabPage, "editTabPage");
            this.editTabPage.Name = "editTabPage";
            this.editTabPage.UseVisualStyleBackColor = true;
            // 
            // holderPanel
            // 
            this.holderPanel.Controls.Add(this.gcPanel);
            resources.ApplyResources(this.holderPanel, "holderPanel");
            this.holderPanel.Name = "holderPanel";
            // 
            // gcPanel
            // 
            this.gcPanel.BackColor = System.Drawing.Color.AntiqueWhite;
            resources.ApplyResources(this.gcPanel, "gcPanel");
            this.gcPanel.Name = "gcPanel";
            // 
            // saveConfigButton
            // 
            resources.ApplyResources(this.saveConfigButton, "saveConfigButton");
            this.saveConfigButton.Name = "saveConfigButton";
            this.saveConfigButton.UseVisualStyleBackColor = true;
            this.saveConfigButton.Click += new System.EventHandler(this.saveConfig_Click);
            // 
            // cloneTriggerButton
            // 
            resources.ApplyResources(this.cloneTriggerButton, "cloneTriggerButton");
            this.cloneTriggerButton.Name = "cloneTriggerButton";
            this.cloneTriggerButton.UseVisualStyleBackColor = true;
            this.cloneTriggerButton.Click += new System.EventHandler(this.cloneTriggerButton_Click);
            // 
            // addTriggerButton
            // 
            resources.ApplyResources(this.addTriggerButton, "addTriggerButton");
            this.addTriggerButton.Name = "addTriggerButton";
            this.addTriggerButton.UseVisualStyleBackColor = true;
            this.addTriggerButton.Click += new System.EventHandler(this.addTriggerButton_Click);
            // 
            // cloneWatchButton
            // 
            resources.ApplyResources(this.cloneWatchButton, "cloneWatchButton");
            this.cloneWatchButton.Name = "cloneWatchButton";
            this.cloneWatchButton.UseVisualStyleBackColor = true;
            this.cloneWatchButton.Click += new System.EventHandler(this.cloneWatchButton_Click);
            // 
            // addWatchButton
            // 
            resources.ApplyResources(this.addWatchButton, "addWatchButton");
            this.addWatchButton.Name = "addWatchButton";
            this.addWatchButton.UseVisualStyleBackColor = true;
            this.addWatchButton.Click += new System.EventHandler(this.addWatchButton_Click);
            // 
            // currentUILabel
            // 
            resources.ApplyResources(this.currentUILabel, "currentUILabel");
            this.currentUILabel.Name = "currentUILabel";
            // 
            // gcTreeView
            // 
            resources.ApplyResources(this.gcTreeView, "gcTreeView");
            this.gcTreeView.Name = "gcTreeView";
            this.gcTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.gcTreeView_AfterSelect);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)(irrKlangPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(fedUniPictureBox)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.mainTabPage.ResumeLayout(false);
            this.mainTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.editTabPage.ResumeLayout(false);
            this.editTabPage.PerformLayout();
            this.holderPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage mainTabPage;
        private System.Windows.Forms.TabPage editTabPage;
        private System.Windows.Forms.Label appTitleLabel;
        private System.Windows.Forms.ComboBox configsComboBox;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.Button createConfigButton;
        private System.Windows.Forms.Button stopConfigButton;
        private System.Windows.Forms.Button runConfigButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.TreeView gcTreeView;
        private System.Windows.Forms.Label currentUILabel;
        private System.Windows.Forms.Button saveConfigButton;
        private System.Windows.Forms.Button cloneTriggerButton;
        private System.Windows.Forms.Button addTriggerButton;
        private System.Windows.Forms.Button cloneWatchButton;
        private System.Windows.Forms.Button addWatchButton;
        private System.Windows.Forms.Panel holderPanel;

        private System.Windows.Forms.TableLayoutPanel gcPanel;
        //private au.edu.federation.SoniFight.CoTableLayoutPanel gcPanel;

        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

