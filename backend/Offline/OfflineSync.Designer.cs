namespace Quanto.Offline
{
    partial class OfflineSync
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OfflineSync));
            this.button4 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblBillStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.lblbillpending = new System.Windows.Forms.Label();
            this.lblsettlementpending = new System.Windows.Forms.Label();
            this.chkBilling = new System.Windows.Forms.CheckBox();
            this.chkSettlement = new System.Windows.Forms.CheckBox();
            this.chkMaster = new System.Windows.Forms.CheckBox();
            this.progressBar3 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.lblmasterstatus = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rdHour = new System.Windows.Forms.RadioButton();
            this.rdDay = new System.Windows.Forms.RadioButton();
            this.rdMinute = new System.Windows.Forms.RadioButton();
            this.nmEvery = new System.Windows.Forms.NumericUpDown();
            this.button3 = new System.Windows.Forms.Button();
            this.chkautobarcode = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nmEvery)).BeginInit();
            this.SuspendLayout();
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Red;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.ForeColor = System.Drawing.Color.Transparent;
            this.button4.Location = new System.Drawing.Point(341, 360);
            this.button4.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(88, 26);
            this.button4.TabIndex = 32;
            this.button4.Text = "Close";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Transparent;
            this.button1.Location = new System.Drawing.Point(243, 360);
            this.button1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 27);
            this.button1.TabIndex = 31;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 102);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(414, 23);
            this.progressBar1.TabIndex = 37;
            // 
            // lblBillStatus
            // 
            this.lblBillStatus.AutoSize = true;
            this.lblBillStatus.Location = new System.Drawing.Point(9, 128);
            this.lblBillStatus.Name = "lblBillStatus";
            this.lblBillStatus.Size = new System.Drawing.Size(93, 18);
            this.lblBillStatus.TabIndex = 39;
            this.lblBillStatus.Text = "Bill Status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 223);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 18);
            this.label3.TabIndex = 42;
            this.label3.Text = "Settlement Status";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(12, 197);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(414, 23);
            this.progressBar2.TabIndex = 40;
            // 
            // lblbillpending
            // 
            this.lblbillpending.Location = new System.Drawing.Point(277, 81);
            this.lblbillpending.Name = "lblbillpending";
            this.lblbillpending.Size = new System.Drawing.Size(149, 18);
            this.lblbillpending.TabIndex = 43;
            this.lblbillpending.Text = "Pending - 0";
            this.lblbillpending.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblbillpending.Click += new System.EventHandler(this.lblbillpending_Click);
            // 
            // lblsettlementpending
            // 
            this.lblsettlementpending.Location = new System.Drawing.Point(277, 172);
            this.lblsettlementpending.Name = "lblsettlementpending";
            this.lblsettlementpending.Size = new System.Drawing.Size(149, 18);
            this.lblsettlementpending.TabIndex = 44;
            this.lblsettlementpending.Text = "Pending - 0";
            this.lblsettlementpending.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkBilling
            // 
            this.chkBilling.AutoSize = true;
            this.chkBilling.Location = new System.Drawing.Point(12, 80);
            this.chkBilling.Name = "chkBilling";
            this.chkBilling.Size = new System.Drawing.Size(53, 22);
            this.chkBilling.TabIndex = 45;
            this.chkBilling.Text = "Bill";
            this.chkBilling.UseVisualStyleBackColor = true;
            // 
            // chkSettlement
            // 
            this.chkSettlement.AutoSize = true;
            this.chkSettlement.Location = new System.Drawing.Point(9, 172);
            this.chkSettlement.Name = "chkSettlement";
            this.chkSettlement.Size = new System.Drawing.Size(118, 22);
            this.chkSettlement.TabIndex = 46;
            this.chkSettlement.Text = "Settlement";
            this.chkSettlement.UseVisualStyleBackColor = true;
            // 
            // chkMaster
            // 
            this.chkMaster.AutoSize = true;
            this.chkMaster.Location = new System.Drawing.Point(12, 273);
            this.chkMaster.Name = "chkMaster";
            this.chkMaster.Size = new System.Drawing.Size(127, 22);
            this.chkMaster.TabIndex = 49;
            this.chkMaster.Text = "Master Data";
            this.chkMaster.UseVisualStyleBackColor = true;
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(9, 298);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(420, 23);
            this.progressBar3.TabIndex = 47;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 324);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 18);
            this.label1.TabIndex = 50;
            this.label1.Text = "Master Status";
            // 
            // lblmasterstatus
            // 
            this.lblmasterstatus.Location = new System.Drawing.Point(197, 277);
            this.lblmasterstatus.Name = "lblmasterstatus";
            this.lblmasterstatus.Size = new System.Drawing.Size(232, 18);
            this.lblmasterstatus.TabIndex = 51;
            this.lblmasterstatus.Text = "Last Sync - ";
            this.lblmasterstatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ForeColor = System.Drawing.Color.Transparent;
            this.button2.Location = new System.Drawing.Point(9, 359);
            this.button2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 27);
            this.button2.TabIndex = 52;
            this.button2.Text = "Clear";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(12, 12);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(111, 22);
            this.chkAutoStart.TabIndex = 53;
            this.chkAutoStart.Text = "Auto Start";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 18);
            this.label2.TabIndex = 54;
            this.label2.Text = "Every";
            // 
            // rdHour
            // 
            this.rdHour.AutoSize = true;
            this.rdHour.Checked = true;
            this.rdHour.Location = new System.Drawing.Point(287, 37);
            this.rdHour.Name = "rdHour";
            this.rdHour.Size = new System.Drawing.Size(64, 22);
            this.rdHour.TabIndex = 55;
            this.rdHour.TabStop = true;
            this.rdHour.Text = "Hour";
            this.rdHour.UseVisualStyleBackColor = true;
            // 
            // rdDay
            // 
            this.rdDay.AutoSize = true;
            this.rdDay.Location = new System.Drawing.Point(357, 37);
            this.rdDay.Name = "rdDay";
            this.rdDay.Size = new System.Drawing.Size(57, 22);
            this.rdDay.TabIndex = 56;
            this.rdDay.Text = "Day";
            this.rdDay.UseVisualStyleBackColor = true;
            // 
            // rdMinute
            // 
            this.rdMinute.AutoSize = true;
            this.rdMinute.Location = new System.Drawing.Point(200, 37);
            this.rdMinute.Name = "rdMinute";
            this.rdMinute.Size = new System.Drawing.Size(81, 22);
            this.rdMinute.TabIndex = 57;
            this.rdMinute.Text = "Minute";
            this.rdMinute.UseVisualStyleBackColor = true;
            // 
            // nmEvery
            // 
            this.nmEvery.Location = new System.Drawing.Point(88, 35);
            this.nmEvery.Name = "nmEvery";
            this.nmEvery.Size = new System.Drawing.Size(88, 27);
            this.nmEvery.TabIndex = 58;
            this.nmEvery.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.ForeColor = System.Drawing.Color.Transparent;
            this.button3.Location = new System.Drawing.Point(107, 360);
            this.button3.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(88, 27);
            this.button3.TabIndex = 59;
            this.button3.Text = "Details";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // chkautobarcode
            // 
            this.chkautobarcode.AutoSize = true;
            this.chkautobarcode.Location = new System.Drawing.Point(114, 80);
            this.chkautobarcode.Name = "chkautobarcode";
            this.chkautobarcode.Size = new System.Drawing.Size(136, 22);
            this.chkautobarcode.TabIndex = 60;
            this.chkautobarcode.Text = "Auto Barcode";
            this.chkautobarcode.UseVisualStyleBackColor = true;
            this.chkautobarcode.Visible = false;
            // 
            // OfflineSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(447, 400);
            this.Controls.Add(this.chkautobarcode);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.nmEvery);
            this.Controls.Add(this.rdMinute);
            this.Controls.Add(this.rdDay);
            this.Controls.Add(this.rdHour);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkAutoStart);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.lblmasterstatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkMaster);
            this.Controls.Add(this.progressBar3);
            this.Controls.Add(this.chkSettlement);
            this.Controls.Add(this.chkBilling);
            this.Controls.Add(this.lblsettlementpending);
            this.Controls.Add(this.lblbillpending);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.lblBillStatus);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OfflineSync";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Offline Synchronize";
            this.Load += new System.EventHandler(this.OfflineSync_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nmEvery)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblBillStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label lblbillpending;
        private System.Windows.Forms.Label lblsettlementpending;
        private System.Windows.Forms.CheckBox chkBilling;
        private System.Windows.Forms.CheckBox chkSettlement;
        private System.Windows.Forms.CheckBox chkMaster;
        private System.Windows.Forms.ProgressBar progressBar3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblmasterstatus;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rdHour;
        private System.Windows.Forms.RadioButton rdDay;
        private System.Windows.Forms.RadioButton rdMinute;
        private System.Windows.Forms.NumericUpDown nmEvery;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox chkautobarcode;
    }
}