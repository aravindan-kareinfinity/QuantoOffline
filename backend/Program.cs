using Quanto.Offline;
using System;
using System.Diagnostics;
using System.Reflection;
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
                StartDesktop();
                return;
            }
            else
            {
                try
                {
                    ServiceController[] services = ServiceController.GetServices();
                    foreach (ServiceController service in services)
                    {
                        if (service.ServiceName.Equals(SERVICE_NAME) &&
                            service.Status == ServiceControllerStatus.StartPending)
                        {
                            serviceStarting = true;
                            break;
                        }
                    }
                }
                catch
                {
                    serviceStarting = false;
                }

                if (serviceStarting)
                {
                    ServiceBase[] servicestorun = new ServiceBase[] { new PrinterServiceHost() };
                    ServiceBase.Run(servicestorun);
                    return;
                }

                StartDesktop();
            }
        }

        /// <summary>
        /// Setup (if needed) → Kestrel → tray UI, same process.
        /// </summary>
        private static void StartDesktop()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // First-run / incomplete config must complete BEFORE role-dependent services start.
            if (!MachineConfig.IsConfigured)
            {
                using (var setup = new SetupWizardForm(SetupWizardForm.WizardMode.FirstRun))
                {
                    if (setup.ShowDialog() != DialogResult.OK || !MachineConfig.IsConfigured)
                    {
                        MessageBox.Show(
                            "Setup was not completed. Quanto.Client will exit.",
                            "Quanto Client Setup",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            MachineConfig.EnsureInitialized();
            OfflineSync.StartIfConfigured();

            Console.Write("Starting");
            PrinterServiceHost hc = new PrinterServiceHost();
            if (!hc.StartInProcess())
            {
                var listenUrl = MachineConfig.ListenUrl;
                MessageBox.Show(
                    "Failed to start the local API on " + listenUrl + ".\n\n" +
                    "The port may already be in use. Close the other application using that port, then try again.\n\n" +
                    "The application will continue. Cloud master download still runs on schedule.",
                    "Quanto.Client",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            try
            {
                Application.ApplicationExit += (s, e) =>
                {
                    OfflineSync.StopIfConfigured();
                    hc.StopInProcess();
                };
                Application.Run(new OfflineApplicationContext());
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Desktop application failed", exp);
                Console.Write(exp);
            }
            finally
            {
                OfflineSync.StopIfConfigured();
                hc.StopInProcess();
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Assembly not found: " + args.Name);
            return null;
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
