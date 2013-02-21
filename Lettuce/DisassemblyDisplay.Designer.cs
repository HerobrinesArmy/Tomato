namespace Lettuce
{
    partial class DisassemblyDisplay
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gotoAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPCToAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gotoAddressToolStripMenuItem,
            this.setPCToAddressToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(168, 48);
            // 
            // gotoAddressToolStripMenuItem
            // 
            this.gotoAddressToolStripMenuItem.Name = "gotoAddressToolStripMenuItem";
            this.gotoAddressToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.gotoAddressToolStripMenuItem.Text = "Go To Address";
            this.gotoAddressToolStripMenuItem.Click += new System.EventHandler(this.gotoAddressToolStripMenuItem_Click);
            // 
            // setPCToAddressToolStripMenuItem
            // 
            this.setPCToAddressToolStripMenuItem.Name = "setPCToAddressToolStripMenuItem";
            this.setPCToAddressToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.setPCToAddressToolStripMenuItem.Text = "Set PC to Address";
            this.setPCToAddressToolStripMenuItem.Click += new System.EventHandler(this.setPCToAddressToolStripMenuItem_Click);
            // 
            // vScrollBar1
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(465, 0);
            this.vScrollBar.Name = "vScrollBar1";
            this.vScrollBar.Size = new System.Drawing.Size(17, 202);
            this.vScrollBar.TabIndex = 1;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // DisassemblyDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.vScrollBar);
            this.DoubleBuffered = true;
            this.Name = "DisassemblyDisplay";
            this.Size = new System.Drawing.Size(482, 202);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DisassemblyDisplay_Paint);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gotoAddressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPCToAddressToolStripMenuItem;
        private System.Windows.Forms.VScrollBar vScrollBar;
    }
}
