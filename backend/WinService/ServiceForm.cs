using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quanto
{
    public partial class ServiceForm : Form
    {
        public ServiceForm()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        ServiceController service;
        static string SERVICE_NAME = "Quanto-Client-Service";
        public static ServiceController GetService()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                if (service.ServiceName.Equals(SERVICE_NAME))
                {
                    return service;
                }
            }
            return null;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            btnuninstall.Enabled = false;
            ServiceController[] services = ServiceController.GetServices();
            btnstart.Enabled = false;
            btnstop.Enabled = false;

            foreach (ServiceController service in services)
            {
                if (service.ServiceName.Equals(SERVICE_NAME))
                {
                    lblStatus.Text = service.Status.ToString();
                    btnstart.Enabled = (service.Status != ServiceControllerStatus.Running);
                    btnstop.Enabled = (service.Status == ServiceControllerStatus.Running); ;
                    btnuninstall.Enabled = true;
                    this.service = service;
                    break;
                }
            }

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 10000;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            service.Refresh();



            Invoke((MethodInvoker)delegate
            {

                lblStatus.Text = service.Status.ToString();
                btnstart.Enabled = (service.Status != ServiceControllerStatus.Running);
                btnstop.Enabled = (service.Status == ServiceControllerStatus.Running); ;
                btnuninstall.Enabled = true;
            });




        }

        private void button1_Click(object sender, EventArgs e)
        {
            service.Refresh();
            if (service.Status == ServiceControllerStatus.Stopped)
                service.Start();
            //btnstop.Enabled = true;
            //btnstart.Enabled = false;
        }

        private void btnstop_Click(object sender, EventArgs e)
        {
            service.Refresh();
            if (service.Status == ServiceControllerStatus.Running)
                service.Stop();
            //btnstop.Enabled = false;
            //btnstart.Enabled = true;
        }

        private void btnuninstall_Click(object sender, EventArgs e)
        {
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                DialogResult dr = new DialogResult();
                dr = MessageBox.Show("Do you REALLY like to install the " + SERVICE_NAME + "?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    Quanto.SelfInstaller.InstallMe();
                    MessageBox.Show("Successfully installed the " + SERVICE_NAME, "Status",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                Arguments = fi.Directory.FullName,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Configform configform = new Configform();
            configform.ShowDialog();
        }
    }
}


