namespace Quanto.Offline
{
    partial class Settlement
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settlement));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtBillNo = new System.Windows.Forms.TextBox();
            this.lblAvailable = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCash = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCard = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.txtDiscount = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stcash = new System.Windows.Forms.ToolStripStatusLabel();
            this.stcard = new System.Windows.Forms.ToolStripStatusLabel();
            this.stdiscount = new System.Windows.Forms.ToolStripStatusLabel();
            this.sttotal = new System.Windows.Forms.ToolStripStatusLabel();
            this.stbills = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.settlementDate = new System.Windows.Forms.DateTimePicker();
            this.lblCounter = new System.Windows.Forms.Label();
            this.lblCollectionby = new System.Windows.Forms.Label();
            this.lblBillValue = new System.Windows.Forms.Label();
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblrefund = new System.Windows.Forms.Label();
            this.lblrefundcaption = new System.Windows.Forms.Label();
            this.txtAdjustment = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtCredit = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.ProgressPanel = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label13 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.ProgressPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 54);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(403, 387);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Bill No";
            this.columnHeader1.Width = 243;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Amount";
            this.columnHeader2.Width = 150;
            // 
            // txtBillNo
            // 
            this.txtBillNo.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBillNo.Location = new System.Drawing.Point(12, 14);
            this.txtBillNo.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtBillNo.Name = "txtBillNo";
            this.txtBillNo.Size = new System.Drawing.Size(355, 33);
            this.txtBillNo.TabIndex = 0;
            this.txtBillNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBillNo_KeyPress);
            this.txtBillNo.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtBillNo_KeyUp);
            // 
            // lblAvailable
            // 
            this.lblAvailable.AutoSize = true;
            this.lblAvailable.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailable.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblAvailable.Location = new System.Drawing.Point(454, 54);
            this.lblAvailable.Name = "lblAvailable";
            this.lblAvailable.Size = new System.Drawing.Size(79, 18);
            this.lblAvailable.TabIndex = 32;
            this.lblAvailable.Text = "Counter";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.OrangeRed;
            this.label1.Location = new System.Drawing.Point(454, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 18);
            this.label1.TabIndex = 33;
            this.label1.Text = "Collection By";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.OrangeRed;
            this.label2.Location = new System.Drawing.Point(454, 185);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 18);
            this.label2.TabIndex = 34;
            this.label2.Text = "Balance";
            // 
            // txtCash
            // 
            this.txtCash.Location = new System.Drawing.Point(648, 233);
            this.txtCash.Name = "txtCash";
            this.txtCash.Size = new System.Drawing.Size(165, 27);
            this.txtCash.TabIndex = 4;
            this.txtCash.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(454, 236);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 18);
            this.label3.TabIndex = 39;
            this.label3.Text = "Cash";
            // 
            // txtCard
            // 
            this.txtCard.Location = new System.Drawing.Point(648, 266);
            this.txtCard.Name = "txtCard";
            this.txtCard.Size = new System.Drawing.Size(165, 27);
            this.txtCard.TabIndex = 5;
            this.txtCard.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            this.txtCard.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCard_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(454, 269);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 18);
            this.label4.TabIndex = 41;
            this.label4.Text = "Card";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.OrangeRed;
            this.label5.Location = new System.Drawing.Point(454, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 18);
            this.label5.TabIndex = 43;
            this.label5.Text = "Bill Value";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Transparent;
            this.button1.Location = new System.Drawing.Point(725, 414);
            this.button1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 27);
            this.button1.TabIndex = 9;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Red;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.ForeColor = System.Drawing.Color.Transparent;
            this.button4.Location = new System.Drawing.Point(627, 414);
            this.button4.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(88, 26);
            this.button4.TabIndex = 10;
            this.button4.Text = "Clear";
            this.button4.UseVisualStyleBackColor = false;
            // 
            // txtDiscount
            // 
            this.txtDiscount.Location = new System.Drawing.Point(648, 299);
            this.txtDiscount.Name = "txtDiscount";
            this.txtDiscount.Size = new System.Drawing.Size(165, 27);
            this.txtDiscount.TabIndex = 6;
            this.txtDiscount.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            this.txtDiscount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDiscount_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(454, 302);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 18);
            this.label6.TabIndex = 46;
            this.label6.Text = "Discount";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.OrangeRed;
            this.label7.Location = new System.Drawing.Point(454, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 18);
            this.label7.TabIndex = 48;
            this.label7.Text = "Date";
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.DarkSlateGray;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stcash,
            this.stcard,
            this.stdiscount,
            this.sttotal,
            this.stbills,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 463);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(853, 23);
            this.statusStrip1.TabIndex = 49;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stcash
            // 
            this.stcash.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stcash.ForeColor = System.Drawing.Color.White;
            this.stcash.Name = "stcash";
            this.stcash.Size = new System.Drawing.Size(156, 18);
            this.stcash.Text = "Cash Received : 0";
            // 
            // stcard
            // 
            this.stcard.Font = new System.Drawing.Font("Verdana", 12F);
            this.stcard.ForeColor = System.Drawing.Color.White;
            this.stcard.Name = "stcard";
            this.stcard.Size = new System.Drawing.Size(153, 18);
            this.stcard.Text = "Card Received : 0";
            // 
            // stdiscount
            // 
            this.stdiscount.Font = new System.Drawing.Font("Verdana", 12F);
            this.stdiscount.ForeColor = System.Drawing.Color.White;
            this.stdiscount.Name = "stdiscount";
            this.stdiscount.Size = new System.Drawing.Size(108, 18);
            this.stdiscount.Text = "Discount : 0";
            // 
            // sttotal
            // 
            this.sttotal.BackColor = System.Drawing.Color.DarkSlateGray;
            this.sttotal.Font = new System.Drawing.Font("Verdana", 12F);
            this.sttotal.ForeColor = System.Drawing.Color.White;
            this.sttotal.Name = "sttotal";
            this.sttotal.Size = new System.Drawing.Size(164, 18);
            this.sttotal.Text = "Total Collection : 0";
            // 
            // stbills
            // 
            this.stbills.Font = new System.Drawing.Font("Verdana", 12F);
            this.stbills.ForeColor = System.Drawing.Color.White;
            this.stbills.Name = "stbills";
            this.stbills.Size = new System.Drawing.Size(72, 18);
            this.stbills.Text = "Bills : 0";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Verdana", 12F);
            this.toolStripStatusLabel1.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel1.IsLink = true;
            this.toolStripStatusLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.toolStripStatusLabel1.LinkColor = System.Drawing.Color.White;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(62, 18);
            this.toolStripStatusLabel1.Text = "Report";
            this.toolStripStatusLabel1.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripStatusLabel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Verdana", 12F);
            this.toolStripStatusLabel2.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel2.IsLink = true;
            this.toolStripStatusLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.toolStripStatusLabel2.LinkColor = System.Drawing.Color.White;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(83, 18);
            this.toolStripStatusLabel2.Text = "Summary";
            this.toolStripStatusLabel2.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
            // 
            // settlementDate
            // 
            this.settlementDate.CustomFormat = "dd/MM/yyyy";
            this.settlementDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.settlementDate.Location = new System.Drawing.Point(646, 19);
            this.settlementDate.Name = "settlementDate";
            this.settlementDate.Size = new System.Drawing.Size(167, 27);
            this.settlementDate.TabIndex = 3;
            this.settlementDate.ValueChanged += new System.EventHandler(this.settlementDate_ValueChanged);
            // 
            // lblCounter
            // 
            this.lblCounter.AutoSize = true;
            this.lblCounter.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCounter.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblCounter.Location = new System.Drawing.Point(645, 54);
            this.lblCounter.Name = "lblCounter";
            this.lblCounter.Size = new System.Drawing.Size(61, 18);
            this.lblCounter.TabIndex = 51;
            this.lblCounter.Text = "Select";
            this.lblCounter.DoubleClick += new System.EventHandler(this.lblCounter_DoubleClick);
            // 
            // lblCollectionby
            // 
            this.lblCollectionby.AutoSize = true;
            this.lblCollectionby.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCollectionby.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblCollectionby.Location = new System.Drawing.Point(643, 84);
            this.lblCollectionby.Name = "lblCollectionby";
            this.lblCollectionby.Size = new System.Drawing.Size(61, 18);
            this.lblCollectionby.TabIndex = 52;
            this.lblCollectionby.Text = "Select";
            this.lblCollectionby.DoubleClick += new System.EventHandler(this.lblCounter_DoubleClick);
            // 
            // lblBillValue
            // 
            this.lblBillValue.BackColor = System.Drawing.Color.DarkMagenta;
            this.lblBillValue.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBillValue.ForeColor = System.Drawing.Color.White;
            this.lblBillValue.Location = new System.Drawing.Point(645, 142);
            this.lblBillValue.Name = "lblBillValue";
            this.lblBillValue.Size = new System.Drawing.Size(168, 28);
            this.lblBillValue.TabIndex = 53;
            this.lblBillValue.Text = "0";
            this.lblBillValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBalance
            // 
            this.lblBalance.BackColor = System.Drawing.Color.DarkMagenta;
            this.lblBalance.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalance.ForeColor = System.Drawing.Color.White;
            this.lblBalance.Location = new System.Drawing.Point(645, 185);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(168, 28);
            this.lblBalance.TabIndex = 54;
            this.lblBalance.Text = "0";
            this.lblBalance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblrefund
            // 
            this.lblrefund.BackColor = System.Drawing.Color.Red;
            this.lblrefund.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblrefund.ForeColor = System.Drawing.Color.White;
            this.lblrefund.Location = new System.Drawing.Point(645, 185);
            this.lblrefund.Name = "lblrefund";
            this.lblrefund.Size = new System.Drawing.Size(168, 28);
            this.lblrefund.TabIndex = 56;
            this.lblrefund.Text = "0";
            this.lblrefund.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblrefund.Visible = false;
            // 
            // lblrefundcaption
            // 
            this.lblrefundcaption.AutoSize = true;
            this.lblrefundcaption.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblrefundcaption.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblrefundcaption.Location = new System.Drawing.Point(454, 185);
            this.lblrefundcaption.Name = "lblrefundcaption";
            this.lblrefundcaption.Size = new System.Drawing.Size(72, 18);
            this.lblrefundcaption.TabIndex = 55;
            this.lblrefundcaption.Text = "Refund";
            this.lblrefundcaption.Visible = false;
            // 
            // txtAdjustment
            // 
            this.txtAdjustment.Location = new System.Drawing.Point(649, 332);
            this.txtAdjustment.Name = "txtAdjustment";
            this.txtAdjustment.Size = new System.Drawing.Size(165, 27);
            this.txtAdjustment.TabIndex = 7;
            this.txtAdjustment.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            this.txtAdjustment.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAdjustment_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(455, 335);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 18);
            this.label8.TabIndex = 57;
            this.label8.Text = "Adjustment";
            // 
            // txtCredit
            // 
            this.txtCredit.Location = new System.Drawing.Point(649, 365);
            this.txtCredit.Name = "txtCredit";
            this.txtCredit.Size = new System.Drawing.Size(165, 27);
            this.txtCredit.TabIndex = 8;
            this.txtCredit.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            this.txtCredit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCredit_KeyPress);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(455, 368);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 18);
            this.label9.TabIndex = 59;
            this.label9.Text = "Credit";
            // 
            // ProgressPanel
            // 
            this.ProgressPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ProgressPanel.Controls.Add(this.progressBar1);
            this.ProgressPanel.Controls.Add(this.label13);
            this.ProgressPanel.Location = new System.Drawing.Point(169, 171);
            this.ProgressPanel.Name = "ProgressPanel";
            this.ProgressPanel.Size = new System.Drawing.Size(514, 144);
            this.ProgressPanel.TabIndex = 61;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 119);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(512, 23);
            this.progressBar1.TabIndex = 10;
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.OrangeRed;
            this.label13.Location = new System.Drawing.Point(3, 41);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(511, 28);
            this.label13.TabIndex = 9;
            this.label13.Text = "Loading data.....";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Red;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ForeColor = System.Drawing.Color.Transparent;
            this.button2.Location = new System.Drawing.Point(367, 14);
            this.button2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(48, 33);
            this.button2.TabIndex = 1;
            this.button2.Text = "Del";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Settlement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(853, 486);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.ProgressPanel);
            this.Controls.Add(this.txtCredit);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtAdjustment);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblrefund);
            this.Controls.Add(this.lblrefundcaption);
            this.Controls.Add(this.lblBalance);
            this.Controls.Add(this.lblBillValue);
            this.Controls.Add(this.lblCollectionby);
            this.Controls.Add(this.lblCounter);
            this.Controls.Add(this.settlementDate);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtDiscount);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtCard);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtCash);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblAvailable);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.txtBillNo);
            this.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settlement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settlement";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ProgressPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox txtBillNo;
        private System.Windows.Forms.Label lblAvailable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCash;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCard;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox txtDiscount;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel stcash;
        private System.Windows.Forms.ToolStripStatusLabel stcard;
        private System.Windows.Forms.ToolStripStatusLabel stdiscount;
        private System.Windows.Forms.ToolStripStatusLabel sttotal;
        private System.Windows.Forms.DateTimePicker settlementDate;
        private System.Windows.Forms.Label lblCounter;
        private System.Windows.Forms.Label lblCollectionby;
        private System.Windows.Forms.Label lblBillValue;
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.ToolStripStatusLabel stbills;
        private System.Windows.Forms.Label lblrefund;
        private System.Windows.Forms.Label lblrefundcaption;
        private System.Windows.Forms.TextBox txtAdjustment;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCredit;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel ProgressPanel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Button button2;
    }
}