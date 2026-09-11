namespace Quanto.Offline
{
    partial class OfflineSyncDetail
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OfflineSyncDetail));
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.AutoSize = true;
            this.panelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelButtons.Controls.Add(this.button1);
            this.panelButtons.Controls.Add(this.button2);
            this.panelButtons.Controls.Add(this.button4);
            this.panelButtons.Controls.Add(this.button5);
            this.panelButtons.Controls.Add(this.button3);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelButtons.Location = new System.Drawing.Point(12, 12);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.panelButtons.Size = new System.Drawing.Size(960, 49);
            this.panelButtons.TabIndex = 1;
            this.panelButtons.WrapContents = true;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.button1.MinimumSize = new System.Drawing.Size(120, 36);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(16, 4, 16, 4);
            this.button1.Size = new System.Drawing.Size(132, 36);
            this.button1.TabIndex = 16;
            this.button1.Text = "Load Bills";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.showbillrecords);
            // 
            // button2
            // 
            this.button2.AutoSize = true;
            this.button2.BackColor = System.Drawing.Color.Red;
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.button2.MinimumSize = new System.Drawing.Size(120, 36);
            this.button2.Name = "button2";
            this.button2.Padding = new System.Windows.Forms.Padding(16, 4, 16, 4);
            this.button2.Size = new System.Drawing.Size(176, 36);
            this.button2.TabIndex = 17;
            this.button2.Text = "Load Settlement";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.showsettlement);
            // 
            // button4
            // 
            this.button4.AutoSize = true;
            this.button4.BackColor = System.Drawing.Color.DarkGreen;
            this.button4.FlatAppearance.BorderSize = 0;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.ForeColor = System.Drawing.Color.White;
            this.button4.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.button4.MinimumSize = new System.Drawing.Size(120, 36);
            this.button4.Name = "button4";
            this.button4.Padding = new System.Windows.Forms.Padding(16, 4, 16, 4);
            this.button4.Size = new System.Drawing.Size(120, 36);
            this.button4.TabIndex = 19;
            this.button4.Text = "Stock";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // button5
            // 
            this.button5.AutoSize = true;
            this.button5.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.ForeColor = System.Drawing.Color.White;
            this.button5.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.button5.MinimumSize = new System.Drawing.Size(120, 36);
            this.button5.Name = "button5";
            this.button5.Padding = new System.Windows.Forms.Padding(16, 4, 16, 4);
            this.button5.Size = new System.Drawing.Size(132, 36);
            this.button5.TabIndex = 20;
            this.button5.Text = "Load OLD";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button3
            // 
            this.button3.AutoSize = true;
            this.button3.BackColor = System.Drawing.Color.Red;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.ForeColor = System.Drawing.Color.White;
            this.button3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.button3.MinimumSize = new System.Drawing.Size(120, 36);
            this.button3.Name = "button3";
            this.button3.Padding = new System.Windows.Forms.Padding(16, 4, 16, 4);
            this.button3.Size = new System.Drawing.Size(120, 36);
            this.button3.TabIndex = 18;
            this.button3.Text = "Delete";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(12, 61);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.Size = new System.Drawing.Size(960, 427);
            this.dataGridView1.TabIndex = 0;
            // 
            // OfflineSyncDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(984, 500);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panelButtons);
            this.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "OfflineSyncDetail";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Offline Synchronize";
            this.panelButtons.ResumeLayout(false);
            this.panelButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}
