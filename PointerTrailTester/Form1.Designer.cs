namespace PointerTrailTester
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.processNameLabel = new System.Windows.Forms.Label();
            this.pointerTrailLabel = new System.Windows.Forms.Label();
            this.dataTypeLabel = new System.Windows.Forms.Label();
            this.memoryAddressLabel = new System.Windows.Forms.Label();
            this.valueLabel = new System.Windows.Forms.Label();
            this.titleLabel = new System.Windows.Forms.Label();
            this.processTB = new System.Windows.Forms.TextBox();
            this.pointerTrailTB = new System.Windows.Forms.TextBox();
            this.memoryAddressTB = new System.Windows.Forms.TextBox();
            this.valueTB = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.pointerTrailTB, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.processNameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pointerTrailLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dataTypeLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.memoryAddressLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.valueLabel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.processTB, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.memoryAddressTB, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.valueTB, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.comboBox1, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 56);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(541, 193);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // processNameLabel
            // 
            this.processNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.processNameLabel.AutoSize = true;
            this.processNameLabel.Location = new System.Drawing.Point(74, 2);
            this.processNameLabel.Name = "processNameLabel";
            this.processNameLabel.Padding = new System.Windows.Forms.Padding(10);
            this.processNameLabel.Size = new System.Drawing.Size(193, 33);
            this.processNameLabel.TabIndex = 0;
            this.processNameLabel.Text = "Process Name (without .EXE suffix)";
            this.processNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pointerTrailLabel
            // 
            this.pointerTrailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pointerTrailLabel.AutoSize = true;
            this.pointerTrailLabel.Location = new System.Drawing.Point(68, 40);
            this.pointerTrailLabel.Name = "pointerTrailLabel";
            this.pointerTrailLabel.Padding = new System.Windows.Forms.Padding(10);
            this.pointerTrailLabel.Size = new System.Drawing.Size(199, 33);
            this.pointerTrailLabel.TabIndex = 1;
            this.pointerTrailLabel.Text = "Pointer Trail (hex, comma separated)";
            this.pointerTrailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dataTypeLabel
            // 
            this.dataTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.dataTypeLabel.AutoSize = true;
            this.dataTypeLabel.Location = new System.Drawing.Point(190, 78);
            this.dataTypeLabel.Name = "dataTypeLabel";
            this.dataTypeLabel.Padding = new System.Windows.Forms.Padding(10);
            this.dataTypeLabel.Size = new System.Drawing.Size(77, 33);
            this.dataTypeLabel.TabIndex = 2;
            this.dataTypeLabel.Text = "Data Type";
            this.dataTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // memoryAddressLabel
            // 
            this.memoryAddressLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.memoryAddressLabel.AutoSize = true;
            this.memoryAddressLabel.Location = new System.Drawing.Point(134, 116);
            this.memoryAddressLabel.Name = "memoryAddressLabel";
            this.memoryAddressLabel.Padding = new System.Windows.Forms.Padding(10);
            this.memoryAddressLabel.Size = new System.Drawing.Size(133, 33);
            this.memoryAddressLabel.TabIndex = 3;
            this.memoryAddressLabel.Text = "Memory Address (Hex)";
            this.memoryAddressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // valueLabel
            // 
            this.valueLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(213, 156);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Padding = new System.Windows.Forms.Padding(10);
            this.valueLabel.Size = new System.Drawing.Size(54, 33);
            this.valueLabel.TabIndex = 4;
            this.valueLabel.Text = "Value";
            this.valueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // titleLabel
            // 
            this.titleLabel.Enabled = false;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(13, 13);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(540, 40);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "SoniFight Pointer Trail Tester v0.1";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // processTB
            // 
            this.processTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.processTB.Location = new System.Drawing.Point(273, 9);
            this.processTB.MaxLength = 256;
            this.processTB.Name = "processTB";
            this.processTB.Size = new System.Drawing.Size(265, 20);
            this.processTB.TabIndex = 5;
            this.processTB.TextChanged += new System.EventHandler(this.processTB_TextChanged);
            // 
            // pointerTrailTB
            // 
            this.pointerTrailTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pointerTrailTB.Location = new System.Drawing.Point(273, 47);
            this.pointerTrailTB.MaxLength = 256;
            this.pointerTrailTB.Name = "pointerTrailTB";
            this.pointerTrailTB.Size = new System.Drawing.Size(265, 20);
            this.pointerTrailTB.TabIndex = 6;
            // 
            // memoryAddressTB
            // 
            this.memoryAddressTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.memoryAddressTB.Location = new System.Drawing.Point(273, 123);
            this.memoryAddressTB.MaxLength = 256;
            this.memoryAddressTB.Name = "memoryAddressTB";
            this.memoryAddressTB.ReadOnly = true;
            this.memoryAddressTB.Size = new System.Drawing.Size(265, 20);
            this.memoryAddressTB.TabIndex = 7;
            // 
            // valueTB
            // 
            this.valueTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.valueTB.Location = new System.Drawing.Point(273, 162);
            this.valueTB.MaxLength = 256;
            this.valueTB.Name = "valueTB";
            this.valueTB.ReadOnly = true;
            this.valueTB.Size = new System.Drawing.Size(265, 20);
            this.valueTB.TabIndex = 8;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Integer",
            "Float",
            "Short",
            "Double",
            "Boolean",
            "String (UTF-8)",
            "String (UTF-16)"});
            this.comboBox1.Location = new System.Drawing.Point(273, 84);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(265, 21);
            this.comboBox1.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 261);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label processNameLabel;
        private System.Windows.Forms.Label pointerTrailLabel;
        private System.Windows.Forms.Label dataTypeLabel;
        private System.Windows.Forms.Label memoryAddressLabel;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.TextBox processTB;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.TextBox pointerTrailTB;
        private System.Windows.Forms.TextBox memoryAddressTB;
        private System.Windows.Forms.TextBox valueTB;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

