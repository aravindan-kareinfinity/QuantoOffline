using Quanto.Offline;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quanto
{
    public class Program
    {
        private static readonly ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve    ;
            bool _IsInstalled = false;
            bool serviceStarting = false;
            string SERVICE_NAME = "Quanto-Client-Service";
            if (args != null && args.Length == 1 && args[0] == "CONFIG" && !System.Diagnostics.Debugger.IsAttached)
            {
                ServiceForm form1 = new ServiceForm();
                form1.ShowDialog();
                return;
            }
            else if (args.Length == 1 && args[0] == "BI")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new BISync());
                return;
            }
            else if (args.Length == 1 && args[0] == "BISync")
            {
                Quanto.BI.BIDownloader bIDownloader = new BI.BIDownloader();
                bIDownloader.Process();
                return;
            }
            else if (args.Length == 1 && args[0] == "BITimer")
            {
                Quanto.BI.BIDownloader bIDownloader = new BI.BIDownloader();
                bIDownloader.Start(null);
                waitHandle.Wait(); // Blocks here indefinitely until Set() is called
                Console.WriteLine("Application exiting...");
                return;
            }
            else if (args.Length == 1 && args[0] == "offline")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new OfflineApplicationContext());
                return;
            }
            else if ((args.Length == 1 && args[0] == "client") || System.Diagnostics.Debugger.IsAttached)
            {
                Console.Write("Starting");
                try
                {
                   PrinterServiceHost hc = new PrinterServiceHost();
                   hc.Start();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new OfflineApplicationContext());
                    System.Console.ReadLine();
                }
                catch (Exception exp)
                {
                    Console.Write(exp);
                }
                return;
            }
            else
            {
                if (!IsRunAsAdministrator())
                {
                    var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);
                    // The following properties run the new process as administrator
                    processInfo.UseShellExecute = true;
                    processInfo.Verb = "runas";
                    // Start the new process
                    try
                    {
                        Process.Start(processInfo);
                        return;
                    }
                    catch (Exception)
                    {
                        // The user did not allow the application to run as administrator
                        MessageBox.Show("Sorry, this application must be run as Administrator.");
                    }

                }
            }


            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController service in services)
            {
                if (service.ServiceName.Equals(SERVICE_NAME))
                {
                    _IsInstalled = true;
                    if (service.Status == ServiceControllerStatus.StartPending)
                    {
                        serviceStarting = true;
                    }
                    break;
                }
            }

            if (!serviceStarting)
            {
                if (_IsInstalled == true)
                {
                    /*ServiceForm form1 = new ServiceForm();
                    form1.ShowDialog();*/
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new OfflineApplicationContext());
                }
                else
                {
                    DialogResult dr = new DialogResult();
                    dr = MessageBox.Show("Do you REALLY like to install the " + SERVICE_NAME + "?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.Yes)
                    {

                        Configform configform = new Configform();
                        if (configform.ShowDialog() == DialogResult.OK)
                        {
                            SelfInstaller.InstallMe();
                            MessageBox.Show("Successfully installed the " + SERVICE_NAME, "Status",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            else
            {
                // Started from the SCM
                System.ServiceProcess.ServiceBase[] servicestorun;
                servicestorun = new System.ServiceProcess.ServiceBase[] { new PrinterServiceHost() };
                ServiceBase.Run(servicestorun);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("⚠️ Assembly not found: " + args.Name);
            return null;
        }

        private static bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public static class Logger
    {
        private static log4net.ILog current;
        public static log4net.ILog Current
        {
            get
            {
                if (current != null) return current;
                current = log4net.LogManager.GetLogger(typeof(Logger));
                return current;
            }
        }
    }

    public static class SelfInstaller
    {
        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;
        public static bool InstallMe()
        {
            try
            {
                var file = _exePath;
                var filename = string.Join("\\",
                    file.Split('\\').ToList().ConvertAll(e => e.StartsWith(".") ? e.Substring(1) : e));
                ManagedInstallerClass.InstallHelper(
                    new string[] { filename });
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                return false;
            }
            return true;
        }

        public static bool UninstallMe()
        {
            try
            {
                var file = _exePath;
                var filename = string.Join("\\",
                    file.Split('\\').ToList().ConvertAll(e => e.StartsWith(".") ? e.Substring(1) : e));
                ManagedInstallerClass.InstallHelper(
                    new string[] { "/u", filename });
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                return false;
            }
            return true;
        }
    }
}
