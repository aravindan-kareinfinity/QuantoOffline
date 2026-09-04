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
    public partial class EmployeeSelection : Form
    {
        public EmployeeSelection()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (cmbCounter.SelectedItem == null)
            {
                MessageBox.Show("Please select the employee");
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        public long employeeid
        {
            get
            {
                return (cmbCounter.SelectedItem as InfyPOS.Processors.OfflineClient.Employee).id;
            }
        }
        public string reason
        {
            get
            {
                return txtCounterCode.Text;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            cmbCounter.DataSource = InfyPOS.Processors.BillManager.Instance.Data.Employee;
        }

    }


}
