namespace Lettuce
{
    partial class HardwareConfiguration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HardwareConfiguration));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.availableListBox = new System.Windows.Forms.ListBox();
            this.addDeviceButton = new System.Windows.Forms.Button();
            this.removeDeviceButton = new System.Windows.Forms.Button();
            this.selectedListBox = new System.Windows.Forms.ListBox();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.saveDevicesCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hardware Configuration";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Available Devices";
            // 
            // availableListBox
            // 
            this.availableListBox.FormattingEnabled = true;
            this.availableListBox.Location = new System.Drawing.Point(12, 45);
            this.availableListBox.Name = "availableListBox";
            this.availableListBox.Size = new System.Drawing.Size(177, 199);
            this.availableListBox.TabIndex = 2;
            this.availableListBox.SelectedIndexChanged += new System.EventHandler(this.availableListBox_SelectedIndexChanged);
            // 
            // addDeviceButton
            // 
            this.addDeviceButton.Enabled = false;
            this.addDeviceButton.Location = new System.Drawing.Point(195, 110);
            this.addDeviceButton.Name = "addDeviceButton";
            this.addDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.addDeviceButton.TabIndex = 3;
            this.addDeviceButton.Text = ">>";
            this.addDeviceButton.UseVisualStyleBackColor = true;
            this.addDeviceButton.Click += new System.EventHandler(this.addDeviceButton_Click);
            // 
            // removeDeviceButton
            // 
            this.removeDeviceButton.Enabled = false;
            this.removeDeviceButton.Location = new System.Drawing.Point(195, 139);
            this.removeDeviceButton.Name = "removeDeviceButton";
            this.removeDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.removeDeviceButton.TabIndex = 4;
            this.removeDeviceButton.Text = "<<";
            this.removeDeviceButton.UseVisualStyleBackColor = true;
            this.removeDeviceButton.Click += new System.EventHandler(this.removeDeviceButton_Click);
            // 
            // selectedListBox
            // 
            this.selectedListBox.FormattingEnabled = true;
            this.selectedListBox.Location = new System.Drawing.Point(276, 45);
            this.selectedListBox.Name = "selectedListBox";
            this.selectedListBox.Size = new System.Drawing.Size(175, 199);
            this.selectedListBox.TabIndex = 5;
            this.selectedListBox.SelectedIndexChanged += new System.EventHandler(this.selectedListBox_SelectedIndexChanged);
            // 
            // moveUpButton
            // 
            this.moveUpButton.Enabled = false;
            this.moveUpButton.Location = new System.Drawing.Point(457, 45);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(75, 23);
            this.moveUpButton.TabIndex = 6;
            this.moveUpButton.Text = "Move up";
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // moveDownButton
            // 
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Location = new System.Drawing.Point(457, 74);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(75, 23);
            this.moveDownButton.TabIndex = 7;
            this.moveDownButton.Text = "Move down";
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(457, 221);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Accept";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(273, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Installed Devices";
            // 
            // saveDevicesCheckBox
            // 
            this.saveDevicesCheckBox.AutoSize = true;
            this.saveDevicesCheckBox.Location = new System.Drawing.Point(457, 198);
            this.saveDevicesCheckBox.Name = "saveDevicesCheckBox";
            this.saveDevicesCheckBox.Size = new System.Drawing.Size(77, 17);
            this.saveDevicesCheckBox.TabIndex = 10;
            this.saveDevicesCheckBox.Text = "Remember";
            this.saveDevicesCheckBox.UseVisualStyleBackColor = true;
            // 
            // HardwareConfiguration
            // 
            this.AcceptButton = this.button5;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 256);
            this.Controls.Add(this.saveDevicesCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.moveDownButton);
            this.Controls.Add(this.moveUpButton);
            this.Controls.Add(this.selectedListBox);
            this.Controls.Add(this.removeDeviceButton);
            this.Controls.Add(this.addDeviceButton);
            this.Controls.Add(this.availableListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HardwareConfiguration";
            this.Text = "Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox availableListBox;
        private System.Windows.Forms.Button addDeviceButton;
        private System.Windows.Forms.Button removeDeviceButton;
        private System.Windows.Forms.ListBox selectedListBox;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox saveDevicesCheckBox;
    }
}

