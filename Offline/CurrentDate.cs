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
    public partial class CurrentDate : Form
    {
        public CurrentDate()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public DateTime Date {
            get
            {
                return monthCalendar1.SelectionEnd.Date;
            }
            set
            {
                monthCalendar1.SelectionStart = value;
                monthCalendar1.SelectionEnd = value;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }


}
