namespace BluetoothLights
{
    partial class controller
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.connectComBox = new System.Windows.Forms.ToolStripComboBox();
            this.connectCom = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.disconnectCom = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectComBox});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip1.Size = new System.Drawing.Size(284, 27);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // connectComBox
            // 
            this.connectComBox.Name = "connectComBox";
            this.connectComBox.Size = new System.Drawing.Size(121, 23);
            // 
            // connectCom
            // 
            this.connectCom.Location = new System.Drawing.Point(145, 3);
            this.connectCom.Name = "connectCom";
            this.connectCom.Size = new System.Drawing.Size(75, 23);
            this.connectCom.TabIndex = 1;
            this.connectCom.Text = "connect";
            this.connectCom.UseVisualStyleBackColor = true;
            this.connectCom.Click += new System.EventHandler(this.connectCom_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 240);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(284, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusBar
            // 
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(38, 17);
            this.statusBar.Text = "status";
            // 
            // disconnectCom
            // 
            this.disconnectCom.Location = new System.Drawing.Point(226, 3);
            this.disconnectCom.Name = "disconnectCom";
            this.disconnectCom.Size = new System.Drawing.Size(75, 23);
            this.disconnectCom.TabIndex = 3;
            this.disconnectCom.Text = "disconnect";
            this.disconnectCom.UseVisualStyleBackColor = true;
            this.disconnectCom.Click += new System.EventHandler(this.disconnectCom_Click);
            // 
            // controller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.disconnectCom);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.connectCom);
            this.Controls.Add(this.menuStrip1);
            this.Icon = global::BluetoothLights.SharedRessources.meister_lampe;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "controller";
            this.Text = "controller";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripComboBox connectComBox;
        private System.Windows.Forms.Button connectCom;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusBar;
        private System.Windows.Forms.Button disconnectCom;

    }
}