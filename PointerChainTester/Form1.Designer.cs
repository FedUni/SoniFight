namespace au.edu.federation.PointerChainTester
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.processNameLabel = new System.Windows.Forms.Label();
            this.processNameTextBox = new System.Windows.Forms.TextBox();
            this.pointerChainTextBox = new System.Windows.Forms.TextBox();
            this.memoryAddressTextBox = new System.Windows.Forms.TextBox();
            this.pointerTrailLabel = new System.Windows.Forms.Label();
            this.dataTypeLabel = new System.Windows.Forms.Label();
            this.memoryAddressLabel = new System.Windows.Forms.Label();
            this.dataTypeComboBox = new System.Windows.Forms.ComboBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.valueLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // processNameLabel
            // 
            resources.ApplyResources(this.processNameLabel, "processNameLabel");
            this.processNameLabel.Name = "processNameLabel";
            // 
            // processNameTextBox
            // 
            resources.ApplyResources(this.processNameTextBox, "processNameTextBox");
            this.processNameTextBox.Name = "processNameTextBox";
            // 
            // pointerTrailTextBox
            // 
            resources.ApplyResources(this.pointerChainTextBox, "pointerTrailTextBox");
            this.pointerChainTextBox.Name = "pointerTrailTextBox";
            // 
            // memoryAddressTextBox
            // 
            resources.ApplyResources(this.memoryAddressTextBox, "memoryAddressTextBox");
            this.memoryAddressTextBox.Name = "memoryAddressTextBox";
            this.memoryAddressTextBox.ReadOnly = true;
            // 
            // pointerTrailLabel
            // 
            resources.ApplyResources(this.pointerTrailLabel, "pointerTrailLabel");
            this.pointerTrailLabel.Name = "pointerTrailLabel";
            // 
            // dataTypeLabel
            // 
            resources.ApplyResources(this.dataTypeLabel, "dataTypeLabel");
            this.dataTypeLabel.Name = "dataTypeLabel";
            // 
            // memoryAddressLabel
            // 
            resources.ApplyResources(this.memoryAddressLabel, "memoryAddressLabel");
            this.memoryAddressLabel.Name = "memoryAddressLabel";
            // 
            // dataTypeComboBox
            // 
            this.dataTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataTypeComboBox.FormattingEnabled = true;
            this.dataTypeComboBox.Items.AddRange(new object[] {
            resources.GetString("dataTypeComboBox.Items"),
            resources.GetString("dataTypeComboBox.Items1"),
            resources.GetString("dataTypeComboBox.Items2"),
            resources.GetString("dataTypeComboBox.Items3"),
            resources.GetString("dataTypeComboBox.Items4"),
            resources.GetString("dataTypeComboBox.Items5"),
            resources.GetString("dataTypeComboBox.Items6"),
            resources.GetString("dataTypeComboBox.Items7")});
            resources.ApplyResources(this.dataTypeComboBox, "dataTypeComboBox");
            this.dataTypeComboBox.Name = "dataTypeComboBox";
            // 
            // titleLabel
            // 
            resources.ApplyResources(this.titleLabel, "titleLabel");
            this.titleLabel.Name = "titleLabel";
            // 
            // valueTextBox
            // 
            resources.ApplyResources(this.valueTextBox, "valueTextBox");
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.ReadOnly = true;
            // 
            // valueLabel
            // 
            resources.ApplyResources(this.valueLabel, "valueLabel");
            this.valueLabel.Name = "valueLabel";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.valueTextBox);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.dataTypeComboBox);
            this.Controls.Add(this.memoryAddressLabel);
            this.Controls.Add(this.dataTypeLabel);
            this.Controls.Add(this.pointerTrailLabel);
            this.Controls.Add(this.memoryAddressTextBox);
            this.Controls.Add(this.pointerChainTextBox);
            this.Controls.Add(this.processNameTextBox);
            this.Controls.Add(this.processNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label processNameLabel;
        private System.Windows.Forms.TextBox processNameTextBox;
        private System.Windows.Forms.TextBox pointerChainTextBox;
        private System.Windows.Forms.TextBox memoryAddressTextBox;
        private System.Windows.Forms.Label pointerTrailLabel;
        private System.Windows.Forms.Label dataTypeLabel;
        private System.Windows.Forms.Label memoryAddressLabel;
        private System.Windows.Forms.ComboBox dataTypeComboBox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

