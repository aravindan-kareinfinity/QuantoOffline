using log4net;
using log4net.Appender;
using Quanto.BI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Quanto
{
    partial class PrinterServiceHost : ServiceBase
    {
        public PrinterServiceHost()
        {
            InitializeComponent();
        }

        private System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
        protected override void OnStart(string[] args)
        {
         
            try
            {
                AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Logger.Current.Info("Initializing started..");
                mainThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Start));
                mainThread.Start(false);


                

                Logger.Current.Info("initialization done");
            }
            catch (Exception exp)
            {
                Logger.Current.Error("Initialization failed", exp);
            }
        }


        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception.Message.Contains(".XmlSerializers"))
                return;
            Logger.Current.Error("First Chance Exception", e.Exception as Exception);
        }


        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Current.Error("Unhandled Exception", e.ExceptionObject as Exception);
        }

        HostingConfiguration SERVICE;

        /// <summary>In-process desktop instance (WinForms + Kestrel in same EXE).</summary>
        public static PrinterServiceHost InProcessInstance { get; private set; }

        /// <summary>Legacy fire-and-forget start used by Windows Service OnStart.</summary>
        public void Start()
        {
            mainThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Start));
            mainThread.IsBackground = true;
            mainThread.Start(false);
        }

        /// <summary>
        /// Starts Kestrel on a background thread and waits until listen succeeds or fails.
        /// Used by normal desktop/client mode (same process as WinForms).
        /// </summary>
        public bool StartInProcess()
        {
            InProcessInstance = this;
            bool started = false;
            Exception error = null;
            using (var done = new ManualResetEventSlim(false))
            {
                mainThread = new Thread(() =>
                {
                    try
                    {
                        Logger.Current.Info("Starting in-process local API...");
                        SERVICE = new HostingConfiguration();
                        started = SERVICE.Start();
                    }
                    catch (Exception exp)
                    {
                        error = exp;
                        started = false;
                        Logger.Current.Error("In-process local API start failed", exp);
                    }
                    finally
                    {
                        done.Set();
                    }
                });
                mainThread.IsBackground = true;
                mainThread.Name = "QuantoLocalApi";
                mainThread.Start();

                if (!done.Wait(TimeSpan.FromSeconds(45)))
                {
                    Logger.Current.Error("In-process local API start timed out");
                    return false;
                }
            }

            if (error != null || !started)
                return false;

            if (System.Configuration.ConfigurationManager.AppSettings["AppMode"] == "BI" &&
                !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["BIDownloadTimer"]))
            {
                Quanto.BI.BIDownloader bIDownloader = new BI.BIDownloader();
                biThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(bIDownloader.Start));
                biThread.IsBackground = true;
                biThread.Start(false);
                Logger.Current.Info("BIDownloadTimer Started");
            }

            return true;
        }

        public void StopInProcess()
        {
            try
            {
                SERVICE?.Stop();
            }
            catch (Exception exp)
            {
                Logger.Current.Error("In-process local API stop failed", exp);
            }
            finally
            {
                SERVICE = null;
                if (ReferenceEquals(InProcessInstance, this))
                    InProcessInstance = null;
            }
        }

        public void Start(object startUI)
        {
            System.Threading.Thread.Sleep(1000);
            try
            {
                Logger.Current.Info("Starting...");

                System.Threading.Thread.Sleep(1000);
                SERVICE = new HostingConfiguration();
                SERVICE.Start();

                if(System.Configuration.ConfigurationManager.AppSettings["AppMode"] == "BI" &&
                   !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["BIDownloadTimer"]))
                {
                    Quanto.BI.BIDownloader bIDownloader = new BI.BIDownloader();
                    biThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(bIDownloader.Start));
                    biThread.Start(false);
                    Logger.Current.Info("BIDownloadTimer Started");
                }

                if ((bool)startUI)
                {
                    Logger.Current.Info("Launching notification");
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new OfflineApplicationContext());
                    Logger.Current.Info("Launching completed");
                }
            }
            catch(Exception exp)
            {
                Logger.Current.Error("TA service down",exp);
            }
        }

        System.Threading.Thread mainThread;
        System.Threading.Thread biThread;

        public ManualResetEvent Locker
        {
            get
            {
                return mre;
            }

            set
            {
                mre = value;
            }
        }

        protected override void OnStop()
        {
            try
            {
                Locker.Set();
                Logger.Current.Info("Shutting down sms service");
                SERVICE?.Stop();
                SERVICE = null;
                Logger.Current.Info("TA service down");
            }
            catch(Exception exp)
            {
                Logger.Current.Error("TA service down", exp); ;
            }
        }

        public class HostingConfiguration
        {
            public List<string> LocalIPAddress()
            {
                List<string> localIP = new List<string>();
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["LocalIP"]))
                {
                    if (System.Configuration.ConfigurationManager.AppSettings["LocalIP"].Split('.').Length == 4)
                    {
                        localIP.Add(System.Configuration.ConfigurationManager.AppSettings["LocalIP"]);
                        return localIP;
                    }
                }
                IPHostEntry host;
                
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    Logger.Current.InfoFormat("Machine IP Address List {0} - {1}", ip.AddressFamily, ip.ToString());
                }
                foreach (IPAddress ip in host.AddressList)
                {

                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP.Add(ip.ToString());
                    }
                }
                return localIP;
            }

            private IDisposable _webapplication;
            private IDisposable _webapplicationIPBased;

            private static string GetConfiguredListenUrl()
            {
                return MachineConfig.ListenUrl;
            }

            private static List<string> BuildFixedLocalUrls(string listenUrl)
            {
                // Prefer a single bind URL. http://0.0.0.0:8099 serves localhost + LAN.
                return new List<string> { listenUrl };
            }

            public bool Start()
            {
                MachineConfig.EnsureInitialized();

                string hostname = Dns.GetHostName();
                List<string> localhostname = NetworkInfo.GetPrivateIPv4Addresses();
                if (localhostname.Count == 0)
                    localhostname = LocalIPAddress();
                Logger.Current.InfoFormat("Machine Name {0}", hostname);
                Logger.Current.InfoFormat("Machine IP {0}", string.Join(",", localhostname));
                Logger.Current.InfoFormat("MachineRole={0} DeviceId={1}", MachineConfig.MachineRole, MachineConfig.DeviceId);

                int activeport = MachineConfig.ApiPort;
                string listenUrl = GetConfiguredListenUrl();
                if (Uri.TryCreate(listenUrl, UriKind.Absolute, out var listenUri))
                    activeport = listenUri.Port;

                if (!FirewallHelper.EnsureApiPortAllowed(activeport, out var fwMessage))
                    Logger.Current.Error("Firewall: " + fwMessage);
                else
                    Logger.Current.Info("Firewall: " + fwMessage);

                try
                {
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["PrintOnSSLPort"])
                        && System.Configuration.ConfigurationManager.AppSettings["PrintOnSSLPort"].ToLower() == "true")
                    {
                        int i = 4443;
                        activeport = i;
                        var urls = new List<string>
                        {
                            string.Format("https://{0}:{1}", hostname, i),
                            string.Format("https://{0}:{1}", "localhost", i),
                            string.Format("https://{0}:{1}", "127.0.0.1", i),
                        };
                        foreach (var lhn in localhostname)
                            urls.Add(string.Format("https://{0}:{1}", lhn, i));

                        _webapplication = AspNetCoreHost.Start(urls);

                        foreach (var url in urls)
                            Logger.Current.InfoFormat("Service URL Available on {0}", url);
                    }
                    else if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["PrintOn80Port"])
                        && System.Configuration.ConfigurationManager.AppSettings["PrintOn80Port"].ToLower() == "true")
                    {
                        int i = 80;
                        activeport = i;
                        var urls = new List<string>
                        {
                            string.Format("http://{0}:{1}", hostname, i),
                            string.Format("http://{0}:{1}", "localhost", i),
                            string.Format("http://{0}:{1}", "127.0.0.1", i),
                        };
                        foreach (var lhn in localhostname)
                            urls.Add(string.Format("http://{0}:{1}", lhn, i));

                        _webapplication = AspNetCoreHost.Start(urls);

                        foreach (var url in urls)
                            Logger.Current.InfoFormat("Service URL Available on {0}", url);
                    }
                    else
                    {
                        // Fixed ListenUrl (default http://0.0.0.0:8099). Fail clearly if occupied.
                        var urls = BuildFixedLocalUrls(listenUrl);
                        _webapplication = AspNetCoreHost.Start(urls);
                        foreach (var url in urls)
                            Logger.Current.InfoFormat("Service URL Available on {0}", url);
                        foreach (var ip in localhostname)
                            Logger.Current.InfoFormat("LAN URL Available on http://{0}:{1}", ip, activeport);
                        Logger.Current.InfoFormat("Local URL Available on http://localhost:{0}", activeport);
                    }
                }
                catch (Exception exp)
                {
                    Logger.Current.Error(
                        "Failed to start Kestrel on " + listenUrl + ". Port may already be in use.", exp);
                    _webapplication = null;
                    return false;
                }

                if (MachineConfig.IsMaster)
                    MasterDiscovery.StartResponder();
                if (MachineConfig.IsClient)
                    MasterConnectionMonitor.Start();

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnablePrintServer"]) &&
                            System.Configuration.ConfigurationManager.AppSettings["EnablePrintServer"].ToLower() == "true")
                {
                    Quanto.Logger.Current.Info("print service enabled...");
                    foreach (var ip in localhostname)
                    {
                        var result = Quanto.ServiceProxy.Instance.AddPrinter(new PrinterConfig()
                        {
                            companycode = System.Configuration.ConfigurationManager.AppSettings["companycode"],
                            locationcode = System.Configuration.ConfigurationManager.AppSettings["locationcode"],
                            port = activeport.ToString(),
                            server = hostname,
                            computername = hostname,
                            ipaddress = ip
                        });
                    }
                }
                else
                {
                    Quanto.Logger.Current.Info("print service disabled...");
                }

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableSMSServer"]) &&
                    System.Configuration.ConfigurationManager.AppSettings["EnableSMSServer"].ToLower() == "true")
                {
                    Quanto.Logger.Current.Info("sms service enabled...");
                    Quanto.SMS.SMSManager.Instance.Start();
                }
                else
                {
                    Quanto.Logger.Current.Info("sms service disabled...");
                }

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableKeepAlive"]) &&
                   System.Configuration.ConfigurationManager.AppSettings["EnableKeepAlive"].ToLower() == "true")
                {
                    Quanto.Logger.Current.Info("Keep alive service enabled...");
                    Quanto.KeepAlive.Instance.Start();
                }
                else
                {
                    Quanto.Logger.Current.Info("Keep alive service disabled...");
                }

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableOffline"]) &&
                   System.Configuration.ConfigurationManager.AppSettings["EnableOffline"].ToLower() == "true")
                {
                    Quanto.Logger.Current.Info("Scheduled master download enabled (EnableOffline)...");
                    Quanto.OfflineSync.Instance.Start();
                }
                else
                {
                    Quanto.Logger.Current.Info("Offline service disabled...");
                }

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableSchedule"]))
                {
                    Quanto.SMS.ServiceTimer.Instance.Start();
                }

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableDataManager"]))
                {
                    Quanto.Data.DataManager.Instance.Start();
                }

                return true;
            }

            public bool Stop()
            {
                try
                {
                    MasterConnectionMonitor.Stop();
                    MasterDiscovery.StopResponder();

                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableSMSServer"]) &&
                                System.Configuration.ConfigurationManager.AppSettings["EnableSMSServer"].ToLower() == "true")
                    {
                        Quanto.SMS.SMSManager.Instance.Stop();
                    }

                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableKeepAlive"]) &&
                       System.Configuration.ConfigurationManager.AppSettings["EnableKeepAlive"].ToLower() == "true")
                    {
                        Quanto.KeepAlive.Instance.Stop();
                    }

                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableDataManager"]))
                    {
                        Quanto.Data.DataManager.Instance.Stop();
                    }

                    _webapplication?.Dispose();
                }
                finally
                {
                    _webapplication = null;
                }
                return true;
            }
        }
    }
}
