namespace Quanto
{
    partial class ServiceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnstart = new System.Windows.Forms.Button();
            this.btnstop = new System.Windows.Forms.Button();
            this.btnuninstall = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(24, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Status";
            this.label1.DoubleClick += new System.EventHandler(this.label1_DoubleClick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Verdana", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblStatus.Location = new System.Drawing.Point(113, 23);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(105, 25);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Running";
            // 
            // btnstart
            // 
            this.btnstart.BackColor = System.Drawing.Color.Teal;
            this.btnstart.Font = new System.Drawing.Font("Verdana", 12F);
            this.btnstart.ForeColor = System.Drawing.Color.White;
            this.btnstart.Location = new System.Drawing.Point(19, 71);
            this.btnstart.Name = "btnstart";
            this.btnstart.Size = new System.Drawing.Size(75, 26);
            this.btnstart.TabIndex = 2;
            this.btnstart.Text = "Start";
            this.btnstart.UseVisualStyleBackColor = false;
            this.btnstart.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnstop
            // 
            this.btnstop.BackColor = System.Drawing.Color.Teal;
            this.btnstop.Font = new System.Drawing.Font("Verdana", 12F);
            this.btnstop.ForeColor = System.Drawing.Color.White;
            this.btnstop.Location = new System.Drawing.Point(114, 71);
            this.btnstop.Name = "btnstop";
            this.btnstop.Size = new System.Drawing.Size(75, 26);
            this.btnstop.TabIndex = 3;
            this.btnstop.Text = "Stop";
            this.btnstop.UseVisualStyleBackColor = false;
            this.btnstop.Click += new System.EventHandler(this.btnstop_Click);
            // 
            // btnuninstall
            // 
            this.btnuninstall.BackColor = System.Drawing.Color.Teal;
            this.btnuninstall.Font = new System.Drawing.Font("Verdana", 12F);
            this.btnuninstall.ForeColor = System.Drawing.Color.White;
            this.btnuninstall.Location = new System.Drawing.Point(207, 71);
            this.btnuninstall.Name = "btnuninstall";
            this.btnuninstall.Size = new System.Drawing.Size(107, 26);
            this.btnuninstall.TabIndex = 4;
            this.btnuninstall.Text = "Uninstall";
            this.btnuninstall.UseVisualStyleBackColor = false;
            this.btnuninstall.Click += new System.EventHandler(this.btnuninstall_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Teal;
            this.button1.Font = new System.Drawing.Font("Verdana", 12F);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(330, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 26);
            this.button1.TabIndex = 5;
            this.button1.Text = "Config";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // ServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(454, 129);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnuninstall);
            this.Controls.Add(this.btnstop);
            this.Controls.Add(this.btnstart);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ServiceForm";
            this.Text = "Printer Service";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnstart;
        private System.Windows.Forms.Button btnstop;
        private System.Windows.Forms.Button btnuninstall;
        private System.Windows.Forms.Button button1;
    }
}