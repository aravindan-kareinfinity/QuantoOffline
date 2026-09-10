namespace Quanto.Offline
{
    partial class Billing
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Billing));
            this.txtBarcode = new System.Windows.Forms.TextBox();
            this.txtQty = new System.Windows.Forms.TextBox();
            this.txtRate = new System.Windows.Forms.TextBox();
            this.txtDiscount = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtfinalprice = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txttotaldiscountpercentage = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txttotaldiscount = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txttotalamount = new System.Windows.Forms.TextBox();
            this.lblAvailable = new System.Windows.Forms.Label();
            this.lblLastBillNo = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.newBillToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPrinterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pROMOTIONDISABLEDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button4 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.ProgressPanel = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label13 = new System.Windows.Forms.Label();
            this.billScroller = new System.Windows.Forms.HScrollBar();
            this.txtotalqty = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbTax = new Quanto.Controls.ComboBox();
            this.cmbProduts = new Quanto.Controls.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txttotalitemdiscount = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.ProgressPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBarcode
            // 
            this.txtBarcode.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBarcode.Location = new System.Drawing.Point(32, 79);
            this.txtBarcode.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(216, 33);
            this.txtBarcode.TabIndex = 1;
            // 
            // txtQty
            // 
            this.txtQty.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtQty.Location = new System.Drawing.Point(621, 79);
            this.txtQty.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(76, 33);
            this.txtQty.TabIndex = 3;
            this.txtQty.TextChanged += new System.EventHandler(this.txtQty_TextChanged);
            // 
            // txtRate
            // 
            this.txtRate.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRate.Location = new System.Drawing.Point(707, 79);
            this.txtRate.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtRate.Name = "txtRate";
            this.txtRate.Size = new System.Drawing.Size(99, 33);
            this.txtRate.TabIndex = 4;
            this.txtRate.TextChanged += new System.EventHandler(this.txtQty_TextChanged);
            // 
            // txtDiscount
            // 
            this.txtDiscount.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiscount.Location = new System.Drawing.Point(816, 79);
            this.txtDiscount.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtDiscount.Name = "txtDiscount";
            this.txtDiscount.Size = new System.Drawing.Size(99, 33);
            this.txtDiscount.TabIndex = 5;
            this.txtDiscount.TextChanged += new System.EventHandler(this.txtQty_TextChanged);
            this.txtDiscount.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtDiscount_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.OrangeRed;
            this.label1.Location = new System.Drawing.Point(29, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 18);
            this.label1.TabIndex = 7;
            this.label1.Text = "Barcode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.OrangeRed;
            this.label2.Location = new System.Drawing.Point(255, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 18);
            this.label2.TabIndex = 8;
            this.label2.Text = "Product";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.OrangeRed;
            this.label3.Location = new System.Drawing.Point(618, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "Qty";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.OrangeRed;
            this.label4.Location = new System.Drawing.Point(704, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 18);
            this.label4.TabIndex = 10;
            this.label4.Text = "Rate";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.OrangeRed;
            this.label5.Location = new System.Drawing.Point(813, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 18);
            this.label5.TabIndex = 11;
            this.label5.Text = "Discount";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.OrangeRed;
            this.label6.Location = new System.Drawing.Point(924, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 18);
            this.label6.TabIndex = 13;
            this.label6.Text = "Final Price";
            // 
            // txtfinalprice
            // 
            this.txtfinalprice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtfinalprice.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtfinalprice.Location = new System.Drawing.Point(925, 79);
            this.txtfinalprice.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtfinalprice.Name = "txtfinalprice";
            this.txtfinalprice.ReadOnly = true;
            this.txtfinalprice.Size = new System.Drawing.Size(99, 33);
            this.txtfinalprice.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.OrangeRed;
            this.label7.Location = new System.Drawing.Point(465, 57);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 18);
            this.label7.TabIndex = 14;
            this.label7.Text = "Tax";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Transparent;
            this.button1.Location = new System.Drawing.Point(1032, 79);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(53, 33);
            this.button1.TabIndex = 15;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.OrangeRed;
            this.label8.Location = new System.Drawing.Point(914, 518);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 18);
            this.label8.TabIndex = 17;
            this.label8.Text = "Total";
            // 
            // txttotaldiscountpercentage
            // 
            this.txttotaldiscountpercentage.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txttotaldiscountpercentage.Location = new System.Drawing.Point(705, 545);
            this.txttotaldiscountpercentage.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txttotaldiscountpercentage.Name = "txttotaldiscountpercentage";
            this.txttotaldiscountpercentage.Size = new System.Drawing.Size(105, 33);
            this.txttotaldiscountpercentage.TabIndex = 16;
            this.txttotaldiscountpercentage.TextChanged += new System.EventHandler(this.txtdiscountpercentage_TextChanged);
            this.txttotaldiscountpercentage.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txttotaldiscountpercentage_KeyUp);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.OrangeRed;
            this.label9.Location = new System.Drawing.Point(817, 520);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 18);
            this.label9.TabIndex = 19;
            this.label9.Text = "A.Discnt";
            // 
            // txttotaldiscount
            // 
            this.txttotaldiscount.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txttotaldiscount.Location = new System.Drawing.Point(820, 545);
            this.txttotaldiscount.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txttotaldiscount.Name = "txttotaldiscount";
            this.txttotaldiscount.Size = new System.Drawing.Size(76, 33);
            this.txttotaldiscount.TabIndex = 18;
            this.txttotaldiscount.TextChanged += new System.EventHandler(this.txtgrossdiscount_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.OrangeRed;
            this.label10.Location = new System.Drawing.Point(702, 520);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(104, 18);
            this.label10.TabIndex = 21;
            this.label10.Text = "A.Discnt %";
            // 
            // txttotalamount
            // 
            this.txttotalamount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txttotalamount.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txttotalamount.Location = new System.Drawing.Point(916, 543);
            this.txttotalamount.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txttotalamount.Name = "txttotalamount";
            this.txttotalamount.ReadOnly = true;
            this.txttotalamount.Size = new System.Drawing.Size(108, 33);
            this.txttotalamount.TabIndex = 20;
            // 
            // lblAvailable
            // 
            this.lblAvailable.AutoSize = true;
            this.lblAvailable.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailable.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblAvailable.Location = new System.Drawing.Point(29, 518);
            this.lblAvailable.Name = "lblAvailable";
            this.lblAvailable.Size = new System.Drawing.Size(124, 18);
            this.lblAvailable.TabIndex = 24;
            this.lblAvailable.Text = "Available Bills";
            // 
            // lblLastBillNo
            // 
            this.lblLastBillNo.AutoSize = true;
            this.lblLastBillNo.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLastBillNo.ForeColor = System.Drawing.Color.DimGray;
            this.lblLastBillNo.Location = new System.Drawing.Point(380, 518);
            this.lblLastBillNo.Name = "lblLastBillNo";
            this.lblLastBillNo.Size = new System.Drawing.Size(110, 18);
            this.lblLastBillNo.TabIndex = 26;
            this.lblLastBillNo.Text = "Last Bill No";
            this.lblLastBillNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newBillToolStripMenuItem,
            this.configurationToolStripMenuItem,
            this.reportToolStripMenuItem,
            this.dateToolStripMenuItem,
            this.toolStripMenuItem1,
            this.selectPrinterToolStripMenuItem,
            this.pROMOTIONDISABLEDToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1184, 26);
            this.menuStrip1.TabIndex = 28;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // newBillToolStripMenuItem
            // 
            this.newBillToolStripMenuItem.Name = "newBillToolStripMenuItem";
            this.newBillToolStripMenuItem.Size = new System.Drawing.Size(87, 22);
            this.newBillToolStripMenuItem.Text = "New Bill";
            this.newBillToolStripMenuItem.Click += new System.EventHandler(this.newBillToolStripMenuItem_Click);
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.configurationToolStripMenuItem.Text = "Configuration";
            this.configurationToolStripMenuItem.Click += new System.EventHandler(this.configurationToolStripMenuItem_Click);
            // 
            // reportToolStripMenuItem
            // 
            this.reportToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.reportToolStripMenuItem.Name = "reportToolStripMenuItem";
            this.reportToolStripMenuItem.Size = new System.Drawing.Size(74, 22);
            this.reportToolStripMenuItem.Text = "Report";
            this.reportToolStripMenuItem.Click += new System.EventHandler(this.reportToolStripMenuItem_Click);
            // 
            // dateToolStripMenuItem
            // 
            this.dateToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.dateToolStripMenuItem.Name = "dateToolStripMenuItem";
            this.dateToolStripMenuItem.Size = new System.Drawing.Size(59, 22);
            this.dateToolStripMenuItem.Text = "Date";
            this.dateToolStripMenuItem.Click += new System.EventHandler(this.dateToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(12, 22);
            // 
            // selectPrinterToolStripMenuItem
            // 
            this.selectPrinterToolStripMenuItem.Name = "selectPrinterToolStripMenuItem";
            this.selectPrinterToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.selectPrinterToolStripMenuItem.Text = "Select Printer";
            this.selectPrinterToolStripMenuItem.Click += new System.EventHandler(this.selectPrinterToolStripMenuItem_Click);
            // 
            // pROMOTIONDISABLEDToolStripMenuItem
            // 
            this.pROMOTIONDISABLEDToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.pROMOTIONDISABLEDToolStripMenuItem.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pROMOTIONDISABLEDToolStripMenuItem.ForeColor = System.Drawing.Color.Crimson;
            this.pROMOTIONDISABLEDToolStripMenuItem.Name = "pROMOTIONDISABLEDToolStripMenuItem";
            this.pROMOTIONDISABLEDToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.pROMOTIONDISABLEDToolStripMenuItem.Text = "PROMOTION - DISABLED";
            this.pROMOTIONDISABLEDToolStripMenuItem.Click += new System.EventHandler(this.pROMOTIONDISABLEDToolStripMenuItem_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader7,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(32, 119);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1121, 396);
            this.listView1.TabIndex = 29;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Barcode";
            this.columnHeader1.Width = 213;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Product";
            this.columnHeader2.Width = 375;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Qty";
            this.columnHeader7.Width = 78;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Gross";
            this.columnHeader3.Width = 109;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Discount";
            this.columnHeader4.Width = 117;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Price";
            this.columnHeader5.Width = 98;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Amount";
            this.columnHeader6.Width = 100;
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Red;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.ForeColor = System.Drawing.Color.Transparent;
            this.button4.Location = new System.Drawing.Point(1100, 79);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(53, 33);
            this.button4.TabIndex = 30;
            this.button4.Text = "Del";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.DarkCyan;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ForeColor = System.Drawing.Color.Transparent;
            this.button2.Location = new System.Drawing.Point(1032, 543);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(121, 33);
            this.button2.TabIndex = 31;
            this.button2.Text = "Save";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Goldenrod;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.ForeColor = System.Drawing.Color.Transparent;
            this.button3.Location = new System.Drawing.Point(378, 543);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(99, 33);
            this.button3.TabIndex = 32;
            this.button3.Text = "Print";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ProgressPanel
            // 
            this.ProgressPanel.Controls.Add(this.progressBar1);
            this.ProgressPanel.Controls.Add(this.label13);
            this.ProgressPanel.Location = new System.Drawing.Point(366, 216);
            this.ProgressPanel.Name = "ProgressPanel";
            this.ProgressPanel.Size = new System.Drawing.Size(514, 144);
            this.ProgressPanel.TabIndex = 33;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 121);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(514, 23);
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
            // billScroller
            // 
            this.billScroller.LargeChange = 1;
            this.billScroller.Location = new System.Drawing.Point(32, 543);
            this.billScroller.Maximum = 1000;
            this.billScroller.Minimum = 1;
            this.billScroller.Name = "billScroller";
            this.billScroller.Size = new System.Drawing.Size(343, 33);
            this.billScroller.TabIndex = 23;
            this.billScroller.Value = 1000;
            this.billScroller.Scroll += new System.Windows.Forms.ScrollEventHandler(this.billScroller_Scroll);
            // 
            // txtotalqty
            // 
            this.txtotalqty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtotalqty.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtotalqty.Location = new System.Drawing.Point(485, 544);
            this.txtotalqty.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtotalqty.Name = "txtotalqty";
            this.txtotalqty.ReadOnly = true;
            this.txtotalqty.Size = new System.Drawing.Size(80, 33);
            this.txtotalqty.TabIndex = 35;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.OrangeRed;
            this.label11.Location = new System.Drawing.Point(482, 520);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(39, 18);
            this.label11.TabIndex = 36;
            this.label11.Text = "Qty";
            // 
            // cmbTax
            // 
            this.cmbTax.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbTax.EnterKeyAsTab = true;
            this.cmbTax.EntryScreen = null;
            this.cmbTax.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbTax.FormattingEnabled = true;
            this.cmbTax.InEditMode = false;
            this.cmbTax.ItemHeight = 28;
            this.cmbTax.LimitToList = true;
            this.cmbTax.Location = new System.Drawing.Point(467, 78);
            this.cmbTax.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.cmbTax.MaxDropDownItems = 10;
            this.cmbTax.Name = "cmbTax";
            this.cmbTax.ReadOnly = false;
            this.cmbTax.Size = new System.Drawing.Size(144, 34);
            this.cmbTax.TabIndex = 34;
            this.cmbTax.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbProduts_DrawItem);
            // 
            // cmbProduts
            // 
            this.cmbProduts.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbProduts.EnterKeyAsTab = true;
            this.cmbProduts.EntryScreen = null;
            this.cmbProduts.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbProduts.FormattingEnabled = true;
            this.cmbProduts.InEditMode = false;
            this.cmbProduts.ItemHeight = 28;
            this.cmbProduts.LimitToList = true;
            this.cmbProduts.Location = new System.Drawing.Point(258, 79);
            this.cmbProduts.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.cmbProduts.MaxDropDownItems = 10;
            this.cmbProduts.Name = "cmbProduts";
            this.cmbProduts.ReadOnly = false;
            this.cmbProduts.Size = new System.Drawing.Size(199, 34);
            this.cmbProduts.TabIndex = 2;
            this.cmbProduts.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbProduts_DrawItem);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.OrangeRed;
            this.label12.Location = new System.Drawing.Point(572, 520);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(83, 18);
            this.label12.TabIndex = 38;
            this.label12.Text = "Discount";
            // 
            // txttotalitemdiscount
            // 
            this.txttotalitemdiscount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txttotalitemdiscount.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txttotalitemdiscount.Location = new System.Drawing.Point(575, 544);
            this.txttotalitemdiscount.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txttotalitemdiscount.Name = "txttotalitemdiscount";
            this.txttotalitemdiscount.ReadOnly = true;
            this.txttotalitemdiscount.Size = new System.Drawing.Size(120, 33);
            this.txttotalitemdiscount.TabIndex = 37;
            // 
            // Billing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(1184, 595);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txttotalitemdiscount);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtotalqty);
            this.Controls.Add(this.cmbTax);
            this.Controls.Add(this.ProgressPanel);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.lblLastBillNo);
            this.Controls.Add(this.lblAvailable);
            this.Controls.Add(this.billScroller);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txttotalamount);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txttotaldiscount);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txttotaldiscountpercentage);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtfinalprice);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDiscount);
            this.Controls.Add(this.txtRate);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.cmbProduts);
            this.Controls.Add(this.txtBarcode);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Billing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quanto Offline Billing";
            this.Load += new System.EventHandler(this.OfflineBilling_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ProgressPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtBarcode;
        private Quanto.Controls.ComboBox cmbProduts;
        private System.Windows.Forms.TextBox txtQty;
        private System.Windows.Forms.TextBox txtRate;
        private System.Windows.Forms.TextBox txtDiscount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtfinalprice;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txttotaldiscountpercentage;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txttotaldiscount;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txttotalamount;
        private System.Windows.Forms.Label lblAvailable;
        private System.Windows.Forms.Label lblLastBillNo;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Panel ProgressPanel;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ToolStripMenuItem dateToolStripMenuItem;
        private Controls.ComboBox cmbTax;
        private System.Windows.Forms.ToolStripMenuItem newBillToolStripMenuItem;
        private System.Windows.Forms.HScrollBar billScroller;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem selectPrinterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pROMOTIONDISABLEDToolStripMenuItem;
        private System.Windows.Forms.TextBox txtotalqty;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txttotalitemdiscount;
    }
}