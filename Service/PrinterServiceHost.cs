using log4net;
using log4net.Appender;
using Microsoft.Owin.Hosting;
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
        public void Start()
        {
            mainThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Start));
            mainThread.Start(false);
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
                    //var filename = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    //Logger.Current.Error("Launching offline win app", new Exception(filename));
                    //ProcessStartInfo psi = new ProcessStartInfo(filename, "offline");
                    //psi.UseShellExecute = true;
                    //Process.Start(psi);
                    //Logger.Current.Error("offline win app started", new Exception(filename));
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
                SERVICE.Stop();
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

            public bool Start()
            {

                //string path = "";
                ////Logger.Current = log4net.LogManager.GetLogger(typeof(HostingConfiguration));
                ////Logger.Current.Info("Starting");
                //foreach (var appender in LogManager.GetRepository().GetAppenders())
                //{
                //    var fileAppender = appender as FileAppender;
                //    if (fileAppender != null)
                //    {
                //        path = fileAppender.File;
                //    }
                //}

                //System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //var AutoLog = System.IO.Path.Combine(fi.Directory.FullName, "AutoLog.txt");
                //System.IO.File.WriteAllText(AutoLog, "Existing path :" + path + " new Path" + AutoLog);

                string hostname = Dns.GetHostName();
                List<string> localhostname = LocalIPAddress();
                Logger.Current.InfoFormat("Machine Name {0}", hostname);
                Logger.Current.InfoFormat("Machine IP {0}", string.Join(",", localhostname));
                int activeport = 80;
                bool installed = false;

                //try
                //{
                //    StartOptions startOptions = new StartOptions();
                //    startOptions.Urls.Add(string.Format("http://{0}:{1}", hostname, i));
                //    startOptions.Urls.Add("https://+:443");
                //    _webapplication = WebApp.Start<OwinCofiguration>(startOptions);
                //    installed = true;
                //}
                //catch (Exception httpexp)
                //{

                //}

                if (!installed)
                {
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["PrintOnSSLPort"])
                        && System.Configuration.ConfigurationManager.AppSettings["PrintOnSSLPort"].ToLower() == "true")
                    {
                        int i = 4443;
                        StartOptions startOptions = new StartOptions();
                        startOptions.Urls.Add(string.Format("https://{0}:{1}", hostname, i));
                        startOptions.Urls.Add(string.Format("https://{0}:{1}", "localhost", i));
                        startOptions.Urls.Add(string.Format("https://{0}:{1}", "127.0.0.1", i));
                        foreach (var lhn in localhostname)
                            startOptions.Urls.Add(string.Format("http://{0}:{1}", lhn, i));

                        _webapplication = WebApp.Start<OwinConfiguration>(startOptions);

                        foreach (var url in startOptions.Urls)
                        {
                            Logger.Current.InfoFormat("Service URL Available on {0}", url);
                        }
                    }
                    else if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["PrintOn80Port"]) 
                        && System.Configuration.ConfigurationManager.AppSettings["PrintOn80Port"].ToLower() == "true")
                    {
                        int i = 80;
                        StartOptions startOptions = new StartOptions();
                        startOptions.Urls.Add(string.Format("http://{0}:{1}", hostname, i));
                        startOptions.Urls.Add(string.Format("http://{0}:{1}", "localhost", i));
                        startOptions.Urls.Add(string.Format("http://{0}:{1}", "127.0.0.1", i));
                        foreach(var lhn in localhostname)
                            startOptions.Urls.Add(string.Format("http://{0}:{1}", lhn, i));

                        _webapplication = WebApp.Start<OwinConfiguration>(startOptions);

                        foreach (var url in startOptions.Urls)
                        {
                            Logger.Current.InfoFormat("Service URL Available on {0}", url);
                        }
                    }
                    else
                    {

                        for (int i = 8099; i < 9010; i++)
                        {
                            try
                            {
                                activeport = i;

                                StartOptions startOptions = new StartOptions();
                                startOptions.Urls.Add(string.Format("http://{0}:{1}", hostname, i));
                                startOptions.Urls.Add(string.Format("http://{0}:{1}", "localhost", i));
                                startOptions.Urls.Add(string.Format("http://{0}:{1}", "127.0.0.1", i));
                                foreach (var lhn in localhostname)
                                    startOptions.Urls.Add(string.Format("http://{0}:{1}", lhn, i));
                                _webapplication = WebApp.Start<OwinConfiguration>(startOptions);

                                foreach (var url in startOptions.Urls)
                                {
                                    Logger.Current.InfoFormat("Service URL Available on {0}", url);
                                }

                                //string url = string.Format("http://{0}:{1}", hostname, i);
                                //Logger.Current.InfoFormat("Service URL trying to host on {0}", url);

                                //_webapplication = WebApp.Start<OwinCofiguration>(url);
                                //Logger.Current.InfoFormat("Service URL Available on {0}", url);

                                //string ipurl = string.Format("http://{0}:{1}", localhostname, i + 1);
                                //Logger.Current.InfoFormat("Service URL trying to host on {0}", ipurl);
                                //_webapplicationIPBased = WebApp.Start<OwinCofiguration>(ipurl);
                                //Logger.Current.InfoFormat("Service URL Available on {0}", ipurl);
                                break;
                            }
                            catch (Exception exp)
                            {
                                if (exp.InnerException.Message.Contains("Access is denied"))
                                {
                                    if (hostname == "localhost")
                                        break;
                                    hostname = "localhost";
                                    i--;
                                }
                                Logger.Current.Error(exp);
                            }
                        }

                    }

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
                        Quanto.Logger.Current.Info("Offline service enabled...");
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
                }

                return true;
            }
            public bool Stop()
            {
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
                _webapplication.Dispose();
                return true;
            }
        }
    }
}
