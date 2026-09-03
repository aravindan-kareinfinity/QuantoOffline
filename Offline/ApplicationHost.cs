using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Quanto.Offline;
using System.Web;

namespace Quanto
{
    class OfflineApplicationContext : ApplicationContext
    {
        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private string ApplicationMode = "";
        public OfflineApplicationContext()
        {
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["AppMode"]))
            {
                ApplicationMode = System.Configuration.ConfigurationManager.AppSettings["AppMode"];
            }
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText = "Quanto Offline system";
            TrayIcon.BalloonTipTitle = "Quanto Offline system";
            TrayIcon.Text = "Quanto Offline system";


            //The icon is added to the project resources.
            //Here I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = Quanto.Properties.Resources.favicon;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            TrayIconContextMenu.ImageList = new ImageList();
            TrayIconContextMenu.ImageList.Images.Add("close", Quanto.Properties.Resources.close);
            TrayIconContextMenu.ImageList.Images.Add("pos", Quanto.Properties.Resources.pos);
            TrayIconContextMenu.ImageList.Images.Add("settings", Quanto.Properties.Resources.settings);
            TrayIconContextMenu.ImageList.Images.Add("start", Quanto.Properties.Resources.start);
            TrayIconContextMenu.ImageList.Images.Add("stop", Quanto.Properties.Resources.stop);
            TrayIconContextMenu.ImageList.Images.Add("sync", Quanto.Properties.Resources.sync);
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 

            if (string.IsNullOrEmpty(ApplicationMode))
            {
                this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
                CreateIcon("Load Data","start"),
                CreateIcon("Connect Server","start"),
                CreateIcon("Login","start"),
                CreateIcon("Bill","pos") ,
                CreateIcon("Settlement","pos") ,
                CreateIcon("Settings","settings") ,
                CreateIcon("Printer Service Start","start") ,
                CreateIcon("Printer Service Stop","stop") ,
                CreateIcon("Data Sync","sync"),
                CreateIcon("BI Sync","bisync"),
                CreateIcon("Log out","close"),
                CreateIcon("Exit","close")
                });
            }
            else if (ApplicationMode == "BI")
            {
                this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
                    CreateIcon("BI Sync","bisync"),
                    CreateIcon("Exit","close")
                });
            }
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);
            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        public ToolStripMenuItem CreateIcon(string display, string key)
        {
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(display);
            toolStripMenuItem.ImageKey = key;
            toolStripMenuItem.Size = new Size(152, 22);
            toolStripMenuItem.Click += ToolStripMenuItem_Click;
            return toolStripMenuItem;
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
            switch (toolStripMenuItem.Text)
            {
                case "Printer Service Start":
                    ServiceForm.GetService().Start();
                    break;
                case "Printer Service Stop":
                    ServiceForm.GetService().Stop();
                    break;
				case "Printer Service Settings":
                case "Settings":
                    new Configform().ShowDialog();
                    break;
                case "BI Sync":
                    new BISync().ShowDialog();
                    break;
                case "Data Sync":
                    new Offline.OfflineSync().ShowDialog();
                    break;
                case "Load Data":
                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        InfyPOS.Processors.BillManager.Instance.Load(ofd.FileName);
                        MessageBox.Show("Your offline system ready.");
                    }
                    break;
                case "Connect Server":
                    new Offline.MasterDownload().ShowDialog();
                    break;
                case "Bill":
                    if (InfyPOS.Processors.BillManager.Instance.CurrentUser == null)
                    {
                        Quanto.Offline.Login login = new Offline.Login();
                        if (login.ShowDialog() == DialogResult.OK)
                        {
                            new Quanto.Offline.Billing().ShowDialog();
                        }
                    }
                    else
                    {
                        new Quanto.Offline.Billing().ShowDialog();
                    }
                    break;
                case "Settlement":
                    if (InfyPOS.Processors.BillManager.Instance.CurrentUser == null)
                    {
                        Quanto.Offline.Login login = new Offline.Login();
                        if (login.ShowDialog() == DialogResult.OK)
                        {
                            new Quanto.Offline.Settlement().ShowDialog();
                        }
                    }
                    else
                    {
                        new Quanto.Offline.Settlement().ShowDialog();
                    }
                    break;
                case "Login":
                    {
                        Quanto.Offline.Login login = new Offline.Login();
                        login.ShowDialog();
                        break;
                    }
                case "Log out":
                    InfyPOS.Processors.BillManager.Instance.CurrentUser = null;
                    break;
                case "Exit":
                    Application.Exit();
                    break;
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            //Here you can do stuff if the tray icon is doubleclicked
            TrayIcon.ShowBalloonTip(10000);
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to close me?",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
