namespace bt_lights_GUI
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.status_connection = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tool_connect = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_options = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_options_connection = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gui_channelSlider = new System.Windows.Forms.GroupBox();
            this.comboBox9 = new System.Windows.Forms.ComboBox();
            this.comboBox10 = new System.Windows.Forms.ComboBox();
            this.comboBox5 = new System.Windows.Forms.ComboBox();
            this.comboBox6 = new System.Windows.Forms.ComboBox();
            this.comboBox7 = new System.Windows.Forms.ComboBox();
            this.comboBox8 = new System.Windows.Forms.ComboBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.trackBar6 = new System.Windows.Forms.TrackBar();
            this.trackBar7 = new System.Windows.Forms.TrackBar();
            this.trackBar8 = new System.Windows.Forms.TrackBar();
            this.trackBar9 = new System.Windows.Forms.TrackBar();
            this.trackBar10 = new System.Windows.Forms.TrackBar();
            this.trackBar5 = new System.Windows.Forms.TrackBar();
            this.trackBar4 = new System.Windows.Forms.TrackBar();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.master_mode = new System.Windows.Forms.ComboBox();
            this.master_value = new System.Windows.Forms.TrackBar();
            this.outputBox = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.gui_channelSlider.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.master_value)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_connection});
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // status_connection
            // 
            this.status_connection.Name = "status_connection";
            this.status_connection.Size = new System.Drawing.Size(146, 17);
            this.status_connection.Text = "Connection: disconnected";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tool_connect});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(784, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tool_connect
            // 
            this.tool_connect.CheckOnClick = true;
            this.tool_connect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tool_connect.Image = ((System.Drawing.Image)(resources.GetObject("tool_connect.Image")));
            this.tool_connect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tool_connect.Name = "tool_connect";
            this.tool_connect.Size = new System.Drawing.Size(23, 22);
            this.tool_connect.Text = "tool_connect";
            this.tool_connect.ToolTipText = "Connect to module";
            this.tool_connect.Click += new System.EventHandler(this.tool_connect_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.menu_options,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menu_options
            // 
            this.menu_options.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_options_connection});
            this.menu_options.Name = "menu_options";
            this.menu_options.Size = new System.Drawing.Size(61, 20);
            this.menu_options.Text = "Options";
            // 
            // menu_options_connection
            // 
            this.menu_options_connection.Name = "menu_options_connection";
            this.menu_options_connection.Size = new System.Drawing.Size(136, 22);
            this.menu_options_connection.Text = "Connection";
            this.menu_options_connection.Click += new System.EventHandler(this.menu_options_connection_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // gui_channelSlider
            // 
            this.gui_channelSlider.Controls.Add(this.comboBox9);
            this.gui_channelSlider.Controls.Add(this.comboBox10);
            this.gui_channelSlider.Controls.Add(this.comboBox5);
            this.gui_channelSlider.Controls.Add(this.comboBox6);
            this.gui_channelSlider.Controls.Add(this.comboBox7);
            this.gui_channelSlider.Controls.Add(this.comboBox8);
            this.gui_channelSlider.Controls.Add(this.comboBox3);
            this.gui_channelSlider.Controls.Add(this.comboBox4);
            this.gui_channelSlider.Controls.Add(this.comboBox2);
            this.gui_channelSlider.Controls.Add(this.comboBox1);
            this.gui_channelSlider.Controls.Add(this.trackBar6);
            this.gui_channelSlider.Controls.Add(this.trackBar7);
            this.gui_channelSlider.Controls.Add(this.trackBar8);
            this.gui_channelSlider.Controls.Add(this.trackBar9);
            this.gui_channelSlider.Controls.Add(this.trackBar10);
            this.gui_channelSlider.Controls.Add(this.trackBar5);
            this.gui_channelSlider.Controls.Add(this.trackBar4);
            this.gui_channelSlider.Controls.Add(this.trackBar3);
            this.gui_channelSlider.Controls.Add(this.trackBar2);
            this.gui_channelSlider.Controls.Add(this.trackBar1);
            this.gui_channelSlider.Location = new System.Drawing.Point(12, 52);
            this.gui_channelSlider.Name = "gui_channelSlider";
            this.gui_channelSlider.Size = new System.Drawing.Size(760, 318);
            this.gui_channelSlider.TabIndex = 3;
            this.gui_channelSlider.TabStop = false;
            this.gui_channelSlider.Text = "Channel slider";
            // 
            // comboBox9
            // 
            this.comboBox9.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox9.FormattingEnabled = true;
            this.comboBox9.Location = new System.Drawing.Point(681, 271);
            this.comboBox9.Name = "comboBox9";
            this.comboBox9.Size = new System.Drawing.Size(65, 21);
            this.comboBox9.TabIndex = 19;
            // 
            // comboBox10
            // 
            this.comboBox10.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox10.FormattingEnabled = true;
            this.comboBox10.Location = new System.Drawing.Point(606, 271);
            this.comboBox10.Name = "comboBox10";
            this.comboBox10.Size = new System.Drawing.Size(65, 21);
            this.comboBox10.TabIndex = 18;
            // 
            // comboBox5
            // 
            this.comboBox5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox5.FormattingEnabled = true;
            this.comboBox5.Location = new System.Drawing.Point(531, 271);
            this.comboBox5.Name = "comboBox5";
            this.comboBox5.Size = new System.Drawing.Size(65, 21);
            this.comboBox5.TabIndex = 17;
            // 
            // comboBox6
            // 
            this.comboBox6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox6.FormattingEnabled = true;
            this.comboBox6.Location = new System.Drawing.Point(456, 271);
            this.comboBox6.Name = "comboBox6";
            this.comboBox6.Size = new System.Drawing.Size(65, 21);
            this.comboBox6.TabIndex = 16;
            // 
            // comboBox7
            // 
            this.comboBox7.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox7.FormattingEnabled = true;
            this.comboBox7.Location = new System.Drawing.Point(381, 271);
            this.comboBox7.Name = "comboBox7";
            this.comboBox7.Size = new System.Drawing.Size(65, 21);
            this.comboBox7.TabIndex = 15;
            // 
            // comboBox8
            // 
            this.comboBox8.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox8.FormattingEnabled = true;
            this.comboBox8.Location = new System.Drawing.Point(306, 271);
            this.comboBox8.Name = "comboBox8";
            this.comboBox8.Size = new System.Drawing.Size(65, 21);
            this.comboBox8.TabIndex = 14;
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(231, 271);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(65, 21);
            this.comboBox3.TabIndex = 13;
            // 
            // comboBox4
            // 
            this.comboBox4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Location = new System.Drawing.Point(156, 271);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(65, 21);
            this.comboBox4.TabIndex = 12;
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(81, 271);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(65, 21);
            this.comboBox2.TabIndex = 11;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 271);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(65, 21);
            this.comboBox1.TabIndex = 10;
            // 
            // trackBar6
            // 
            this.trackBar6.Location = new System.Drawing.Point(381, 19);
            this.trackBar6.Maximum = 255;
            this.trackBar6.Name = "trackBar6";
            this.trackBar6.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar6.Size = new System.Drawing.Size(45, 236);
            this.trackBar6.TabIndex = 9;
            this.trackBar6.TickFrequency = 15;
            // 
            // trackBar7
            // 
            this.trackBar7.Location = new System.Drawing.Point(456, 19);
            this.trackBar7.Maximum = 255;
            this.trackBar7.Name = "trackBar7";
            this.trackBar7.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar7.Size = new System.Drawing.Size(45, 236);
            this.trackBar7.TabIndex = 8;
            this.trackBar7.TickFrequency = 15;
            // 
            // trackBar8
            // 
            this.trackBar8.Location = new System.Drawing.Point(531, 19);
            this.trackBar8.Maximum = 255;
            this.trackBar8.Name = "trackBar8";
            this.trackBar8.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar8.Size = new System.Drawing.Size(45, 236);
            this.trackBar8.TabIndex = 7;
            this.trackBar8.TickFrequency = 15;
            // 
            // trackBar9
            // 
            this.trackBar9.Location = new System.Drawing.Point(606, 19);
            this.trackBar9.Maximum = 255;
            this.trackBar9.Name = "trackBar9";
            this.trackBar9.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar9.Size = new System.Drawing.Size(45, 236);
            this.trackBar9.TabIndex = 6;
            this.trackBar9.TickFrequency = 15;
            // 
            // trackBar10
            // 
            this.trackBar10.Location = new System.Drawing.Point(681, 19);
            this.trackBar10.Maximum = 255;
            this.trackBar10.Name = "trackBar10";
            this.trackBar10.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar10.Size = new System.Drawing.Size(45, 236);
            this.trackBar10.TabIndex = 5;
            this.trackBar10.TickFrequency = 15;
            // 
            // trackBar5
            // 
            this.trackBar5.Location = new System.Drawing.Point(306, 19);
            this.trackBar5.Maximum = 255;
            this.trackBar5.Name = "trackBar5";
            this.trackBar5.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar5.Size = new System.Drawing.Size(45, 236);
            this.trackBar5.TabIndex = 4;
            this.trackBar5.TickFrequency = 15;
            // 
            // trackBar4
            // 
            this.trackBar4.Location = new System.Drawing.Point(231, 19);
            this.trackBar4.Maximum = 255;
            this.trackBar4.Name = "trackBar4";
            this.trackBar4.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar4.Size = new System.Drawing.Size(45, 236);
            this.trackBar4.TabIndex = 3;
            this.trackBar4.TickFrequency = 15;
            // 
            // trackBar3
            // 
            this.trackBar3.Location = new System.Drawing.Point(156, 19);
            this.trackBar3.Maximum = 255;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar3.Size = new System.Drawing.Size(45, 236);
            this.trackBar3.TabIndex = 2;
            this.trackBar3.TickFrequency = 15;
            // 
            // trackBar2
            // 
            this.trackBar2.Location = new System.Drawing.Point(81, 19);
            this.trackBar2.Maximum = 255;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar2.Size = new System.Drawing.Size(45, 236);
            this.trackBar2.TabIndex = 1;
            this.trackBar2.TickFrequency = 15;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(6, 19);
            this.trackBar1.Maximum = 255;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar1.Size = new System.Drawing.Size(45, 236);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.TickFrequency = 15;
            // 
            // master_mode
            // 
            this.master_mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.master_mode.FormattingEnabled = true;
            this.master_mode.Location = new System.Drawing.Point(12, 513);
            this.master_mode.Name = "master_mode";
            this.master_mode.Size = new System.Drawing.Size(65, 21);
            this.master_mode.TabIndex = 12;
            // 
            // master_value
            // 
            this.master_value.Location = new System.Drawing.Point(18, 376);
            this.master_value.Maximum = 255;
            this.master_value.Name = "master_value";
            this.master_value.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.master_value.Size = new System.Drawing.Size(45, 131);
            this.master_value.TabIndex = 11;
            this.master_value.TickFrequency = 15;
            // 
            // outputBox
            // 
            this.outputBox.Location = new System.Drawing.Point(183, 385);
            this.outputBox.Multiline = true;
            this.outputBox.Name = "outputBox";
            this.outputBox.Size = new System.Drawing.Size(588, 148);
            this.outputBox.TabIndex = 13;
            this.outputBox.TextChanged += new System.EventHandler(this.outputBox_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.outputBox);
            this.Controls.Add(this.master_mode);
            this.Controls.Add(this.master_value);
            this.Controls.Add(this.gui_channelSlider);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Meister Lampe Control GUI";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.gui_channelSlider.ResumeLayout(false);
            this.gui_channelSlider.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.master_value)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tool_connect;
        private System.Windows.Forms.ToolStripStatusLabel status_connection;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menu_options;
        private System.Windows.Forms.ToolStripMenuItem menu_options_connection;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.GroupBox gui_channelSlider;
        private System.Windows.Forms.TrackBar trackBar6;
        private System.Windows.Forms.TrackBar trackBar7;
        private System.Windows.Forms.TrackBar trackBar8;
        private System.Windows.Forms.TrackBar trackBar9;
        private System.Windows.Forms.TrackBar trackBar10;
        private System.Windows.Forms.TrackBar trackBar5;
        private System.Windows.Forms.TrackBar trackBar4;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox9;
        private System.Windows.Forms.ComboBox comboBox10;
        private System.Windows.Forms.ComboBox comboBox5;
        private System.Windows.Forms.ComboBox comboBox6;
        private System.Windows.Forms.ComboBox comboBox7;
        private System.Windows.Forms.ComboBox comboBox8;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ComboBox master_mode;
        private System.Windows.Forms.TrackBar master_value;
        private System.Windows.Forms.TextBox outputBox;
    }
}

