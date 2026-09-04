using Quanto.Offline;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace Quanto
{
    public class Program
    {
        private static readonly ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
                waitHandle.Wait();
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
                    var processInfo = new ProcessStartInfo(Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location)
                    {
                        UseShellExecute = true,
                        Verb = "runas",
                    };
                    try
                    {
                        Process.Start(processInfo);
                        return;
                    }
                    catch (Exception)
                    {
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
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new OfflineApplicationContext());
                }
                else
                {
                    DialogResult dr = MessageBox.Show("Do you REALLY like to install the " + SERVICE_NAME + "?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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
                ServiceBase[] servicestorun = new ServiceBase[] { new PrinterServiceHost() };
                ServiceBase.Run(servicestorun);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Assembly not found: " + args.Name);
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
        private static readonly string _exePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
        private const string ServiceName = "Quanto-Client-Service";

        public static bool InstallMe()
        {
            try
            {
                RunSc($"create \"{ServiceName}\" binPath= \"\\\"{_exePath}\\\"\" start= auto DisplayName= \"{ServiceName}\"");
                RunSc($"start \"{ServiceName}\"");
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
                RunSc($"stop \"{ServiceName}\"");
                RunSc($"delete \"{ServiceName}\"");
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                return false;
            }
            return true;
        }

        private static void RunSc(string arguments)
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process?.WaitForExit();
        }
    }
}
