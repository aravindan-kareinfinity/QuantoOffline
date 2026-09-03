using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quanto.Offline
{
    public partial class FormatSelection : Form
    {
        public string format { get; set; }
        public FormatSelection(string format)
        {
            this.format = format;
            InitializeComponent();
        }
        public bool SkipPrinter { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            format = richTextBox1.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = format;
        }

    }


}
