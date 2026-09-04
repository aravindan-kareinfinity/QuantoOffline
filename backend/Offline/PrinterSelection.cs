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
    public partial class PrinterSelection : Form
    {
        public PrinterSelection()
        {
            InitializeComponent();
        }
        public bool SkipPrinter { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            if(cmbCounter.SelectedItem == null)
            {
                MessageBox.Show("Please select the printer");
                return;
            }
            InfyPOS.Processors.BillManager.Instance.Data.Printer = cmbCounter.SelectedItem as Quanto.Printer.PrinterService.Printer;
            InfyPOS.Processors.BillManager.Instance.Data.noofcopy =Convert.ToInt32(noofcopy.Value);
            InfyPOS.Processors.BillManager.Instance.PersistMasters();
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            cmbCounter.DataSource = Quanto.Printer.PrinterService.Instance.ListOfPrinters();
            if (InfyPOS.Processors.BillManager.Instance.Data.noofcopy == 0)
                InfyPOS.Processors.BillManager.Instance.Data.noofcopy = 1;
            noofcopy.Value = InfyPOS.Processors.BillManager.Instance.Data.noofcopy;

            InfyPOS.Processors.BillManager.Instance.Data.printconfig.ForEach(x =>
            {
                if(InfyPOS.Processors.BillManager.Instance.Data.Company.Exists(xx => xx.id == x.companyid))
                    x.company = InfyPOS.Processors.BillManager.Instance.Data.Company.Find(xx => xx.id == x.companyid).name;
            });
            cmbFormtList.DataSource = InfyPOS.Processors.BillManager.Instance.Data.printconfig;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            SkipPrinter = true;
            this.DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (cmbFormtList.SelectedItem != null)
            {
                var printconfig = cmbFormtList.SelectedItem as InfyPOS.Processors.OfflineClient.PrintCofig;
                FormatSelection formatSelection = new FormatSelection(printconfig.format);
                if(formatSelection.ShowDialog() == DialogResult.OK)
                {
                    printconfig.format = formatSelection.format;
                    InfyPOS.Processors.BillManager.Instance.PersistMasters();
                }
            }
        }
    }


}
